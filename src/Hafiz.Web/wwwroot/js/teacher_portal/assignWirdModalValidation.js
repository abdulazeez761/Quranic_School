// Validation for the Assign Wird modal.
// The first step is intentionally simple: a teacher only has to choose a type and
// enter a quantity (+ unit). The From/To location fields live in the collapsed
// "More details" section and are entirely optional.
(function () {
  const form = document.getElementById('wirdAssignmentForm');
  if (!form) return;

  // The same script is loaded by the Edit modal, which posts via AJAX and may carry
  // legacy wirds that predate the amount field. Detect it and stay lenient there.
  const isEditMode = !!document.getElementById('AssignmentId');

  const typeField = document.getElementById('Type');
  const amount = document.getElementById('Amount');
  const amountUnit = document.getElementById('AmountUnit');
  const fromJuz = document.getElementById('FromJuz');
  const toJuz = document.getElementById('ToJuz');
  const fromPage = document.getElementById('FromPage');
  const toPage = document.getElementById('ToPage');
  const fromSurah = document.getElementById('FromSurah');
  const toSurah = document.getElementById('ToSurah');
  const fromAyah = document.getElementById('FromAyah');
  const toAyah = document.getElementById('ToAyah');

  // Juz and Pages can be fractional (e.g. 1.5 juz, 1.2 pages); ayahs are counted
  // as whole numbers (their sub-page value is captured by EquivalentPages instead).
  // Note: the browser anchors valid step values to `min`, so min and step must
  // agree — otherwise min="0.5" + step="1" makes whole numbers like 3 invalid.
  // Pages use step="any" so any fraction the teacher types is accepted; the
  // decimal(4,2) column keeps it to two decimal places on save.
  function applyUnitConstraints(roundExisting) {
    if (!amount || !amountUnit) return;
    if (amountUnit.value === '2') {
      amount.min = '0.5';
      amount.step = '0.5';
    } else if (amountUnit.value === '0') {
      amount.min = '0.1';
      amount.step = 'any';
    } else if (amountUnit.value === '1') {
      amount.min = '1';
      amount.step = '1';
      if (roundExisting && amount.value) {
        amount.value = Math.round(parseFloat(amount.value));
      }
    }
  }

  amountUnit?.addEventListener('change', function () {
    applyUnitConstraints(true);
  });

  // Initialize on load too: the Edit modal may already have a unit selected.
  applyUnitConstraints(false);

  // In the advanced section, only offer "to" surahs at or after the chosen "from" surah.
  fromSurah?.addEventListener('change', function () {
    if (!toSurah) return;
    const fromNum = parseInt(fromSurah.value);
    for (const option of toSurah.options) {
      option.style.display =
        option.value && fromNum && parseInt(option.value) < fromNum ? 'none' : '';
    }
  });

  // The "To" side of a range must never come before the "From" side.
  // Only checks pairs where both values are present; empty fields are ignored.
  function getRangeErrors() {
    const errors = [];
    const num = (el) => {
      const v = parseFloat(el ? el.value : '');
      return isNaN(v) ? null : v;
    };

    const fJuz = num(fromJuz),
      tJuz = num(toJuz);
    if (fJuz !== null && tJuz !== null && fJuz > tJuz) errors.push('الجزء');

    const fPage = num(fromPage),
      tPage = num(toPage);
    if (fPage !== null && tPage !== null && fPage > tPage) errors.push('الصفحة');

    const fSurah = num(fromSurah),
      tSurah = num(toSurah);
    if (fSurah !== null && tSurah !== null && fSurah > tSurah) errors.push('السورة');

    // Ayah order is only meaningful within the same surah (or when none is chosen).
    const fAyah = num(fromAyah),
      tAyah = num(toAyah);
    const sameSurah = (fSurah === null && tSurah === null) || fSurah === tSurah;
    if (sameSurah && fAyah !== null && tAyah !== null && fAyah > tAyah) {
      errors.push('الآية');
    }

    return errors;
  }

  // Only the quantity is required to assign a wird.
  form.addEventListener('submit', function (event) {
    const rangeErrors = getRangeErrors();
    if (rangeErrors.length) {
      event.preventDefault();
      event.stopImmediatePropagation();
      Swal.fire({
        icon: 'warning',
        title: 'ترتيب غير صحيح',
        text:
          'قيمة "إلى" يجب ألا تكون قبل قيمة "من" في: ' + rangeErrors.join('، '),
        confirmButtonText: 'حسناً',
      });
      return;
    }

    if (isEditMode) return;

    const missingType = !typeField || !typeField.value;
    const missingAmount = !amount || !amount.value || parseFloat(amount.value) <= 0;
    const missingUnit = !amountUnit || !amountUnit.value;

    if (missingType || missingAmount || missingUnit) {
      event.preventDefault();
      event.stopImmediatePropagation();
      Swal.fire({
        icon: 'warning',
        title: 'تنبيه',
        text: 'الرجاء اختيار نوع الورد وإدخال المقدار والوحدة',
        confirmButtonText: 'حسناً',
      });
    }
  });
})();
