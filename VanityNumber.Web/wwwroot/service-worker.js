// Service Worker for Vanity Number Generator PWA
// Cache version will be updated by build script
const CACHE_NAME = 'vanity-number-v1';
const RUNTIME_CACHE = 'vanity-number-runtime';

// Files to cache on install
const PRECACHE_URLS = [
    '/',
    '/index.html',
    '/css/app.css',
    '/js/theme-manager.js',
    '/manifest.json'
};

// Install event - cache static assets
self.addEventListener('install', event => {
    console.log('[SW] Installing version:', CACHE_NAME);
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('[SW] Caching app shell');
                return cache.addAll(PRECACHE_URLS);
            })
            .then(() => {
                console.log('[SW] Skip waiting');
                return self.skipWaiting();
            })
            .catch(error => {
                console.error('[SW] Installation failed:', error);
            })
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', event => {
    console.log('[SW] Activating version:', CACHE_NAME);
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames
                    .filter(name => {
                        // Delete old versions but keep current
                        const isOldCache = name.startsWith('vanity-number-') && name !== CACHE_NAME && name !== RUNTIME_CACHE;
                        if (isOldCache) {
                            console.log('[SW] Deleting old cache:', name);
                        }
                        return isOldCache;
                    })
                    .map(name => caches.delete(name))
            );
        }).then(() => {
            console.log('[SW] Claiming clients');
            return self.clients.claim();
        })
    );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);
    
    // Skip cross-origin requests
    if (url.origin !== self.location.origin) {
        return;
    }

    // Skip API calls - always go to network
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    // Clone and cache successful API responses
                    if (response.ok) {
                        const responseToCache = response.clone();
                        caches.open(RUNTIME_CACHE).then(cache => {
                            cache.put(event.request, responseToCache);
                        });
                    }
                    return response;
                })
                .catch(() => {
                    // Fallback to cached response if network fails
                    return caches.match(event.request);
                })
        );
        return;
    }

    // For HTML files - network first (to get latest version)
    if (url.pathname.endsWith('.html') || url.pathname === '/') {
        event.respondWith(
            fetch(event.request)
                .then(response => {
                    // Cache the new version
                    const responseToCache = response.clone();
                    caches.open(CACHE_NAME).then(cache => {
                        cache.put(event.request, responseToCache);
                    });
                    return response;
                })
                .catch(() => {
                    // Fallback to cache if offline
                    return caches.match(event.request)
                        .then(cachedResponse => cachedResponse || caches.match('/index.html'));
                })
        );
        return;
    }

    // Cache-first strategy for static assets
    event.respondWith(
        caches.match(event.request)
            .then(cachedResponse => {
                if (cachedResponse) {
                    // Return cached version
                    return cachedResponse;
                }

                // Fetch from network and cache
                return fetch(event.request).then(response => {
                    // Only cache successful responses
                    if (!response || response.status !== 200 || response.type === 'error') {
                        return response;
                    }

                    // Clone and cache the response
                    const responseToCache = response.clone();
                    caches.open(RUNTIME_CACHE).then(cache => {
                        cache.put(event.request, responseToCache);
                    });

                    return response;
                });
            })
    );
});

// Handle messages from clients
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.log('[SW] Received SKIP_WAITING message');
        self.skipWaiting();
    }
    
    if (event.data && event.data.type === 'CLEAR_CACHE') {
        console.log('[SW] Clearing all caches');
        event.waitUntil(
            caches.keys().then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        console.log('[SW] Deleting cache:', cacheName);
                        return caches.delete(cacheName);
                    })
                );
            })
        );
    }
});

// Log version on startup
console.log('[SW] Service Worker loaded. Cache version:', CACHE_NAME);
