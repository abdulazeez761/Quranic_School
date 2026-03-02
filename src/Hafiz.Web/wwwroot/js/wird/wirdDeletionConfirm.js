document.addEventListener('DOMContentLoaded', function () {
  // نضع مستمع الأحداث على الصفحة بالكامل (document)
  document.addEventListener('submit', async function (e) {
    // نتحقق مما إذا كان العنصر الذي أطلق الحدث هو فورم الحذف الخاص بنا
    const formElement = e.target.closest('.delete-form');

    // إذا كان هو فورم الحذف، ننفذ كودنا
    if (formElement) {
      e.preventDefault();

      const actionUrl = formElement.getAttribute('action');
      const submitBtn = formElement.querySelector('button[type="submit"]');
      const confirmMessage = submitBtn.getAttribute('data-confirm-delete');

      // تحديد الـ Card الأب الذي يحتوي على زر الحذف، لكي نخفيه لاحقاً
      const cardElement = formElement.closest('.wird-card');

      // إظهار رسالة التأكيد باستخدام SweetAlert2
      Swal.fire({
        title: 'تأكيد الحذف',
        text: confirmMessage,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف!',
        cancelButtonText: 'إلغاء',
      }).then(async (result) => {
        if (result.isConfirmed) {
          try {
            const formData = new FormData(formElement);

            const response = await fetch(actionUrl, {
              method: 'POST',
              body: formData,
            });

            if (response.ok) {
              if (cardElement) {
                cardElement.style.transition = 'opacity 0.4s ease';
                cardElement.style.opacity = 0;
                setTimeout(() => {
                  cardElement.remove();
                }, 400);
              }

              Swal.fire('تم الحذف!', 'تم مسح السجل بنجاح.', 'success');
            } else {
              Swal.fire('خطأ!', 'حدث خطأ أثناء عملية الحذف.', 'error');
            }
          } catch (error) {
            console.error('Error:', error);
            Swal.fire('خطأ!', 'حدثت مشكلة في الاتصال بالخادم.', 'error');
          }
        }
      });
    }
  });
});
