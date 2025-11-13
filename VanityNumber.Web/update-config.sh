#!/bin/sh
set -e

# Default API base URL
API_BASE_URL="${API_BASE_URL:-http://localhost:8081/}"

# Update appsettings.json with the API base URL from environment variable
cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "ApiBaseUrl": "$API_BASE_URL"
}
EOF

echo "Updated appsettings.json with ApiBaseUrl: $API_BASE_URL"

# Start nginx
exec nginx -g "daemon off;"
