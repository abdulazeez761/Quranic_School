document.addEventListener('DOMContentLoaded', function () {
  const form = document.getElementById('wirdAssignmentForm');

  if (form) {
    form.addEventListener('submit', async function (e) {
      // 1. إيقاف إعادة تحميل الصفحة
      e.preventDefault();

      // 2. جمع بيانات الفورم (بما فيها الـ AntiForgeryToken والـ Id)
      const formData = new FormData(this);

      // 3. جلب المسار من الفورم نفسه (لتجنب تعارض المسارات)
      const actionUrl = this.getAttribute('action');

      // 4. استدعاء دالة الحفظ
      await saveWirdEdit(actionUrl, formData);
    });
  }
});

// دالة حفظ التعديل عبر AJAX
async function saveWirdEdit(actionUrl, formData) {
  try {
    const response = await fetch(actionUrl, {
      method: 'POST',
      body: formData,
    });

    if (response.ok) {
      const newCardHtml = await response.text();

      // جلب الـ ID الخاص بالورد لتحديث الكارد المناسب
      const wirdId = formData.get('Id');
      let oldCard = document.getElementById(`wird-card-${wirdId}`);

      // استبدال الكارد القديم بالكارد الجديد
      if (oldCard) {
        oldCard.outerHTML = newCardHtml;
      }

      // إغلاق المودال وإظهار رسالة نجاح
      if (typeof closeWirdModal === 'function') {
        closeWirdModal();
      }

      Swal.fire('تم!', 'تم حفظ التعديلات بنجاح', 'success');
    } else {
      // في حال أرجع السيرفر 400 BadRequest أو غيره
      Swal.fire('خطأ!', 'تأكد من إدخال جميع البيانات بشكل صحيح.', 'error');
    }
  } catch (error) {
    Swal.fire('خطأ!', 'حدث مشكلة في الاتصال بالخادم.', 'error');
  }
}
