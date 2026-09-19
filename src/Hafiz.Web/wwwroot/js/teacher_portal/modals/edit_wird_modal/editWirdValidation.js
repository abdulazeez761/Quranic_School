/**
 * ============================================================================
 * Edit Single Wird Modal - Validation & Dynamic Form Rules
 * ============================================================================
 * Manages unit constraints (Pages/Ayahs/Juz), equivalent chips synchronization,
 * Surah-range dependency filters, and range order validation.
 * ============================================================================
 */

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

/**
 * Synchronize Grade select state with IsUpcoming toggle
 */
function syncEditModalUpcomingState(isUpcoming) {
  const gradeSelect = document.getElementById('Grade');
  const notice = document.getElementById('gradeUpcomingNotice');

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
      if (gradeSelect.dataset.savedGrade) {
        gradeSelect.value = gradeSelect.dataset.savedGrade;
      }
      if (notice) notice.style.display = 'none';
    }
  }
}
window.syncEditModalUpcomingState = syncEditModalUpcomingState;

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

  // 4. Upcoming wird toggle & Grade mutual exclusivity
  const upcomingToggle = document.getElementById('IsUpcoming');
  const gradeSelect = document.getElementById('Grade');

  if (upcomingToggle) {
    upcomingToggle.addEventListener('change', function () {
      syncEditModalUpcomingState(this.checked);
    });
  }

  if (gradeSelect) {
    gradeSelect.addEventListener('change', function () {
      if (this.value !== '0' && upcomingToggle && upcomingToggle.checked) {
        upcomingToggle.checked = false;
        syncEditModalUpcomingState(false);
      }
    });
  }
}
