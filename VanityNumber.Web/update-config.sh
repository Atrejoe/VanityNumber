#!/bin/sh
set -e

echo "🔧 Updating runtime configuration..."

# Default values
API_BASE_URL="${API_BASE_URL:-/}"
SENTRY_WEB_DSN="${SENTRY_WEB_DSN:-}"
SENTRY_WEB_ENVIRONMENT="${SENTRY_WEB_ENVIRONMENT:-Production}"
SENTRY_WEB_SAMPLE_RATE="${SENTRY_WEB_SAMPLE_RATE:-0.1}"
SENTRY_WEB_DEBUG="${SENTRY_WEB_DEBUG:-false}"

# Create appsettings.json with environment variable values
cat > /usr/share/nginx/html/appsettings.json <<EOF
{
  "ApiBaseUrl": "$API_BASE_URL",
  "Sentry": {
    "Dsn": "$SENTRY_WEB_DSN",
    "Environment": "$SENTRY_WEB_ENVIRONMENT",
    "TracesSampleRate": $SENTRY_WEB_SAMPLE_RATE,
    "Debug": $SENTRY_WEB_DEBUG
  }
}
EOF

echo "✅ Configuration updated:"
cat /usr/share/nginx/html/appsettings.json
echo ""

if [ -z "$SENTRY_WEB_DSN" ]; then
    echo "ℹ️  Sentry DSN not provided - monitoring will be disabled"
else
    echo "✅ Sentry monitoring enabled"
    echo "   Environment: $SENTRY_WEB_ENVIRONMENT"
    echo "   Sample Rate: $SENTRY_WEB_SAMPLE_RATE"
fi

# Start nginx
echo "🚀 Starting nginx..."
exec nginx -g "daemon off;"
