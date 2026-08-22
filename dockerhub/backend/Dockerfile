FROM node:22-alpine

WORKDIR /app
COPY package*.json ./
RUN npm install --omit=dev

#Declare the build argument
ARG DATABASE_URL
# Set it as an environment variable for Next.js to find
ENV DATABASE_URL=$DATABASE_URL


COPY . .
EXPOSE 5000
CMD ["node", "src/index.js"]
