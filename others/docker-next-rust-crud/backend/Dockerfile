# --- Stage 1: Cargo Compilation Cache Environment ---
# Using the latest alpine toolchain to fully satisfy the v1.88+ package requirements
FROM rust:alpine AS builder

# Install core compiler build-tools required for compiling native Rust dependencies on Alpine
RUN apk add --no-cache musl-dev libressl-dev pkgconfig

WORKDIR /usr/src/app

# Copy dependency manifests first
COPY Cargo.toml Cargo.lock* ./

# Build a dummy application to cache dependencies 
RUN mkdir src && echo "fn main() {}" > src/main.rs
RUN cargo build --release
RUN rm -f target/release/deps/backend*

# Copy your actual production source files
COPY src ./src

# Compile the final release binary
RUN cargo build --release

# --- Stage 2: Minimal Execution Runtime Environment ---
FROM alpine:3.20

RUN apk add --no-cache ca-certificates libgcc

WORKDIR /app

COPY --from=builder /usr/src/app/target/release/backend /app/backend

EXPOSE 5000

CMD ["./backend"]

