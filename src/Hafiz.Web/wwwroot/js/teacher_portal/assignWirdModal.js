function openWirdModal(studentId, studentName) {
  document.getElementById('StudentId').value = studentId;
  document.getElementById('studentName').textContent = studentName;
  document.getElementById('assignWirdModal').classList.add('active');
  document.body.style.overflow = 'hidden';
}

function closeWirdModal() {
  document.getElementById('assignWirdModal').classList.remove('active');
  document.body.style.overflow = '';
  document.getElementById('wirdAssignmentForm').reset();
}

// Close modal when clicking outside
document
  .getElementById('assignWirdModal')
  ?.addEventListener('click', function (e) {
    if (e.target === this) {
      closeWirdModal();
    }
  });

// Close modal on Escape key
document.addEventListener('keydown', function (e) {
  if (
    e.key === 'Escape' &&
    document.getElementById('assignWirdModal').classList.contains('active')
  ) {
    closeWirdModal();
  }
});

// Toggle the EquivalentPages input based on AmountUnit (1 = Ayahs).
// EquivalentPages is a free decimal input (e.g. 0.2, 0.5, 2.5) so teachers can
// express any fractional page-equivalent for a scattered-ayah wird. Quick-pick
// chips underneath just set the input value for common fractions.
(function () {
  const unitSelect = document.getElementById('AmountUnit');
  const equivalentGroup = document.getElementById('equivalentPagesGroup');
  const equivalentField = document.getElementById('EquivalentPages');
  if (!unitSelect || !equivalentGroup || !equivalentField) return;

  function syncActiveChip() {
    const chips = equivalentGroup.querySelectorAll('.chip');
    const current = parseFloat(equivalentField.value);
    chips.forEach((c) => {
      const match = !isNaN(current) && parseFloat(c.dataset.value) === current;
      c.classList.toggle('is-active', match);
    });
  }

  function syncEquivalentVisibility() {
    const isAyahs = unitSelect.value === '1';
    equivalentGroup.style.display = isAyahs ? '' : 'none';
    equivalentField.required = isAyahs;
    if (!isAyahs) equivalentField.value = '';
    syncActiveChip();
  }

  unitSelect.addEventListener('change', syncEquivalentVisibility);
  equivalentField.addEventListener('input', syncActiveChip);

  equivalentGroup.querySelectorAll('.chip').forEach((chip) => {
    chip.addEventListener('click', function () {
      equivalentField.value = this.dataset.value;
      equivalentField.dispatchEvent(new Event('input', { bubbles: true }));
      equivalentField.focus();
    });
  });

  syncEquivalentVisibility();
})();
