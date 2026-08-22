
FROM node:22-alpine

WORKDIR /app
ENV RABBITMQ_URL="amqp://admin:supersecret@localhost:5672"

COPY package*.json ./
RUN npm install
COPY . .

RUN npm run build
EXPOSE 3000

CMD ["npm", "run", "start"]
