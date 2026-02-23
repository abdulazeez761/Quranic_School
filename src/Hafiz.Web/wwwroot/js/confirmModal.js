document.addEventListener('DOMContentLoaded', function () {
  const deleteElements = document.querySelectorAll('[data-confirm-delete]');

  deleteElements.forEach((el) => {
    el.addEventListener('click', function (event) {
      event.preventDefault();
      const message = this.getAttribute('data-confirm-delete');
      const form = this.closest('form');
      const url = this.getAttribute('href');
      Swal.fire({
        title: 'تأكيد الحذف',
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#198754',
        cancelButtonColor: '#d33',
        confirmButtonText: 'نعم، احذف',
        cancelButtonText: 'إلغاء',
      }).then((result) => {
        if (result.isConfirmed) {
          if (form) {
            form.submit(); // POST with AntiForgeryToken
          } else {
            window.location.href = url; // fallback for <a> links
          }
        }
      });
    });
  });
});
