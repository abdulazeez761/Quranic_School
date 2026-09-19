/**
 * ============================================================================
 * Wird Drawer - Navigation & Drawer UI Lifecycle Controller
 * ============================================================================
 * Handles drawer visibility, lifecycle transitions, loading student context,
 * and initializing the drawer on DOM Ready.
 * ============================================================================
 */

function openWirdDrawer() {
  const drawer = document.getElementById('drawerPanel');
  const backdrop = document.getElementById('drawerBackdrop');
  if (!drawer || !backdrop) return;

  if (
    window.selectedStudentIndex === undefined ||
    window.selectedStudentIndex === null ||
    window.selectedStudentIndex < 0
  ) {
    window.selectedStudentIndex = 0;
  }

  const isAlreadyOpen = drawer.classList.contains('active') || drawer.classList.contains('is-open');
  const student = window.classStudents?.[window.selectedStudentIndex];

  // Render header immediately (extremely fast < 1ms)
  if (student && typeof renderDrawerStudentHeader === 'function') {
    renderDrawerStudentHeader(student, window.selectedStudentIndex);
  }

  if (isAlreadyOpen) {
    // If drawer is already open, load student fields directly without animation delay
    loadStudentIntoDrawer(window.selectedStudentIndex);
    return;
  }

  // Prepare compositing layer
  drawer.style.visibility = 'visible';
  backdrop.style.visibility = 'visible';

  // Decouple CSS transform animation from heavy DOM rendering
  requestAnimationFrame(() => {
    drawer.classList.add('is-open', 'active', 'mobile-open');
    backdrop.classList.add('is-open', 'active');
    document.body.style.overflow = 'hidden';
  });

  // Defer heavy card fields rendering and network fetch until opening animation finishes (~200ms)
  // This guarantees 60fps smooth animation with 0 frame drops
  setTimeout(() => {
    loadStudentIntoDrawer(window.selectedStudentIndex);
  }, 200);
}

function closeWirdDrawer() {
  const drawer = document.getElementById('drawerPanel');
  const backdrop = document.getElementById('drawerBackdrop');
  if (!drawer) return;

  drawer.classList.remove('is-open', 'active', 'mobile-open');
  if (backdrop) backdrop.classList.remove('is-open', 'active');

  // Defer restoring body scroll until closing animation finishes (250ms)
  setTimeout(() => {
    if (!drawer.classList.contains('active') && !drawer.classList.contains('is-open')) {
      document.body.style.overflow = '';
      drawer.style.visibility = '';
      if (backdrop) backdrop.style.visibility = '';
    }
  }, 250);
}

function closeWirdModal() {
  closeWirdDrawer();
}

function openWirdModal(studentId, studentName) {
  if (!window.classStudents || window.classStudents.length === 0) {
    if (Array.isArray(window.allClassStudents) && window.allClassStudents.length > 0) {
      window.classStudents = window.allClassStudents.map(s =>
        createStudentState(s.id, s.name, s.initials, s.level)
      );
    }
  }

  if (!studentId) {
    // If called without studentId (e.g. from empty state button), open first student if available
    window.selectedStudentIndex = 0;
    openWirdDrawer();
    return;
  }

  let index = window.classStudents ? window.classStudents.findIndex((s) => s.id === studentId) : -1;

  if (index === -1) {
    if (typeof createStudentState === 'function') {
      const newStudent = createStudentState(studentId, studentName);
      window.classStudents = window.classStudents || [];
      window.classStudents.push(newStudent);
      index = window.classStudents.length - 1;
    }
  }

  window.selectedStudentIndex = index >= 0 ? index : 0;
  openWirdDrawer();
}

function loadStudentIntoDrawer(index) {
  if (!window.classStudents || index < 0 || index >= window.classStudents.length) return;
  const student = window.classStudents[index];

  // Update header in case it wasn't rendered yet
  if (typeof renderDrawerStudentHeader === 'function') {
    renderDrawerStudentHeader(student, index);
  }

  // Populate cards state into DOM synchronously with dirty checking
  if (typeof renderStudentWirdsFields === 'function') {
    renderStudentWirdsFields(student);
  }

  const prevBtn = document.getElementById('drawerPrevBtn');
  if (prevBtn) {
    const isFirst = index === 0;
    prevBtn.disabled = isFirst;
    prevBtn.classList.toggle('is-disabled', isFirst);
  }

  const nextBtnText = document.getElementById('drawerNextBtnText');
  if (nextBtnText) {
    const targetText = (index === window.classStudents.length - 1)
      ? '🎉 حفظ وإنهاء الحلقة'
      : 'حفظ والتالي';
    if (nextBtnText.textContent !== targetText) nextBtnText.textContent = targetText;
  }

  // Auto-fetch student complete database context (plan, last wirds, today wirds)
  if (typeof ensureStudentContextLoaded === 'function' && !student.isContextLoaded) {
    ensureStudentContextLoaded(student)
      .then(() => {
        // Only update fields if this student is still the currently selected one
        if (window.classStudents[window.selectedStudentIndex]?.id === student.id) {
          if (typeof renderStudentWirdsFields === 'function') {
            renderStudentWirdsFields(student);
          }
        }
      })
      .catch((err) => console.warn('Background context load error:', err));
  }
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
  if (typeof initializeClassStudents === 'function') {
    initializeClassStudents();
  }
  if (typeof setupDrawerKeyboardEvents === 'function') {
    setupDrawerKeyboardEvents();
  }
});
