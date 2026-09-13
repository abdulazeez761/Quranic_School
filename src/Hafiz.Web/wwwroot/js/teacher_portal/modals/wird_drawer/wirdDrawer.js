/**
 * ============================================================================
 * Wird Drawer - Navigation & Drawer UI Lifecycle Controller
 * ============================================================================
 * Handles drawer visibility, loading student data into the UI fields,
 * previous/next stepper navigation, tab filtering, and keyboard shortcuts.
 * ============================================================================
 */

async function openWirdDrawer() {
  const drawer = document.getElementById('drawerPanel');
  const backdrop = document.getElementById('drawerBackdrop');
  if (drawer && backdrop) {
    if (
      window.selectedStudentIndex === undefined ||
      window.selectedStudentIndex === null ||
      window.selectedStudentIndex < 0
    ) {
      window.selectedStudentIndex = 0;
    }
    if (
      window.classStudents &&
      window.classStudents.length > 0 &&
      typeof loadStudentIntoDrawer === 'function'
    ) {
      await loadStudentIntoDrawer(window.selectedStudentIndex);
    }
    drawer.classList.add('active');
    drawer.classList.add('mobile-open');
    backdrop.classList.add('active');
    document.body.style.overflow = 'hidden';
  }
}

function closeWirdDrawer() {
  const drawer = document.getElementById('drawerPanel');
  const backdrop = document.getElementById('drawerBackdrop');
  if (!drawer || !drawer.classList.contains('active')) return; // إذا كان مغلقاً أصلاً، اخرج مباشرة بدون فعل أي شيء

  if (drawer && backdrop) {
    drawer.classList.remove('active');
    drawer.classList.remove('mobile-open');
    backdrop.classList.remove('active');
    document.body.style.overflow = '';
  }
}

function closeWirdModal() {
  closeWirdDrawer();
}

async function openWirdModal(studentId, studentName) {
  let index = window.classStudents.findIndex((s) => s.id === studentId);

  if (index === -1) {
    if (typeof createStudentState === 'function') {
      const newStudent = createStudentState(studentId, studentName);
      window.classStudents.push(newStudent);
      index = window.classStudents.length - 1;
    } else {
      return;
    }
  }

  window.selectedStudentIndex = index;
  await loadStudentIntoDrawer(window.selectedStudentIndex);
  openWirdDrawer();
}

async function loadStudentIntoDrawer(index) {
  if (index < 0 || index >= window.classStudents.length) return;
  const student = window.classStudents[index];

  // Auto-apply student routine plan if found and not yet loaded
  if (
    typeof ensureStudentBaselineLoaded === 'function' &&
    !student.isPlanLoaded
  ) {
    await ensureStudentBaselineLoaded(student);
  }

  const studentIdInput = document.getElementById('StudentId');
  if (studentIdInput) studentIdInput.value = student.id;

  const nameEl = document.getElementById('drawerStudentName');
  if (nameEl) nameEl.textContent = student.name;

  const avatarEl = document.getElementById('drawerAvatar');
  if (avatarEl) {
    avatarEl.innerHTML = student.initials
      ? `<span>${student.initials}</span>`
      : `<i class='bx bx-user'></i>`;
  }

  const counterEl = document.getElementById('drawerStudentCounter');
  if (counterEl) {
    counterEl.textContent = `طالب ${index + 1} من ${window.classStudents.length}`;
  }

  const levelTag = document.getElementById('drawerStudentLevelTag');
  if (levelTag) {
    levelTag.textContent = student.level ? `• ${student.level}` : '';
  }

  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const wird = student.wirds[typeKey];
    if (!wird) return;

    const toggle = document.getElementById(`wtoggle-${typeKey}`);
    const box = document.getElementById(`wbox-${typeKey}`);
    const statusLabel = document.getElementById(`wstatus-${typeKey}`);
    if (toggle) toggle.checked = wird.active;
    if (box) box.classList.toggle('is-disabled', !wird.active);
    if (statusLabel) {
      statusLabel.textContent = wird.active ? 'مفعل' : 'معطل';
      statusLabel.style.color = wird.active ? '#16a34a' : 'var(--text-medium)';
    }

    const amountInput = document.getElementById(`amount-${typeKey}`);
    if (amountInput) amountInput.value = wird.amount || '';

    if (typeof updateUnitRadioUI === 'function') {
      updateUnitRadioUI(typeKey, wird.amountUnit);
    }

    const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
    if (equivGroup) {
      equivGroup.style.display = wird.amountUnit === 1 ? 'block' : 'none';
      const chips = equivGroup.querySelectorAll('.chip');
      chips.forEach((c) => {
        const cVal = parseFloat(c.dataset.val);
        c.classList.toggle('is-active', cVal === wird.equivalentPages);
      });
    }

    const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
    const fromAyah = document.getElementById(`fromAyah-${typeKey}`);
    const toSurah = document.getElementById(`toSurah-${typeKey}`);
    const toAyah = document.getElementById(`toAyah-${typeKey}`);

    if (fromSurah && wird.fromSurah) fromSurah.value = wird.fromSurah;
    if (fromAyah) fromAyah.value = wird.fromAyah || '';
    if (toSurah && wird.toSurah) toSurah.value = wird.toSurah;
    if (toAyah) toAyah.value = wird.toAyah || '';

    if (typeof updateRatingUI === 'function') {
      updateRatingUI(typeKey, wird.rating);
    }

    const noteInput = document.getElementById(`note-${typeKey}`);
    if (noteInput) noteInput.value = wird.note || '';

    const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
    if (upcomingToggle) upcomingToggle.checked = !!wird.isUpcoming;

    if (typeof syncUpcomingRatingState === 'function') {
      syncUpcomingRatingState(typeKey, !!wird.isUpcoming);
    }
  });

  const prevBtn = document.getElementById('drawerPrevBtn');
  if (prevBtn) {
    const isFirst = index === 0;
    prevBtn.disabled = isFirst;
    prevBtn.classList.toggle('is-disabled', isFirst);
  }

  const nextBtnText = document.getElementById('drawerNextBtnText');
  if (nextBtnText) {
    if (index === window.classStudents.length - 1) {
      nextBtnText.textContent = '🎉 حفظ وإنهاء الحلقة';
    } else {
      nextBtnText.textContent = 'حفظ والتالي';
    }
  }
}

async function prevStudent() {
  if (window.selectedStudentIndex > 0) {
    if (typeof saveDrawerWirds === 'function') {
      saveDrawerWirds(false);
    }
    window.selectedStudentIndex--;
    await loadStudentIntoDrawer(window.selectedStudentIndex);
  } else if (typeof showDrawerToast === 'function') {
    showDrawerToast('أنت في بداية قائمة طلاب الحلقة', 'info');
  }
}

async function saveAndNextStudent() {
  if (typeof saveDrawerWirds === 'function') {
    saveDrawerWirds(false);
  }

  if (window.selectedStudentIndex < window.classStudents.length - 1) {
    window.selectedStudentIndex++;
    await loadStudentIntoDrawer(window.selectedStudentIndex);
    const body = document.getElementById('drawerWirdsList');
    if (body) body.scrollTop = 0;
  } else {
    closeWirdDrawer();
    if (typeof Swal !== 'undefined') {
      Swal.fire({
        icon: 'success',
        title: '🎉 اكتملت الحلقة بنجاح!',
        text: `تم حفظ وتقييم أوراد جميع طلاب الحلقة (${window.classStudents.length} طالباً) بنجاح تام.`,
        confirmButtonText: 'ممتاز',
        confirmButtonColor: '#059669',
      });
    }
  }
}

function setWirdFilterTab(tabKey, btn) {
  window.currentFilterTab = tabKey;
  document
    .querySelectorAll('.segmented-nav .segmented-btn')
    .forEach((b) => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const boxes = document.querySelectorAll('#drawerWirdsList .drawer-wird-box');
  boxes.forEach((box) => {
    const bType = box.dataset.type;
    const visible = tabKey === 'all' || tabKey === bType;
    box.style.display = visible ? 'block' : 'none';
  });
}

function setupDrawerKeyboardEvents() {
  document.addEventListener('keydown', (e) => {
    const drawer = document.getElementById('drawerPanel');
    const isOpen =
      drawer &&
      (drawer.classList.contains('active') ||
        drawer.classList.contains('mobile-open'));

    if (!isOpen) return;

    if (e.key === 'Escape') {
      closeWirdDrawer();
    } else if (e.ctrlKey && e.key === 'Enter') {
      e.preventDefault();
      saveAndNextStudent();
    } else if (e.ctrlKey && (e.key === 's' || e.key === 'S' || e.key === 'س')) {
      e.preventDefault();
      if (typeof saveDrawerWirds === 'function') {
        saveDrawerWirds(false);
      }
    }
  });
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
  if (typeof discoverStudentsFromPage === 'function') {
    discoverStudentsFromPage();
  }
  setupDrawerKeyboardEvents();
});
