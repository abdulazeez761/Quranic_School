if ('serviceWorker' in navigator) {
  window.addEventListener('load', async () => {
    try {
      const registration = await navigator.serviceWorker.register('/sw.js');

      registration.update();

      if (registration.waiting) {
        registration.waiting.postMessage({ type: 'SKIP_WAITING' });
      }

      registration.addEventListener('updatefound', () => {
        const newWorker = registration.installing;
        newWorker.addEventListener('statechange', () => {
          if (
            newWorker.state === 'installed' &&
            navigator.serviceWorker.controller
          ) {
            newWorker.postMessage({ type: 'SKIP_WAITING' });
          }
        });
      });
    } catch (error) {
      console.error('[SW] Registration failed:', error);
    }
  });

  let refreshing = false;
  navigator.serviceWorker.addEventListener('controllerchange', () => {
    if (!refreshing) {
      refreshing = true;
      window.location.reload();
    }
  });
}

// Timezone Cookie Detection
(function() {
  const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
  const cookieName = "UserTimeZone";
  
  function getCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return decodeURIComponent(parts.pop().split(';').shift());
    return null;
  }
  
  const existingTimeZone = getCookie(cookieName);
  if (existingTimeZone !== timeZone) {
    document.cookie = `${cookieName}=${encodeURIComponent(timeZone)}; path=/; max-age=31536000; SameSite=Lax`;
    if (existingTimeZone) {
      window.location.reload();
    }
  }
})();
