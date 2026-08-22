FROM node:22-alpine

ENV RABBITMQ_URL="amqp://admin:supersecret@localhost:5672"

WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

# Standard command to run your long-running consumer script
CMD ["node", "index.ts"]
