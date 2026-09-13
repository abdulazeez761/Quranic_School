/**
 * ============================================================================
 * Hafiz Platform - Edit Single Wird Modal Controller (Dedicated)
 * ============================================================================
 * Handles opening, populating, validating, and AJAX-saving single wird edits.
 * Completely standalone without dependencies on the bulk wird drawer.
 * ============================================================================
 */

function openWirdModal(studentId, studentName) {
  const modal = document.getElementById('assignWirdModal');
  const nameSpan = document.getElementById('studentName');
  const idInput = document.getElementById('StudentId');

  if (nameSpan) nameSpan.textContent = studentName || '';
  if (idInput) idInput.value = studentId || '';

  if (modal) {
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
  }
}

function closeWirdModal() {
  const modal = document.getElementById('assignWirdModal');
  if (modal) {
    modal.classList.remove('active');
    document.body.style.overflow = '';
  }
}

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
    populateEditModal(data);
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
 * Populate all fields in the edit modal from the server response
 */
function populateEditModal(data) {
  const setVal = (id, val) => {
    const el = document.getElementById(id);
    if (el) el.value = (val !== null && val !== undefined) ? val : '';
  };

  // 1. Assignment ID & Type
  setVal('AssignmentId', data.id);
  setVal('Type', data.type);

  // 2. Quantity & Unit
  setVal('Amount', data.amount);

  const amountUnitEl = document.getElementById('AmountUnit');
  if (amountUnitEl) {
    amountUnitEl.value = (data.amountUnit !== null && data.amountUnit !== undefined) ? data.amountUnit : '';
    // Dispatch change to toggle equivalent pages visibility & set min/step
    amountUnitEl.dispatchEvent(new Event('change'));
  }

  // 3. Equivalent Pages
  setVal('EquivalentPages', data.equivalentPages);
  syncEquivalentChips(data.equivalentPages);

  // 4. From Range
  setVal('FromJuz', data.fromJuz);
  setVal('FromPage', data.fromPage);
  setVal('FromSurah', (data.fromSurah && data.fromSurah !== 0) ? data.fromSurah : '');
  setVal('FromAyah', data.fromAyah);

  // 5. To Range
  setVal('ToJuz', data.toJuz);
  setVal('ToPage', data.toPage);
  setVal('ToSurah', (data.toSurah && data.toSurah !== 0) ? data.toSurah : '');
  setVal('ToAyah', data.toAyah);

  // 6. Grade / Status
  setVal('Grade', (data.status !== null && data.status !== undefined) ? data.status : 0);

  // 7. Note
  setVal('Note', data.note || '');

  // 8. Upcoming toggle
  const upcomingEl = document.getElementById('IsUpcoming');
  if (upcomingEl) {
    upcomingEl.checked = data.isUpcoming === true;
  }

  // Trigger FromSurah change to filter ToSurah options
  const fromSurahEl = document.getElementById('FromSurah');
  if (fromSurahEl) {
    fromSurahEl.dispatchEvent(new Event('change'));
  }
}

/**
 * Highlight matching chip if value matches
 */
function syncEquivalentChips(val) {
  const numVal = parseFloat(val);
  const chips = document.querySelectorAll('.equivalent-pages-chips .chip');
  chips.forEach(chip => {
    const chipVal = parseFloat(chip.dataset.value);
    if (!isNaN(numVal) && !isNaN(chipVal) && Math.abs(numVal - chipVal) < 0.001) {
      chip.classList.add('is-active');
    } else {
      chip.classList.remove('is-active');
    }
  });
}

document.addEventListener('DOMContentLoaded', function () {
  const modal = document.getElementById('assignWirdModal');
  const form = document.getElementById('wirdAssignmentForm');

  // Backdrop click to close
  if (modal) {
    modal.addEventListener('click', function (e) {
      if (e.target === modal) {
        closeWirdModal();
      }
    });
  }

  // Escape key to close
  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && modal && modal.classList.contains('active')) {
      closeWirdModal();
    }
  });

  // Setup unit constraints, equivalent pages toggle, and chips
  setupEditFormValidation();

  // Handle Form Submission via AJAX
  if (form) {
    form.addEventListener('submit', async function (e) {
      e.preventDefault();

      // Validate range order
      const rangeErrors = getRangeErrors();
      if (rangeErrors.length > 0) {
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'warning',
            title: 'ترتيب غير صحيح',
            text: 'قيمة "إلى" يجب ألا تكون قبل قيمة "من" في: ' + rangeErrors.join('، '),
            confirmButtonText: 'حسناً'
          });
        } else {
          alert('قيمة "إلى" يجب ألا تكون قبل قيمة "من" في: ' + rangeErrors.join('، '));
        }
        return;
      }

      const submitBtn = form.querySelector('button[type="submit"]');
      const originalBtnHtml = submitBtn ? submitBtn.innerHTML : '';
      if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> جاري الحفظ...';
      }

      try {
        const formData = new FormData(form);
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
          }

          closeWirdModal();

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
});

/**
 * Configure range checks, unit toggles, and chip interactions
 */
function setupEditFormValidation() {
  const amount = document.getElementById('Amount');
  const amountUnit = document.getElementById('AmountUnit');
  const equivalentGroup = document.getElementById('equivalentPagesGroup');
  const equivalentPagesInput = document.getElementById('EquivalentPages');
  const fromSurah = document.getElementById('FromSurah');
  const toSurah = document.getElementById('ToSurah');

  // 1. AmountUnit change -> adjust constraints & toggle equivalent pages
  function applyUnitConstraints(roundExisting) {
    if (!amountUnit) return;
    const unitVal = amountUnit.value;

    if (unitVal === '1') {
      // Ayahs
      if (amount) {
        amount.min = '1';
        amount.step = '1';
        if (roundExisting && amount.value) {
          amount.value = Math.round(parseFloat(amount.value));
        }
      }
      if (equivalentGroup) equivalentGroup.style.display = 'block';
    } else {
      if (equivalentGroup) equivalentGroup.style.display = 'none';

      if (unitVal === '2') {
        // Juz
        if (amount) {
          amount.min = '0.5';
          amount.step = '0.5';
        }
      } else if (unitVal === '0') {
        // Pages
        if (amount) {
          amount.min = '0.1';
          amount.step = 'any';
        }
      }
    }
  }

  amountUnit?.addEventListener('change', () => applyUnitConstraints(true));
  applyUnitConstraints(false);

  // 2. Equivalent Pages Chips
  const chips = document.querySelectorAll('.equivalent-pages-chips .chip');
  chips.forEach(chip => {
    chip.addEventListener('click', function () {
      const val = this.dataset.value;
      if (equivalentPagesInput) {
        equivalentPagesInput.value = val;
      }
      syncEquivalentChips(val);
    });
  });

  if (equivalentPagesInput) {
    equivalentPagesInput.addEventListener('input', function () {
      syncEquivalentChips(this.value);
    });
  }

  // 3. Surah dependency filter
  fromSurah?.addEventListener('change', function () {
    if (!toSurah) return;
    const fromNum = parseInt(fromSurah.value, 10);
    for (const option of toSurah.options) {
      option.style.display =
        option.value && fromNum && parseInt(option.value, 10) < fromNum ? 'none' : '';
    }
  });
}

/**
 * Validate that "To" fields are not strictly less than "From" fields
 */
function getRangeErrors() {
  const errors = [];
  const num = (id) => {
    const el = document.getElementById(id);
    const v = parseFloat(el ? el.value : '');
    return isNaN(v) ? null : v;
  };

  const fJuz = num('FromJuz'), tJuz = num('ToJuz');
  if (fJuz !== null && tJuz !== null && fJuz > tJuz) errors.push('الجزء');

  const fPage = num('FromPage'), tPage = num('ToPage');
  if (fPage !== null && tPage !== null && fPage > tPage) errors.push('الصفحة');

  const fSurah = num('FromSurah'), tSurah = num('ToSurah');
  if (fSurah !== null && tSurah !== null && fSurah > tSurah) errors.push('السورة');

  const fAyah = num('FromAyah'), tAyah = num('ToAyah');
  const sameSurah = (fSurah === null && tSurah === null) || (fSurah === tSurah);
  if (sameSurah && fAyah !== null && tAyah !== null && fAyah > tAyah) {
    errors.push('الآية');
  }

  return errors;
}
