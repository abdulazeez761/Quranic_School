document.addEventListener('DOMContentLoaded', function () {
  // العثور على جميع فورمات الحذف في الصفحة
  const deleteForms = document.querySelectorAll('.delete-form');

  deleteForms.forEach((form) => {
    form.addEventListener('submit', async function (e) {
      e.preventDefault(); // إيقاف إعادة تحميل الصفحة

      const formElement = this;
      const actionUrl = formElement.getAttribute('action');
      const submitBtn = formElement.querySelector('button[type="submit"]');
      const confirmMessage = submitBtn.getAttribute('data-confirm-delete');

      // تحديد الـ Card الأب الذي يحتوي على زر الحذف، لكي نخفيه لاحقاً
      const cardElement = formElement.closest('.wird-card');

      // إظهار رسالة التأكيد باستخدام SweetAlert2
      Swal.fire({
        title: 'تأكيد الحذف',
        text: confirmMessage, // الرسالة القادمة من الداتا الخاصة بالزر
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'نعم، احذف!',
        cancelButtonText: 'إلغاء',
      }).then(async (result) => {
        if (result.isConfirmed) {
          try {
            // نستخدم FormData لكي نرسل الـ Token الأمني تلقائياً مع الطلب
            const formData = new FormData(formElement);

            const response = await fetch(actionUrl, {
              method: 'POST',
              body: formData,
            });

            if (response.ok) {
              // إذا نجح الحذف في السيرفر، نخفي الـ Card بحركة سلسة
              if (cardElement) {
                cardElement.style.transition = 'opacity 0.4s ease';
                cardElement.style.opacity = 0;
                setTimeout(() => {
                  cardElement.remove(); // مسح العنصر من الـ HTML
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
    });
  });
});
