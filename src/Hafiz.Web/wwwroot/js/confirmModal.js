document.addEventListener('DOMContentLoaded', function () {
  const deleteLinks = document.querySelectorAll('[data-confirm-delete]');

  deleteLinks.forEach((link) => {
    link.addEventListener('click', function (event) {
      event.preventDefault();
      const message = this.getAttribute('data-confirm-delete');
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
          window.location.href = url;
        }
      });
    });
  });
});
