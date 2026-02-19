// Language Switcher
(function () {
  const languageToggle = document.getElementById('languageToggle');
  const languageMenu = document.getElementById('languageMenu');
  const currentLangSpan = document.getElementById('currentLang');
  const languageOptions = document.querySelectorAll('.language-option');

  // Load saved language from localStorage
  const savedLang = localStorage.getItem('appLanguage') || 'ar';

  // Apply RTL/LTR on page load
  applyDirection(savedLang);

  // Update active state
  languageOptions.forEach((opt) => {
    if (opt.dataset.lang === savedLang) {
      opt.classList.add('active');
      currentLangSpan.textContent = opt.querySelector('span').textContent;
    } else {
      opt.classList.remove('active');
    }
  });

  // Toggle language menu
  if (languageToggle) {
    languageToggle.addEventListener('click', function (e) {
      e.stopPropagation();
      languageToggle.classList.toggle('active');
      languageMenu.classList.toggle('show');
    });
  }

  // Close menu when clicking outside
  document.addEventListener('click', function (e) {
    if (
      !languageToggle.contains(e.target) &&
      !languageMenu.contains(e.target)
    ) {
      languageToggle.classList.remove('active');
      languageMenu.classList.remove('show');
    }
  });

  // Language option click handler
  languageOptions.forEach((option) => {
    option.addEventListener('click', function (e) {
      e.preventDefault();
      const lang = this.dataset.lang;
      setLanguage(lang, true);

      // Close menu
      languageToggle.classList.remove('active');
      languageMenu.classList.remove('show');
    });
  });

  async function setLanguage(lang, reload = false) {
    // Save to localStorage
    localStorage.setItem('appLanguage', lang);

    // Apply direction immediately
    applyDirection(lang);

    // Update active state
    languageOptions.forEach((opt) => {
      if (opt.dataset.lang === lang) {
        opt.classList.add('active');
        currentLangSpan.textContent = opt.querySelector('span').textContent;
      } else {
        opt.classList.remove('active');
      }
    });

    const url = window.location.pathname + window.location.search;
    const encodedReturnUrl = encodeURIComponent(url);

    await fetch(
      `/Home/SetLanguage?culture=${lang}&returnUrl=${encodedReturnUrl}`,
      {
        method: 'POST',
      }
    )
      .then(() => {
        if (reload) window.location.reload();
      })
      .catch((error) => console.error('Error:', error));
  }

  // Helper function to apply text direction
  function applyDirection(lang) {
    const isRTL = lang === 'ar' || lang === 'he'; // Arabic or Hebrew
    const dir = isRTL ? 'rtl' : 'ltr';

    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);
    document.body.setAttribute('dir', dir);
  }
})();
