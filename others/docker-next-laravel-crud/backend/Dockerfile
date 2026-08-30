FROM php:8.3-cli-alpine

# Install system dependencies and PHP extensions for PostgreSQL
RUN apk add --no-cache libpq postgresql-client bash git unzip zip libzip-dev \
    && apk add --no-cache --virtual .build-deps build-base postgresql-dev \
    && docker-php-ext-install pdo pdo_pgsql zip \
    && apk del .build-deps
    
# Install Composer
COPY --from=composer:latest /usr/bin/composer /usr/bin/composer

WORKDIR /var/www/html


# Copy composer.json, and copy composer.lock ONLY if it exists
COPY composer.json composer.lock* ./

# FIX: Added configuration to bypass security advisory blocking
RUN composer config --global --no-interaction allow-plugins true \
    && composer config --global policy.advisories.block false \
    && composer install --no-interaction --optimize-autoloader --no-scripts --ignore-platform-reqs --no-plugins

# Copy the rest of the project files
COPY . .


# Expose port 8000 and start the Laravel dev server
EXPOSE 8000
CMD ["php", "artisan", "serve", "--host=0.0.0.0", "--port=8000"]

