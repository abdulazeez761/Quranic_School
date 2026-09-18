/**
 * ============================================================================
 * Wird Drawer - Navigation & Drawer UI Lifecycle Controller
 * ============================================================================
 * Handles drawer visibility, loading student data into the UI fields,
 * previous/next stepper navigation, tab filtering, and keyboard shortcuts.
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

  // Populate drawer synchronously with current student data (0ms latency)
  if (
    window.classStudents &&
    window.classStudents.length > 0 &&
    typeof loadStudentIntoDrawer === 'function'
  ) {
    loadStudentIntoDrawer(window.selectedStudentIndex);
  }

  // Make visible to prepare compositing layer
  drawer.style.visibility = 'visible';
  backdrop.style.visibility = 'visible';

  // Decouple animation from click event listener frame to eliminate LoAF delays
  requestAnimationFrame(() => {
    drawer.classList.add('is-open', 'active', 'mobile-open');
    backdrop.classList.add('is-open', 'active');
    document.body.style.overflow = 'hidden';
  });
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
  if (!studentId) return;

  if (!window.classStudents || window.classStudents.length === 0) {
    if (Array.isArray(window.allClassStudents) && window.allClassStudents.length > 0) {
      window.classStudents = window.allClassStudents.map(s =>
        createStudentState(s.id, s.name, s.initials, s.level)
      );
    }
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

  // Populate cards state into DOM synchronously (instant 0ms render!)
  renderStudentWirdsFields(student);

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

  // Auto-fetch student complete database context (plan, last wirds, today wirds)
  if (typeof ensureStudentContextLoaded === 'function' && !student.isContextLoaded) {
    ensureStudentContextLoaded(student)
      .then(() => {
        // Only update fields if this student is still the currently selected one
        if (window.classStudents[window.selectedStudentIndex]?.id === student.id) {
          renderStudentWirdsFields(student);
        }
      })
      .catch((err) => console.warn('Background context load error:', err));
  }
}

function renderStudentWirdsFields(student) {
  if (!student || !student.wirds) return;

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
      const prevActiveChip = equivGroup.querySelector('.chip.is-active');
      const targetChip = equivGroup.querySelector(`.chip[data-val="${wird.equivalentPages}"]`);
      if (prevActiveChip !== targetChip) {
        if (prevActiveChip) prevActiveChip.classList.remove('is-active');
        if (targetChip) targetChip.classList.add('is-active');
      }
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
}

async function prevStudent() {
  const prevBtn = document.getElementById('drawerPrevBtn');
  if (prevBtn?.disabled) return;

  if (window.selectedStudentIndex > 0) {
    if (typeof saveDrawerWirds === 'function') {
      await saveDrawerWirds(false);
    }
    window.selectedStudentIndex--;
    loadStudentIntoDrawer(window.selectedStudentIndex);
  } else if (typeof showDrawerToast === 'function') {
    showDrawerToast('أنت في بداية قائمة طلاب الحلقة', 'info');
  }
}

async function saveAndNextStudent() {
  const nextBtn = document.getElementById('drawerSaveNextBtn');
  const saveBtn = document.getElementById('drawerSaveBtn');
  const prevBtn = document.getElementById('drawerPrevBtn');

  // Prevent double-click race condition
  if (nextBtn?.disabled) return;

  // Set loading state and lock buttons
  if (nextBtn) {
    nextBtn.disabled = true;
    nextBtn.dataset.originalHtml = nextBtn.innerHTML;
    nextBtn.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري الحفظ...";
  }
  if (saveBtn) saveBtn.disabled = true;
  if (prevBtn) prevBtn.disabled = true;

  try {
    let success = true;
    if (typeof saveDrawerWirds === 'function') {
      success = await saveDrawerWirds(false);
    }

    if (success) {
      if (window.selectedStudentIndex < window.classStudents.length - 1) {
        window.selectedStudentIndex++;
        loadStudentIntoDrawer(window.selectedStudentIndex);
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
  } catch (err) {
    console.error('Error during saveAndNextStudent:', err);
    if (typeof showDrawerToast === 'function') {
      showDrawerToast('تعذر إكمال الحفظ، يرجى إعادة المحاولة.', 'error');
    }
  } finally {
    if (nextBtn) {
      nextBtn.disabled = false;
      nextBtn.innerHTML = nextBtn.dataset.originalHtml || '<span id="drawerNextBtnText">حفظ والتالي</span> <i class=\'bx bx-chevron-left\'></i>';
    }
    if (saveBtn) saveBtn.disabled = false;
    if (prevBtn) {
      prevBtn.disabled = window.selectedStudentIndex === 0;
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
  document.addEventListener('keydown', async (e) => {
    const drawer = document.getElementById('drawerPanel');
    const isOpen =
      drawer &&
      (drawer.classList.contains('active') ||
        drawer.classList.contains('mobile-open') ||
        drawer.classList.contains('is-open'));

    if (!isOpen) return;

    if (e.key === 'Escape') {
      closeWirdDrawer();
    } else if (e.ctrlKey && e.key === 'Enter') {
      e.preventDefault();
      await saveAndNextStudent();
    } else if (e.ctrlKey && (e.key === 's' || e.key === 'S' || e.key === 'س')) {
      e.preventDefault();
      if (typeof saveDrawerWirds === 'function') {
        await saveDrawerWirds(false);
      }
    }
  });
}

// Initialize on DOM Ready
document.addEventListener('DOMContentLoaded', () => {
  if (typeof initializeClassStudents === 'function') {
    initializeClassStudents();
  }
  setupDrawerKeyboardEvents();
});
