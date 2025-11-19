#!/bin/bash

# Sentry Configuration Helper Script
# This script helps you configure Sentry DSNs for both front-end and backend

set -e

echo "=================================================="
echo "  Vanity Number - Sentry Configuration Helper"
echo "=================================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to prompt for input
prompt_dsn() {
    local service_name=$1
    local default_value=$2
    
    echo -e "${YELLOW}Enter Sentry DSN for ${service_name}:${NC}"
    echo "  (Leave empty to disable Sentry for this service)"
    echo "  Example: https://abc123@o123456.ingest.sentry.io/7654321"
    read -r dsn
    
    if [ -z "$dsn" ]; then
        echo -e "${YELLOW}  ⚠️  Sentry will be DISABLED for ${service_name}${NC}"
        echo ""
    else
        echo -e "${GREEN}  ✅ Sentry configured for ${service_name}${NC}"
        echo ""
    fi
    
    echo "$dsn"
}

# Function to update JSON file
update_json() {
    local file=$1
    local dsn=$2
    local environment=$3
    local sample_rate=$4
    
    if [ ! -f "$file" ]; then
        echo -e "${RED}Error: File not found: $file${NC}"
        return 1
    fi
    
    # Create backup
    cp "$file" "$file.backup"
    
    # Update DSN using sed (works on Linux and macOS)
    sed -i.tmp "s|\"Dsn\": \"[^\"]*\"|\"Dsn\": \"$dsn\"|" "$file"
    sed -i.tmp "s|\"Environment\": \"[^\"]*\"|\"Environment\": \"$environment\"|" "$file"
    sed -i.tmp "s|\"TracesSampleRate\": [0-9.]*|\"TracesSampleRate\": $sample_rate|" "$file"
    
    # Clean up temporary files
    rm -f "$file.tmp"
    
    echo -e "${GREEN}  ✅ Updated: $file${NC}"
}

echo "=================================================="
echo "  Step 1: Front-End Configuration (Blazor WASM)"
echo "=================================================="
echo ""

web_dsn=$(prompt_dsn "Front-End (Blazor)" "")

echo "Select environment for Front-End:"
echo "  1) Development"
echo "  2) Production"
read -p "Choice [1-2]: " web_env_choice

case $web_env_choice in
    1)
        web_env="Development"
        web_sample_rate="1.0"
        web_config="VanityNumber.Web/wwwroot/appsettings.json"
        ;;
    2)
        web_env="Production"
        web_sample_rate="0.1"
        web_config="VanityNumber.Web/wwwroot/appsettings.Production.json"
        ;;
    *)
        echo -e "${RED}Invalid choice. Using Development.${NC}"
        web_env="Development"
        web_sample_rate="1.0"
        web_config="VanityNumber.Web/wwwroot/appsettings.json"
        ;;
esac

echo ""
echo "Updating Front-End configuration..."
update_json "$web_config" "$web_dsn" "$web_env" "$web_sample_rate"

echo ""
echo "=================================================="
echo "  Step 2: Backend Configuration (API)"
echo "=================================================="
echo ""

api_dsn=$(prompt_dsn "Backend (API)" "")

echo "Select environment for Backend:"
echo "  1) Development"
echo "  2) Production"
read -p "Choice [1-2]: " api_env_choice

case $api_env_choice in
    1)
        api_env="Development"
        api_sample_rate="1.0"
        api_config="VanityNumber.Api/appsettings.json"
        ;;
    2)
        api_env="Production"
        api_sample_rate="0.1"
        api_config="VanityNumber.Api/appsettings.Production.json"
        ;;
    *)
        echo -e "${RED}Invalid choice. Using Development.${NC}"
        api_env="Development"
        api_sample_rate="1.0"
        api_config="VanityNumber.Api/appsettings.json"
        ;;
esac

echo ""
echo "Updating Backend configuration..."
update_json "$api_config" "$api_dsn" "$api_env" "$api_sample_rate"

echo ""
echo "=================================================="
echo "  Configuration Summary"
echo "=================================================="
echo ""
echo -e "Front-End (Blazor WASM):"
echo -e "  File: ${YELLOW}$web_config${NC}"
echo -e "  DSN: ${YELLOW}${web_dsn:-"[DISABLED]"}${NC}"
echo -e "  Environment: ${YELLOW}$web_env${NC}"
echo -e "  Sample Rate: ${YELLOW}$web_sample_rate${NC}"
echo ""
echo -e "Backend (API):"
echo -e "  File: ${YELLOW}$api_config${NC}"
echo -e "  DSN: ${YELLOW}${api_dsn:-"[DISABLED]"}${NC}"
echo -e "  Environment: ${YELLOW}$api_env${NC}"
echo -e "  Sample Rate: ${YELLOW}$api_sample_rate${NC}"
echo ""

echo "=================================================="
echo "  Next Steps"
echo "=================================================="
echo ""
echo "1. Review the updated configuration files"
echo "2. Build your project: dotnet build"
echo "3. Test Sentry integration:"
echo "   - Front-End: Add error button to Home.razor"
echo "   - Backend: Call /api/test-sentry endpoint"
echo "4. Check Sentry dashboard for events"
echo ""
echo "Backup files created:"
echo "  - $web_config.backup"
echo "  - $api_config.backup"
echo ""
echo -e "${GREEN}Configuration complete!${NC}"
echo ""
echo "For more information, see:"
echo "  - SENTRY_QUICK_START.md"
echo "  - SENTRY_MONITORING_GUIDE.md"
echo ""
