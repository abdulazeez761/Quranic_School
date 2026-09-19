/**
 * ============================================================================
 * Hafiz Platform - Edit Single Wird Modal Controller (Modular)
 * ============================================================================
 * Handles opening, populating, and modal lifecycle for single wird edits.
 * Coordinates between validation, service, and UI layers.
 * ============================================================================
 */

function openEditWirdModal(assignmentId, studentId, studentName) {
  const modal = document.getElementById('editWirdModal') || document.getElementById('assignWirdModal');
  const nameSpan = document.getElementById('studentName');
  const idInput = document.getElementById('StudentId');

  if (nameSpan) nameSpan.textContent = studentName || '';
  if (idInput) idInput.value = studentId || '';

  if (modal) {
    modal.classList.add('active');
    document.body.style.overflow = 'hidden';
  }

  if (assignmentId && typeof window.fetchWirdAssignmentById === 'function') {
    window.fetchWirdAssignmentById(assignmentId);
  }
}
window.openEditWirdModal = openEditWirdModal;

function closeEditWirdModal() {
  const modal = document.getElementById('editWirdModal') || document.getElementById('assignWirdModal');
  if (modal) {
    modal.classList.remove('active');
    document.body.style.overflow = '';
  }
}
window.closeEditWirdModal = closeEditWirdModal;
window.closeWirdModal = closeEditWirdModal;

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
  if (typeof syncEquivalentChips === 'function') {
    syncEquivalentChips(data.equivalentPages);
  }

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
    if (typeof syncEditModalUpcomingState === 'function') {
      syncEditModalUpcomingState(upcomingEl.checked);
    }
  }

  // Trigger FromSurah change to filter ToSurah options
  const fromSurahEl = document.getElementById('FromSurah');
  if (fromSurahEl) {
    fromSurahEl.dispatchEvent(new Event('change'));
  }
}

// Lifecycle Events & DOM Ready Setup
document.addEventListener('DOMContentLoaded', function () {
  const getModal = () => document.getElementById('editWirdModal') || document.getElementById('assignWirdModal');

  // Backdrop click to close
  document.addEventListener('click', function (e) {
    const modal = getModal();
    if (modal && e.target === modal) {
      closeEditWirdModal();
    }
  });

  // Escape key to close
  document.addEventListener('keydown', function (e) {
    const modal = getModal();
    if (e.key === 'Escape' && modal && modal.classList.contains('active')) {
      closeEditWirdModal();
    }
  });

  // Setup unit constraints, equivalent pages toggle, and chips
  if (typeof setupEditFormValidation === 'function') {
    setupEditFormValidation();
  }

  // Setup AJAX submission
  if (typeof setupEditWirdFormSubmit === 'function') {
    setupEditWirdFormSubmit();
  }
});
