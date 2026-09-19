/**
 * ============================================================================
 * Edit Single Wird Modal - Service & AJAX Network Handler
 * ============================================================================
 * Fetches single assignment details from the server and handles AJAX form
 * submissions, UI updates, and SweetAlert notifications.
 * ============================================================================
 */

/**
 * Global helper to fetch and populate assignment data into the edit modal
 */
window.fetchWirdAssignmentById = async function (id) {
  try {
    const response = await fetch(`/Teacher/Wird/GetWirdAssignmentById?id=${id}`);
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}`);
    }
    const data = await response.json();
    if (typeof populateEditModal === 'function') {
      populateEditModal(data);
    }
  } catch (error) {
    console.error('Error fetching assignment details:', error);
    if (typeof Swal !== 'undefined') {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: 'تعذر تحميل بيانات الورد. يرجى المحاولة مرة أخرى.',
        confirmButtonText: 'حسناً'
      });
    }
  }
};

/**
 * Handles Form Submission via AJAX with validation and instant UI card replacement
 */
function setupEditWirdFormSubmit() {
  const form = document.getElementById('wirdAssignmentForm');
  if (!form) return;

  form.addEventListener('submit', async function (e) {
    e.preventDefault();

    // Validate range order
    if (typeof getRangeErrors === 'function') {
      const rangeErrors = getRangeErrors();
      if (rangeErrors.length > 0) {
        const errorMsg = 'قيمة "إلى" يجب ألا تكون قبل قيمة "من" في: ' + rangeErrors.join('، ');
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'warning',
            title: 'ترتيب غير صحيح',
            text: errorMsg,
            confirmButtonText: 'حسناً'
          });
        } else {
          alert(errorMsg);
        }
        return;
      }
    }

    const submitBtn = form.querySelector('button[type="submit"]');
    const originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';
    if (submitBtn) {
      submitBtn.disabled = true;
      submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> جاري الحفظ...';
    }

    try {
      const formData = new FormData(form);
      const isUpcomingChecked = document.getElementById('IsUpcoming')?.checked === true;
      if (isUpcomingChecked) {
        formData.set('Status', '0');
        formData.set('IsUpcoming', 'true');
      }
      const actionUrl = form.getAttribute('action') || '/Teacher/Student/EditWird';

      const response = await fetch(actionUrl, {
        method: 'POST',
        body: formData,
        headers: {
          'X-Requested-With': 'XMLHttpRequest'
        }
      });

      if (response.ok) {
        const newCardHtml = await response.text();
        const wirdId = formData.get('Id');
        const oldCard = document.getElementById(`wird-card-${wirdId}`);

        if (oldCard && newCardHtml) {
          oldCard.outerHTML = newCardHtml;
          const updatedCard = document.getElementById(`wird-card-${wirdId}`);
          if (updatedCard) {
            updatedCard.classList.add('updated');
            setTimeout(() => updatedCard.classList.remove('updated'), 1200);
          }
        }

        if (typeof closeEditWirdModal === 'function') {
          closeEditWirdModal();
        }

        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'success',
            title: 'تم بنجاح!',
            text: 'تم حفظ التعديلات على الورد بنجاح',
            timer: 2000,
            showConfirmButton: false
          });
        }
      } else {
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'تأكد من إدخال جميع البيانات بشكل صحيح.',
            confirmButtonText: 'حسناً'
          });
        } else {
          alert('تأكد من إدخال جميع البيانات بشكل صحيح.');
        }
      }
    } catch (error) {
      console.error('Error saving wird edit:', error);
      if (typeof Swal !== 'undefined') {
        Swal.fire({
          icon: 'error',
          title: 'خطأ',
          text: 'حدثت مشكلة في الاتصال بالخادم.',
          confirmButtonText: 'حسناً'
        });
      } else {
        alert('حدثت مشكلة في الاتصال بالخادم.');
      }
    } finally {
      if (submitBtn) {
        submitBtn.disabled = false;
        submitBtn.innerHTML = originalBtnHtml;
      }
    }
  });
}
