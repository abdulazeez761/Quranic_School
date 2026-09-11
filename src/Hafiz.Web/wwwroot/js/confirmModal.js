document.addEventListener('DOMContentLoaded', function () {
  const isArabic =
    document.documentElement.dir === 'rtl' ||
    (document.documentElement.lang &&
      document.documentElement.lang.startsWith('ar'));

  // 1. Delete / Archive Confirmations
  const deleteElements = document.querySelectorAll('[data-confirm-delete]');
  deleteElements.forEach((el) => {
    el.addEventListener('click', function (event) {
      event.preventDefault();
      const message = this.getAttribute('data-confirm-delete');
      const form = this.closest('form');
      const url = this.getAttribute('href');

      Swal.fire({
        title:
          this.getAttribute('data-confirm-title') ||
          (isArabic ? 'تأكيد الحذف والأرشفة' : 'Confirm Archive & Deletion'),
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#6c757d',
        confirmButtonText:
          this.getAttribute('data-confirm-btn') ||
          (isArabic ? 'نعم، أرشف السجل' : 'Yes, Archive'),
        cancelButtonText: isArabic ? 'إلغاء' : 'Cancel',
      }).then((result) => {
        if (result.isConfirmed) {
          if (form) {
            form.submit();
          } else if (url) {
            window.location.href = url;
          }
        }
      });
    });
  });

  // 2. Restore Confirmations
  const restoreElements = document.querySelectorAll('[data-confirm-restore]');
  restoreElements.forEach((el) => {
    el.addEventListener('click', function (event) {
      event.preventDefault();
      const message = this.getAttribute('data-confirm-restore');
      const form = this.closest('form');
      const url = this.getAttribute('href');

      Swal.fire({
        title:
          this.getAttribute('data-confirm-title') ||
          (isArabic ? 'تأكيد استعادة السجل' : 'Confirm Restoration'),
        text: message,
        icon: 'question',
        showCancelButton: true,
        confirmButtonColor: '#198754',
        cancelButtonColor: '#6c757d',
        confirmButtonText:
          this.getAttribute('data-confirm-btn') ||
          (isArabic ? 'نعم، استعد السجل' : 'Yes, Restore'),
        cancelButtonText: isArabic ? 'إلغاء' : 'Cancel',
      }).then((result) => {
        if (result.isConfirmed) {
          if (form) {
            form.submit();
          } else if (url) {
            window.location.href = url;
          }
        }
      });
    });
  });
});
