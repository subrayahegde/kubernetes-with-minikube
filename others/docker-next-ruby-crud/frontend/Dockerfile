FROM node:22-alpine
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
ENV NEXT_PUBLIC_BACKEND_URL="http://10.102.210.53:5000"

RUN npm run build

EXPOSE 3000
CMD ["npm", "run", "start", "--", "-H", "0.0.0.0", "-p", "3000"]

