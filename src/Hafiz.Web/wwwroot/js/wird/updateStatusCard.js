document.addEventListener('click', async function (e) {
  // نتحقق مما إذا كان العنصر المنقور هو زر الحالة (أو أيقونة بداخله)
  const btn = e.target.closest('.status-button');

  if (btn) {
    e.preventDefault();

    let id = btn.getAttribute('data-id');
    let status = btn.getAttribute('data-status');
    // استخدمنا trim() للتأكد من عدم أخذ أي مسافات فارغة بالخطأ
    let originalText = btn.innerText.trim();

    let tokenInput = document.querySelector(
      'input[name="__RequestVerificationToken"]',
    );
    let token = tokenInput ? tokenInput.value : '';

    try {
      // إرسال الطلب للسيرفر
      const res = await fetch('/Teacher/Wird/UpdateStatus', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token,
        },
        body: JSON.stringify({ Id: id, Status: status }),
      });

      const data = await res.json();
      const card = btn.closest('.wird-card');

      if (data.success) {
        let cardStatus = card.querySelector('.wird-status-badge');

        // 1. تحديث النص
        cardStatus.innerText = originalText;

        // 2. تحديث الكلاس (طريقة آمنة بدلاً من item(1) التي قد تسبب مشاكل)
        cardStatus.className = `wird-status-badge status-${status.toLowerCase()}`;

        // 3. تحديث لون إطار الكارد بناءً على الحالة
        const statusColors = {
          excellent: '#28a745',
          verygood: '#17a2b8',
          good: '#fd7e14',
          fair: '#ffc107',
          poor: '#dc3545',
          notset: '#d9d9d9',
        };

        const statusKey = status.toLowerCase().replace(' ', '');
        const borderColor = statusColors[statusKey] || '#34654d';

        // 4. إضافة تأثير النجاح (وميض أخضر)
        card.classList.add('updated');
        card.style.borderLeftColor = '#28a745';

        setTimeout(() => {
          card.classList.remove('updated');
          // بعد الوميض الأخضر، نثبت اللون الخاص بالحالة الجديدة
          card.style.borderLeftColor = borderColor;
        }, 1000);
      } else {
        // إضافة تأثير الفشل (وميض أحمر)
        const originalBorderColor = card.style.borderLeftColor;
        card.classList.add('failed-to-update');
        card.style.borderLeftColor = '#dc3545';

        setTimeout(() => {
          card.classList.remove('failed-to-update');
          card.style.borderLeftColor = originalBorderColor;
        }, 1000);
      }
    } catch (error) {
      console.error('Error updating status:', error);

      // تأثير بصري عند حدوث خطأ في الاتصال
      const card = btn.closest('.wird-card');
      const originalBorderColor = card.style.borderLeftColor;
      card.style.borderLeftColor = '#dc3545';

      setTimeout(() => {
        card.style.borderLeftColor = originalBorderColor;
      }, 1000);
    }
  }
});
