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

// Toggle the EquivalentPages selector based on AmountUnit (1 = Ayahs)
(function () {
  const unitSelect = document.getElementById('AmountUnit');
  const equivalentGroup = document.getElementById('equivalentPagesGroup');
  const equivalentSelect = document.getElementById('EquivalentPages');
  if (!unitSelect || !equivalentGroup || !equivalentSelect) return;

  function syncEquivalentVisibility() {
    const isAyahs = unitSelect.value === '1';
    equivalentGroup.style.display = isAyahs ? '' : 'none';
    equivalentSelect.required = isAyahs;
    if (!isAyahs) equivalentSelect.value = '';
  }

  unitSelect.addEventListener('change', syncEquivalentVisibility);
  syncEquivalentVisibility();
})();
