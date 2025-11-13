#!/bin/sh
set -e

# Default values for backend URLs
API_BACKEND_URL="${API_BACKEND_URL:-vanity-api:8080}"
WEB_BACKEND_URL="${WEB_BACKEND_URL:-vanity-web:8080}"

echo "Configuring nginx gateway..."
echo "API Backend: ${API_BACKEND_URL}"
echo "Web Backend: ${WEB_BACKEND_URL}"

# Replace environment variables in nginx.conf template
envsubst '${API_BACKEND_URL} ${WEB_BACKEND_URL}' \
	< /etc/nginx/nginx.conf.template \
	> /etc/nginx/nginx.conf

echo "Configuration complete. Starting nginx..."

# Start nginx
exec nginx -g "daemon off;"
