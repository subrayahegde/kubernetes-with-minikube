FROM ruby:3.4-alpine

# Install dependencies
RUN apk add --no-cache \
  build-base \
  postgresql-dev \
  nodejs \
  npm \
  yarn \
  tzdata \
  git \
  bash

WORKDIR /app

ENV BUNDLE_PATH=/gems
ENV BUNDLE_BIN=/gems/bin
ENV PATH=$BUNDLE_BIN:$PATH

COPY Gemfile ./
RUN bundle config set path /gems && bundle config set without 'production' && bundle install --jobs 4 --retry 3

COPY . .

RUN mkdir -p tmp/pids tmp/cache

EXPOSE 5000

CMD ["/bin/sh", "-c", "mkdir -p tmp/pids && rm -f tmp/pids/server.pid && bundle exec puma -b tcp://0.0.0.0:5000 config.ru"]
