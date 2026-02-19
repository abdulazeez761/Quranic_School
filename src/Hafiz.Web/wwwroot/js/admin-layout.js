// ===================================
// Mobile Sidebar Toggle
// ===================================
const sidebar = document.getElementById('sidebar');
const sidebarToggle = document.getElementById('sidebarToggle');
const sidebarOverlay = document.getElementById('sidebarOverlay');
const languageToggle = document.getElementById('languageToggle');

// Ensure elements exist before adding listeners
if (sidebar && sidebarToggle && sidebarOverlay) {
  // Toggle sidebar on button click
  sidebarToggle.addEventListener('click', function (e) {
    e.preventDefault();
    e.stopPropagation();
    sidebar.classList.toggle('active');
    sidebarOverlay.classList.toggle('active');
    document.body.style.overflow = sidebar.classList.contains('active')
      ? 'hidden'
      : '';
  });

  // Close sidebar when clicking overlay
  sidebarOverlay.addEventListener('click', function (e) {
    e.preventDefault();
    sidebar.classList.remove('active');
    sidebarOverlay.classList.remove('active');
    document.body.style.overflow = '';
  });

  // Close sidebar on window resize if going to desktop
  let resizeTimer;
  window.addEventListener('resize', function () {
    clearTimeout(resizeTimer);
    resizeTimer = setTimeout(function () {
      if (window.innerWidth > 768) {
        sidebar.classList.remove('active');
        sidebarOverlay.classList.remove('active');
        document.body.style.overflow = '';
      }
    }, 250);
  });

  // Close sidebar when clicking a navigation link on mobile
  const sidebarLinks = sidebar.querySelectorAll('a, button');
  sidebarLinks.forEach((link) => {
    link.addEventListener('click', function (e) {
      // Don't close if clicking on language toggle or menu
      const isLanguageToggle =
        link === languageToggle || languageToggle?.contains(link);
      const isLanguageMenu = document
        .getElementById('languageMenu')
        ?.contains(link);

      // Don't close on logout button in some cases, but close on nav links
      if (
        window.innerWidth <= 768 &&
        !link.classList.contains('logout') &&
        !isLanguageToggle &&
        !isLanguageMenu
      ) {
        e.stopPropagation();
        sidebar.classList.remove('active');
        sidebarOverlay.classList.remove('active');
        document.body.style.overflow = '';
      }
    });
  });

  // Keyboard escape to close sidebar
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && sidebar.classList.contains('active')) {
      sidebar.classList.remove('active');
      sidebarOverlay.classList.remove('active');
      document.body.style.overflow = '';
    }
  });
}
