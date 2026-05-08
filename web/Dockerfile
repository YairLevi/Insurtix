# Build Angular app, then serve static output with nginx.
#
# API URL is provided at runtime via $API_URL env var:
#   - Container startup writes /usr/share/nginx/html/env.js with that URL.
#   - Browser loads env.js before Angular bootstraps, sets window.__env.apiUrl.
#   - Default: http://localhost:8080  (matches API container port mapping).
#   - Override at run time: -e API_URL=http://my-api:9000
#
# Inherited from base image:
#   EXPOSE 80
#   CMD is overridden below to template env.js before launching nginx.
#
# Build:  docker build -f web/Dockerfile -t insurtix-web .
# Run:    docker run -p 80:80 insurtix-web

FROM node:22-alpine AS build
WORKDIR /app
COPY web/ .
RUN npm ci && npm run build

FROM nginx:alpine-slim
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
ENV API_URL=http://localhost:8080
CMD ["sh", "-c", "echo \"window.__env={apiUrl:'$API_URL'};\" > /usr/share/nginx/html/env.js && nginx -g 'daemon off;'"]
