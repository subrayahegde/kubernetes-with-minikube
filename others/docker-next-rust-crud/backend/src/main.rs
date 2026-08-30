use axum::{
    extract::{Path, State},
    http::{StatusCode, Method, header},
    routing::{get, post},
    Json, Router,
};
use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use sqlx::{postgres::PgPoolOptions, PgPool};
use std::env;
use std::net::SocketAddr;
use tower_http::cors::CorsLayer;
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt};

// --- DATA STRUCTURES ---

// Matches your PostgreSQL "products" table layout
#[derive(Serialize, Deserialize, sqlx::FromRow)]
struct Product {
    id: i64,
    name: String,
    description: Option<String>,
    price: String, // Pulling database numeric values securely into string format
    created_at: Option<DateTime<Utc>>,
}

// Payload schema mapping for incoming JSON requests
#[derive(Deserialize)]
struct CreateProductInput {
    name: String,
    description: Option<String>,
    // Added custom deserializer attribute to support both integers/floats and strings from frontend
    #[serde(deserialize_with = "deserialize_price_flexible")]
    price: String, 
}

// Custom flexible parsing helper for the price field
fn deserialize_price_flexible<'de, D>(deserializer: D) -> Result<String, D::Error>
where
    D: serde::Deserializer<'de>,
{
    #[derive(Deserialize)]
    #[serde(untagged)]
    enum Helper {
        Number(f64),
        String(String),
    }

    match Helper::deserialize(deserializer)? {
        Helper::Number(num) => Ok(num.to_string()),
        Helper::String(s) => Ok(s),
    }
}

// --- MAIN RUNTIME ENTRYPOINT ---

#[tokio::main]
async fn main() {
    // Initialize structured logs
    tracing_subscriber::registry()
        .with(tracing_subscriber::fmt::layer())
        .init();

    let port: u16 = env::var("PORT")
        .unwrap_or_else(|_| "5000".to_string())
        .parse()
        .expect("PORT environment variable must be a valid number");

    let database_url = env::var("DATABASE_URL")
        .unwrap_or_else(|_| "postgres://postgres:Password123@host.minikube.internal:5432/clidb".to_string());

    // Connection pool initializer
    let pool = PgPoolOptions::new()
        .max_connections(5)
        .connect(&database_url)
        .await
        .expect("CRITICAL: Failed to establish a database connection pool");

    tracing::info!("Connected to PostgreSQL database successfully.");

/*
    // Setup secure CORS rules for frontend ports
    let cors = CorsLayer::new()
        .allow_origin([
            "http://localhost:3000".parse().unwrap(),
            "http://127.0.0.1:3000".parse().unwrap(),
            "http://10.97.239.218:3000".parse().unwrap() 
        ])
        .allow_methods([Method::GET, Method::POST, Method::PUT, Method::DELETE, Method::OPTIONS])
        .allow_headers([header::CONTENT_TYPE, header::AUTHORIZATION]);
*/
   // Configure CORS to allow everything
    let cors = CorsLayer::new()
         .allow_origin(tower_http::cors::any())
         .allow_methods(tower_http::cors::any())
         .allow_headers(tower_http::cors::any());


    // Define Application Routing Configuration
    let app = Router::new()
        .route("/api/products", post(create_product).get(list_products))
        .route("/api/products/:id", get(get_product).put(update_product).delete(delete_product))
        .layer(cors)
        .with_state(pool);

    // Bind on internal docker network interfaces
    let addr = SocketAddr::from(([0, 0, 0, 0], port));
    tracing::info!("Rust backend server booting up with CORS enabled on environment port: {}", port);
    
    let listener = tokio::net::TcpListener::bind(&addr).await.unwrap();
    axum::serve(listener, app).await.unwrap();
}

// --- API CRUD ROUTE HANDLERS ---

// Create Product (POST /api/products)
async fn create_product(
    State(pool): State<PgPool>,
    Json(payload): Json<CreateProductInput>,
) -> Result<(StatusCode, Json<Product>), (StatusCode, String)> {
    // Cast the inbound string string directly to numeric inside Postgres 
    let product = sqlx::query_as::<_, Product>(
        "INSERT INTO products (name, description, price) VALUES ($1, $2, $3::NUMERIC) RETURNING id, name, description, price::TEXT, created_at"
    )
    .bind(payload.name)
    .bind(payload.description)
    .bind(payload.price)
    .fetch_one(&pool)
    .await
    .map_err(|e| (StatusCode::INTERNAL_SERVER_ERROR, e.to_string()))?;

    Ok((StatusCode::CREATED, Json(product)))
}

// List All Products (GET /api/products)
async fn list_products(
    State(pool): State<PgPool>,
) -> Result<Json<Vec<Product>>, (StatusCode, String)> {
    let products = sqlx::query_as::<_, Product>("SELECT id, name, description, price::TEXT, created_at FROM products ORDER BY id DESC")
        .fetch_all(&pool)
        .await
        .map_err(|e| (StatusCode::INTERNAL_SERVER_ERROR, e.to_string()))?;

    Ok(Json(products))
}

// Get Single Product by ID (GET /api/products/:id)
async fn get_product(
    Path(id): Path<i32>,
    State(pool): State<PgPool>,
) -> Result<Json<Product>, (StatusCode, String)> {
    let product = sqlx::query_as::<_, Product>("SELECT id, name, description, price::TEXT, created_at FROM products WHERE id = $1")
        .bind(id)
        .fetch_optional(&pool)
        .await
        .map_err(|e| (StatusCode::INTERNAL_SERVER_ERROR, e.to_string()))?;

    match product {
        Some(p) => Ok(Json(p)),
        None => Err((StatusCode::NOT_FOUND, "Product not found".to_string())),
    }
}

// Update Product (PUT /api/products/:id)
async fn update_product(
    Path(id): Path<i32>,
    State(pool): State<PgPool>,
    Json(payload): Json<CreateProductInput>,
) -> Result<Json<Product>, (StatusCode, String)> {
    let product = sqlx::query_as::<_, Product>(
        "UPDATE products SET name = $1, description = $2, price = $3::NUMERIC WHERE id = $4 RETURNING id, name, description, price::TEXT, created_at"
    )
    .bind(payload.name)
    .bind(payload.description)
    .bind(payload.price)
    .bind(id)
    .fetch_optional(&pool)
    .await
    .map_err(|e| (StatusCode::INTERNAL_SERVER_ERROR, e.to_string()))?;

    match product {
        Some(p) => Ok(Json(p)),
        None => Err((StatusCode::NOT_FOUND, "Product not found to update".to_string())),
    }
}

// Delete Product (DELETE /api/products/:id)
async fn delete_product(
    Path(id): Path<i32>,
    State(pool): State<PgPool>,
) -> Result<StatusCode, (StatusCode, String)> {
    let rows_affected = sqlx::query("DELETE FROM products WHERE id = $1")
        .bind(id)
        .execute(&pool)
        .await
        .map_err(|e| (StatusCode::INTERNAL_SERVER_ERROR, e.to_string()))?
        .rows_affected();

    if rows_affected == 0 {
        return Err((StatusCode::NOT_FOUND, "Product not found to delete".to_string()));
    }

    Ok(StatusCode::NO_CONTENT)
}

