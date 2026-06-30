const CACHE_NAME = 'linkup-v2';
const STATIC_ASSETS = [
  '/',
  '/css/site.css',
  '/css/custom.css',
  '/css/transitions.css',
  '/js/site.js',
  '/manifest.json'
];

self.addEventListener('install', function (event) {
  event.waitUntil(
    caches.open(CACHE_NAME).then(function (cache) {
      return cache.addAll(STATIC_ASSETS);
    })
  );
});

self.addEventListener('activate', function (event) {
  event.waitUntil(
    caches.keys().then(function (keys) {
      return Promise.all(keys.filter(function (k) { return k !== CACHE_NAME; }).map(function (k) { return caches.delete(k); }));
    })
  );
});

self.addEventListener('fetch', function (event) {
  if (event.request.method !== 'GET') return;
  event.respondWith(
    caches.open(CACHE_NAME).then(function (cache) {
      return cache.match(event.request).then(function (cached) {
        var fetchPromise = fetch(event.request).then(function (response) {
          if (response && response.status === 200) {
            cache.put(event.request, response.clone());
          }
          return response;
        }).catch(function () {
          return cached;
        });
        return cached || fetchPromise;
      });
    })
  );
});
