/**
 * ============================================================================
 * Backward Compatibility Proxy for editWirdModal.js
 * ============================================================================
 * The functionality of editWirdModal.js has been decomposed into smaller,
 * modular files located at wwwroot/js/teacher_portal/modals/edit_wird_modal/:
 *   1. editWirdValidation.js
 *   2. editWirdService.js
 *   3. editWirdModal.js
 * This file remains as a fallback proxy for legacy cache compatibility.
 * ============================================================================
 */

(function () {
  if (typeof window.openEditWirdModal === 'function') return;

  const scripts = [
    '/js/teacher_portal/modals/edit_wird_modal/editWirdValidation.js',
    '/js/teacher_portal/modals/edit_wird_modal/editWirdService.js',
    '/js/teacher_portal/modals/edit_wird_modal/editWirdModal.js'
  ];

  scripts.forEach((src) => {
    if (!document.querySelector(`script[src*="${src}"]`)) {
      const s = document.createElement('script');
      s.src = src;
      s.async = false;
      document.head.appendChild(s);
    }
  });
})();
