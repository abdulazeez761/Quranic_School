/**
 * Hafiz Platform - Edit Single Matn Assignment Modal Controller
 * Standalone controller for opening, populating, validating, and saving Matn edits.
 */
function openEditMatnModal(assignmentId, studentId, studentName) {
  const modal = document.getElementById('editMatnModal');
  const nameSpan = document.getElementById('matnEditStudentName');
  const idInput = document.getElementById('matnEditId');
  const studentIdInput = document.getElementById('matnEditStudentId');

  if (nameSpan) nameSpan.textContent = studentName || '';
  if (idInput) idInput.value = assignmentId || '';
  if (studentIdInput) studentIdInput.value = studentId || '';

  if (modal) {
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
  }

  if (assignmentId) {
    fetchMatnAssignmentById(assignmentId);
  }
}
window.openEditMatnModal = openEditMatnModal;

function closeEditMatnModal() {
  const modal = document.getElementById('editMatnModal');
  if (modal) {
    modal.classList.remove('active');
    document.body.style.overflow = '';
  }
}
window.closeEditMatnModal = closeEditMatnModal;

async function fetchMatnAssignmentById(id) {
  try {
    const res = await fetch(`/Teacher/Wird/GetWirdAssignmentById?id=${id}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const data = await res.json();
    populateMatnEditModal(data);
  } catch (error) {
    console.error('Error fetching matn assignment:', error);
    if (typeof Swal !== 'undefined') {
      Swal.fire({
        icon: 'error',
        title: 'خطأ',
        text: 'تعذر تحميل بيانات ورد المتن. يرجى المحاولة مرة أخرى.',
        confirmButtonText: 'حسناً'
      });
    }
  }
}

function populateMatnEditModal(data) {
  const setVal = (id, val) => {
    const el = document.getElementById(id);
    if (el) el.value = (val !== null && val !== undefined) ? val : '';
  };

  setVal('matnEditId', data.id);
  setVal('matnEditPerformanceType', data.performanceType ?? data.type ?? 1);
  setVal('matnEditAmount', data.amount);
  setVal('matnEditUnit', data.unit ?? 1);
  setVal('matnEditChapterName', data.chapterName);
  setVal('matnEditFromNumber', data.fromNumber);
  setVal('matnEditToNumber', data.toNumber);
  setVal('matnEditGrade', data.status ?? 0);
  setVal('matnEditNote', data.note || '');

  const upcomingEl = document.getElementById('matnEditIsUpcoming');
  if (upcomingEl) {
    upcomingEl.checked = data.isUpcoming === true;
    syncMatnUpcomingState(upcomingEl.checked);
  }
}

function syncMatnUpcomingState(isUpcoming) {
  const gradeSelect = document.getElementById('matnEditGrade');
  const notice = document.getElementById('matnGradeUpcomingNotice');
  if (gradeSelect) {
    if (isUpcoming) {
      if (gradeSelect.value !== '0') {
        gradeSelect.dataset.savedGrade = gradeSelect.value;
      }
      gradeSelect.value = '0';
      gradeSelect.disabled = true;
      gradeSelect.style.opacity = '0.55';
      gradeSelect.style.cursor = 'not-allowed';
      if (notice) notice.style.display = 'block';
    } else {
      gradeSelect.disabled = false;
      gradeSelect.style.opacity = '1';
      gradeSelect.style.cursor = 'default';
      if (gradeSelect.dataset.savedGrade && gradeSelect.dataset.savedGrade !== '0') {
        gradeSelect.value = gradeSelect.dataset.savedGrade;
      }
      if (notice) notice.style.display = 'none';
    }
  }
}

document.addEventListener('DOMContentLoaded', function () {
  const modal = document.getElementById('editMatnModal');
  const form = document.getElementById('matnEditAssignmentForm');
  const upcomingEl = document.getElementById('matnEditIsUpcoming');
  const gradeSelect = document.getElementById('matnEditGrade');

  if (upcomingEl) {
    upcomingEl.addEventListener('change', function () {
      syncMatnUpcomingState(this.checked);
    });
  }

  if (gradeSelect) {
    gradeSelect.addEventListener('change', function () {
      if (this.value !== '0' && upcomingEl && upcomingEl.checked) {
        upcomingEl.checked = false;
        syncMatnUpcomingState(false);
      }
    });
  }

  // Backdrop click to close
  document.addEventListener('click', function (e) {
    if (modal && e.target === modal) {
      closeEditMatnModal();
    }
  });

  // Escape key to close
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && modal && modal.classList.contains('active')) {
      closeEditMatnModal();
    }
  });

  // Submit via AJAX
  if (form) {
    form.addEventListener('submit', async function (e) {
      e.preventDefault();

      const submitBtn = form.querySelector('button[type="submit"]');
      const originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';
      if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> جاري الحفظ...';
      }

      try {
        const formData = new FormData(form);
        const gradeSelect = document.getElementById('matnEditGrade');
        if (gradeSelect && gradeSelect.disabled) {
          formData.set('Status', '0');
        }

        const res = await fetch('/Teacher/Wird/EditMatn', {
          method: 'POST',
          body: formData,
          headers: {
            'X-Requested-With': 'XMLHttpRequest'
          }
        });

        if (res.ok) {
          const newCardHtml = await res.text();
          const assignmentId = formData.get('Id');
          const oldCard = document.getElementById(`wird-card-${assignmentId}`);

          if (oldCard && newCardHtml) {
            oldCard.outerHTML = newCardHtml;
            const updatedCard = document.getElementById(`wird-card-${assignmentId}`);
            if (updatedCard) {
              updatedCard.classList.add('updated');
              setTimeout(() => updatedCard.classList.remove('updated'), 1200);
            }
          }

          closeEditMatnModal();

          if (typeof Swal !== 'undefined') {
            Swal.fire({
              icon: 'success',
              title: 'تم بنجاح!',
              text: 'تم حفظ التعديلات على ورد المتن بنجاح',
              timer: 2000,
              showConfirmButton: false
            });
          }
        } else {
          const errText = await res.text();
          if (typeof Swal !== 'undefined') {
            Swal.fire({
              icon: 'error',
              title: 'خطأ',
              text: errText || 'تأكد من إدخال جميع البيانات بشكل صحيح.',
              confirmButtonText: 'حسناً'
            });
          } else {
            alert(errText || 'تأكد من إدخال جميع البيانات بشكل صحيح.');
          }
        }
      } catch (error) {
        console.error('Error saving matn edit:', error);
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'error',
            title: 'خطأ',
            text: 'حدثت مشكلة في الاتصال بالخادم.',
            confirmButtonText: 'حسناً'
          });
        }
      } finally {
        if (submitBtn) {
          submitBtn.disabled = false;
          submitBtn.innerHTML = originalBtnHtml;
        }
      }
    });
  }
});
