package main

import (
	"context"
	"encoding/json"
	"fmt"
	"log"
	"net/http"
	"os"
    "strings"
	"time"

	"github.com/jackc/pgx/v5/pgxpool"
	"github.com/rs/cors"
)

type Product struct {
	ID          int64     `json:"id"`
	Name        string    `json:"name"`
	Description string    `json:"description"`
	Price       float64   `json:"price"`
	CreatedAt   time.Time `json:"created_at"`
}

var dbPool *pgxpool.Pool

func main() {
	// 1. Get Database URL from environment variables
	dbURL := os.Getenv("DB_CONN")
	if dbURL == "" {
		dbURL = "postgres://postgres:Pass123@database:5432/clidb?sslmode=disable"
	}

	// 2. Connect to PostgreSQL with retry logic
	var err error
	for i := 0; i < 5; i++ {
		dbPool, err = pgxpool.New(context.Background(), dbURL)
		if err == nil {
			err = dbPool.Ping(context.Background())
			if err == nil {
				break
			}
		}
		log.Printf("Waiting for database connection... retry %d/5", i+1)
		time.Sleep(2 * time.Second)
	}

	if err != nil {
		log.Fatalf("Unable to connect to database: %v\n", err)
	}
	defer dbPool.Close()

	// 3. Automatically create schema if it doesn't exist
	ensureTableExists()

	// 4. Clean, isolated explicit routes
	mux := http.NewServeMux()
	
	// Exact match for listing all items and adding items (NO trailing slash)
	mux.HandleFunc("/api/products", productsCollectionHandler)
	
	// Prefix match for dealing with item IDs (WITH trailing slash)
	mux.HandleFunc("/api/products/", productItemHandler)

    c := cors.New(cors.Options{
        AllowedOrigins:   []string{"*"},          
        AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"}, // Explicitly include DELETE
        AllowedHeaders:   []string{"Content-Type", "Authorization"},
        AllowCredentials: true,
    })

	// 5. Enable CORS for your Next.js frontend
	handler := c.Handler(mux)

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	fmt.Printf("Backend server starting on port %s...\n", port)
	log.Fatal(http.ListenAndServe(":"+port, handler))
}

func ensureTableExists() {
	query := `
	CREATE TABLE IF NOT EXISTS products (
		id BIGSERIAL PRIMARY KEY,
		name TEXT NOT NULL,
		description TEXT NOT NULL,
		price NUMERIC(10, 2) NOT NULL,
		created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP
	);`
	_, err := dbPool.Exec(context.Background(), query)
	if err != nil {
		log.Fatalf("Failed to create table: %v\n", err)
	}
}

// 1. Handles /api/products (GET list, POST add)
func productsCollectionHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	// Ensure exact path match to prevent accidental fallback matching
	if r.URL.Path != "/api/products" {
		w.WriteHeader(http.StatusNotFound)
		json.NewEncoder(w).Encode(map[string]string{"error": "Resource not found"})
		return
	}

	switch r.Method {
	case http.MethodGet:
		rows, err := dbPool.Query(context.Background(), "SELECT id, name, description, price, created_at FROM products ORDER BY id DESC")
		if err != nil {
			w.WriteHeader(http.StatusInternalServerError)
			json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
			return
		}
		defer rows.Close()

		products := []Product{}
		for rows.Next() {
			var p Product
			err := rows.Scan(&p.ID, &p.Name, &p.Description, &p.Price, &p.CreatedAt)
			if err != nil {
				w.WriteHeader(http.StatusInternalServerError)
				json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
				return
			}
			products = append(products, p)
		}
		json.NewEncoder(w).Encode(products)

	case http.MethodPost: // ADD FUNCTION
		var p Product
		if err := json.NewDecoder(r.Body).Decode(&p); err != nil {
			w.WriteHeader(http.StatusBadRequest)
			json.NewEncoder(w).Encode(map[string]string{"error": "Invalid request payload"})
			return
		}

		query := `
			INSERT INTO products (name, description, price) 
			VALUES ($1, $2, $3) 
			RETURNING id, created_at`
		
		err := dbPool.QueryRow(context.Background(), query, p.Name, p.Description, p.Price).Scan(&p.ID, &p.CreatedAt)
		if err != nil {
			w.WriteHeader(http.StatusInternalServerError)
			json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
			return
		}

		w.WriteHeader(http.StatusCreated)
		json.NewEncoder(w).Encode(p)

	default:
		w.WriteHeader(http.StatusMethodNotAllowed)
		json.NewEncoder(w).Encode(map[string]string{"error": "Method not allowed"})
	}
}

// 2. Handles /api/products/{id} (DELETE, single item GET)
func productItemHandler(w http.ResponseWriter, r *http.Request) {
	w.Header().Set("Content-Type", "application/json")

	// Parse out the ID (e.g. "/api/products/42" -> "42")
	idStr := strings.TrimPrefix(r.URL.Path, "/api/products/")
	idStr = strings.TrimSpace(idStr)

	if idStr == "" {
		w.WriteHeader(http.StatusBadRequest)
		json.NewEncoder(w).Encode(map[string]string{"error": "Missing product ID"})
		return
	}

	switch r.Method {
	case http.MethodDelete: // DELETE FUNCTION
		query := "DELETE FROM products WHERE id = $1"
		commandTag, err := dbPool.Exec(context.Background(), query, idStr)
		if err != nil {
			w.WriteHeader(http.StatusInternalServerError)
			json.NewEncoder(w).Encode(map[string]string{"error": err.Error()})
			return
		}

		if commandTag.RowsAffected() == 0 {
			w.WriteHeader(http.StatusNotFound)
			json.NewEncoder(w).Encode(map[string]string{"error": "Product not found"})
			return
		}

		w.WriteHeader(http.StatusOK)
		json.NewEncoder(w).Encode(map[string]string{"message": "Product successfully deleted"})

	case http.MethodGet:
		var p Product
		query := "SELECT id, name, description, price, created_at FROM products WHERE id = $1"
		err := dbPool.QueryRow(context.Background(), query, idStr).Scan(&p.ID, &p.Name, &p.Description, &p.Price, &p.CreatedAt)
		if err != nil {
			w.WriteHeader(http.StatusNotFound)
			json.NewEncoder(w).Encode(map[string]string{"error": "Product not found"})
			return
		}
		json.NewEncoder(w).Encode(p)

	default:
		w.WriteHeader(http.StatusMethodNotAllowed)
		json.NewEncoder(w).Encode(map[string]string{"error": "Method not allowed"})
	}
}
