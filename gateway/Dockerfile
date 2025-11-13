FROM nginx:alpine

# Copy nginx configuration template
COPY gateway/nginx.conf /etc/nginx/nginx.conf.template

# Copy entrypoint script
COPY gateway/docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Create necessary directories and set permissions for non-root user
RUN mkdir -p /var/cache/nginx /var/log/nginx /var/run && \
	chown -R nginx:nginx /var/cache/nginx /var/log/nginx /var/run /etc/nginx && \
	touch /var/run/nginx.pid && \
	chown nginx:nginx /var/run/nginx.pid

# Expose port 8080 (non-privileged port)
EXPOSE 8080

# Switch to non-root user
USER nginx

# Use custom entrypoint
ENTRYPOINT ["/docker-entrypoint.sh"]
