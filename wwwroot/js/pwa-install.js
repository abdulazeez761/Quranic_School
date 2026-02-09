// PWA Installation Handler
let deferredPrompt;
let isInstalled = false;

// Check if app is already installed
if (
  window.matchMedia('(display-mode: standalone)').matches ||
  window.navigator.standalone === true
) {
  isInstalled = true;
  console.log('[PWA] App is running in standalone mode');
}

// Register Service Worker
if ('serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker
      .register('/sw.js')
      .then((registration) => {
        console.log('[PWA] Service Worker registered:', registration.scope);

        // Check for updates
        registration.addEventListener('updatefound', () => {
          const newWorker = registration.installing;
          console.log('[PWA] New Service Worker installing...');

          newWorker.addEventListener('statechange', () => {
            if (
              newWorker.state === 'installed' &&
              navigator.serviceWorker.controller
            ) {
              console.log('[PWA] New content available, please refresh');
              showUpdateNotification();
            }
          });
        });
      })
      .catch((error) => {
        console.error('[PWA] Service Worker registration failed:', error);
      });

    // Handle service worker updates
    let refreshing;
    navigator.serviceWorker.addEventListener('controllerchange', () => {
      if (refreshing) return;
      refreshing = true;
      window.location.reload();
    });
  });
}

// Listen for beforeinstallprompt event
window.addEventListener('beforeinstallprompt', (e) => {
  console.log('[PWA] beforeinstallprompt event fired');
  e.preventDefault();
  deferredPrompt = e;
  showInstallPromotion();
});

// Debug: Check if event fires after 3 seconds and show banner anyway on mobile
setTimeout(() => {
  if (!deferredPrompt && !isInstalled) {
    console.log('[PWA] beforeinstallprompt did not fire. Possible reasons:');
    console.log('- App might already be installed');
    console.log('- Browser does not support PWA installation');
    console.log('- Site is not served over HTTPS');
    console.log('- Service worker registration failed');
    console.log('- Browser install criteria not met');

    // For iOS Safari, always show install instructions
    if (isIOS()) {
      showIOSInstallPrompt();
    }
    // For Android/Chrome mobile, show banner anyway to encourage installation
    else if (isMobile()) {
      console.log('[PWA] Showing install banner for mobile anyway');
      showInstallPromotion();
    }
  }
}, 3000);

// Check if device is mobile
function isMobile() {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    navigator.userAgent
  );
}

// Listen for app installed event
window.addEventListener('appinstalled', () => {
  console.log('[PWA] App installed successfully');
  isInstalled = true;
  hideInstallPromotion();
  deferredPrompt = null;
  showInstallSuccessMessage();
});

// Show install promotion UI
function showInstallPromotion() {
  if (isInstalled) return;

  const installBanner = document.createElement('div');
  installBanner.id = 'pwa-install-banner';
  installBanner.className = 'pwa-install-banner';
  installBanner.innerHTML = `
    <div class="pwa-install-content">
      <div class="pwa-install-icon">
        <i class="bx bx-mobile-alt"></i>
      </div>
      <div class="pwa-install-text">
        <strong>Install Hifz App</strong>
        <p>Install our app for a better experience</p>
      </div>
      <div class="pwa-install-actions">
        <button class="pwa-install-btn" id="pwa-install-btn">
          <i class="bx bx-download"></i> Install
        </button>
        <button class="pwa-dismiss-btn" id="pwa-dismiss-btn">
          <i class="bx bx-x"></i>
        </button>
      </div>
    </div>
  `;

  document.body.appendChild(installBanner);

  // Add event listeners
  document
    .getElementById('pwa-install-btn')
    .addEventListener('click', installApp);
  document
    .getElementById('pwa-dismiss-btn')
    .addEventListener('click', hideInstallPromotion);

  // Show banner with animation
  setTimeout(() => {
    installBanner.classList.add('show');
  }, 100);
}

// Hide install promotion
function hideInstallPromotion() {
  const banner = document.getElementById('pwa-install-banner');
  if (banner) {
    banner.classList.remove('show');
    setTimeout(() => {
      banner.remove();
    }, 300);
  }
}

// Install the app
async function installApp() {
  if (!deferredPrompt) {
    console.log('[PWA] Install prompt not available');

    // Show manual install instructions for Android Chrome
    if (isMobile() && !isIOS()) {
      showManualInstallInstructions();
    }
    return;
  }

  console.log('[PWA] Showing install prompt');
  deferredPrompt.prompt();

  const { outcome } = await deferredPrompt.userChoice;
  console.log(`[PWA] User response: ${outcome}`);

  if (outcome === 'accepted') {
    console.log('[PWA] User accepted the install prompt');
  } else {
    console.log('[PWA] User dismissed the install prompt');
  }

  deferredPrompt = null;
  hideInstallPromotion();
}

// Show manual install instructions for Android
function showManualInstallInstructions() {
  const instructionsDiv = document.createElement('div');
  instructionsDiv.className = 'pwa-ios-prompt';
  instructionsDiv.innerHTML = `
    <div class="pwa-ios-content">
      <div class="pwa-ios-header">
        <h3>📱 تثبيت التطبيق - Install App</h3>
        <button class="pwa-ios-close" onclick="this.parentElement.parentElement.parentElement.remove()">
          <i class="bx bx-x"></i>
        </button>
      </div>
      <div class="pwa-ios-body">
        <p><strong>لتثبيت التطبيق على Android:</strong></p>
        <p><strong>To install on Android:</strong></p>
        <ol>
          <li>اضغط على القائمة ⋮ في الأعلى<br/>Tap the menu ⋮ at the top</li>
          <li>اختر "تثبيت التطبيق" أو "إضافة إلى الشاشة الرئيسية"<br/>Choose "Install app" or "Add to Home screen"</li>
          <li>اضغط "تثبيت"<br/>Tap "Install"</li>
        </ol>
      </div>
    </div>
  `;
  document.body.appendChild(instructionsDiv);
  setTimeout(() => instructionsDiv.classList.add('show'), 100);
}

// Show update notification
function showUpdateNotification() {
  const updateNotification = document.createElement('div');
  updateNotification.className = 'pwa-update-notification';
  updateNotification.innerHTML = `
    <div class="pwa-update-content">
      <i class="bx bx-refresh"></i>
      <span>New version available</span>
      <button class="pwa-update-btn" onclick="window.location.reload()">
        Update
      </button>
    </div>
  `;
  document.body.appendChild(updateNotification);

  setTimeout(() => {
    updateNotification.classList.add('show');
  }, 100);
}

// Show install success message
function showInstallSuccessMessage() {
  const successMessage = document.createElement('div');
  successMessage.className = 'pwa-success-message';
  successMessage.innerHTML = `
    <div class="pwa-success-content">
      <i class="bx bx-check-circle"></i>
      <span>App installed successfully!</span>
    </div>
  `;
  document.body.appendChild(successMessage);

  setTimeout(() => {
    successMessage.classList.add('show');
  }, 100);

  setTimeout(() => {
    successMessage.classList.remove('show');
    setTimeout(() => {
      successMessage.remove();
    }, 300);
  }, 3000);
}

// Add iOS install instructions for Safari
function showIOSInstallInstructions() {
  const isIOS =
    /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
  const isInStandaloneMode =
    'standalone' in window.navigator && window.navigator.standalone;

  if (isIOS && !isInStandaloneMode) {
    const iosPrompt = document.createElement('div');
    iosPrompt.className = 'pwa-ios-prompt';
    iosPrompt.innerHTML = `
      <div class="pwa-ios-content">
        <h3>Install Hifz App</h3>
        <p>Tap <i class="bx bx-share"></i> and then "Add to Home Screen"</p>
        <button class="pwa-dismiss-btn" onclick="this.parentElement.parentElement.remove()">Got it</button>
      </div>
    `;
    document.body.appendChild(iosPrompt);

    setTimeout(() => {
      iosPrompt.classList.add('show');
    }, 100);
  }
}

// Check for iOS and show instructions
if (/iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream) {
  window.addEventListener('load', () => {
    setTimeout(showIOSInstallInstructions, 2000);
  });
}
