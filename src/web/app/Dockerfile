# Build context: src/web/app
# docker build -f src/web/app/Dockerfile -t domusmind-web src/web/app

FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies first for layer caching
COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ────────────────────────────────────────────────────────────
FROM nginx:stable-alpine AS runtime

# Static assets from Vite build
COPY --from=build /app/dist /usr/share/nginx/html

# nginx template: ${API_BASE_URL} is substituted at container start.
# The nginx image only substitutes variables that exist in the environment,
# so nginx runtime vars like $host and $remote_addr are passed through intact.
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 80
