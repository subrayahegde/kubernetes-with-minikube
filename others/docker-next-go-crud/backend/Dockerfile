# Stage 1: Build the Go binary
FROM golang:1.25-alpine AS builder

WORKDIR /app

# Copy dependency definition files
COPY go.mod ./
# If go.sum exists locally, uncomment the next line:
# COPY go.sum ./  

# Copy your source code now so 'go mod tidy' can scan it
COPY . .

# Tidy the module file to automatically resolve and add missing dependencies
RUN go mod tidy

# Download dependencies based on the updated module definitions
RUN go mod download

# Build a statically linked binary
RUN CGO_ENABLED=0 GOOS=linux go build -o main .


# Stage 2: Run the binary
FROM alpine:latest  
WORKDIR /root/
# Copy the binary from the builder stage
COPY --from=builder /app/main .
# Expose your backend port (change 8080 to your actual port)
EXPOSE 8080  
# Run the binary
CMD ["./main"]
