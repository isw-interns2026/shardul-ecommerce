# Frontend Dockerfile — multi-stage build for SPA
# Stage 1: Build
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files and install (cached layer)
COPY package.json package-lock.json* ./
RUN npm ci

# Copy source and build
COPY . .
ARG VITE_API_URL=http://localhost:8080/
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

# Stage 2: Serve static files with nginx
FROM nginx:alpine AS runtime

# SPA routing — all paths serve index.html, let React Router handle routing
RUN printf 'server {\n\
    listen 3000;\n\
    root /usr/share/nginx/html;\n\
    index index.html;\n\
    location / {\n\
        try_files $uri $uri/ /index.html;\n\
    }\n\
}\n' > /etc/nginx/conf.d/default.conf

COPY --from=build /app/build/client /usr/share/nginx/html

EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
