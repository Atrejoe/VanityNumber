# Use the official .NET 10 RC SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy solution file
COPY VanityNumber.sln ./

# Copy project files for restore (improves Docker layer caching)
COPY VanityNumber.Web/VanityNumber.Web.csproj VanityNumber.Web/
COPY VanityNumber.Core/VanityNumber.Core.csproj VanityNumber.Core/

# Restore dependencies
RUN dotnet restore VanityNumber.Web/VanityNumber.Web.csproj

# Copy all source files
COPY VanityNumber.Web/ VanityNumber.Web/
COPY VanityNumber.Core/ VanityNumber.Core/

# Build and publish the Blazor WebAssembly application
WORKDIR /src/VanityNumber.Web
RUN dotnet publish VanityNumber.Web.csproj -c Release -o /app/publish

# Build runtime image using nginx to serve static files
FROM nginx:alpine AS final

# Set working directory
WORKDIR /usr/share/nginx/html

# Remove default nginx static content
RUN rm -rf ./*

# Copy published Blazor WebAssembly files
COPY --from=build /app/publish/wwwroot .

# Copy custom nginx configuration (already configured for port 8080)
COPY VanityNumber.Web/nginx.conf /etc/nginx/nginx.conf

# Create necessary directories and set permissions for non-root user
RUN mkdir -p /var/cache/nginx /var/log/nginx /var/run && \
    chown -R nginx:nginx /usr/share/nginx/html /var/cache/nginx /var/log/nginx /var/run /etc/nginx && \
    chmod -R 755 /usr/share/nginx/html && \
    touch /var/run/nginx.pid && \
    chown nginx:nginx /var/run/nginx.pid

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Switch to non-root user
USER nginx

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
