/**
 * ============================================================================
 * Wird Drawer - DOM Renderer & Field Population Controller
 * ============================================================================
 * Handles caching DOM elements, rendering student headers, and applying
 * dirty-checked, high-performance DOM updates across wird cards.
 * ============================================================================
 */

// Cache for DOM references to eliminate repeated getElementById queries
const _drawerDomElementsCache = {};

function getWirdDomRef(typeKey) {
  if (!_drawerDomElementsCache[typeKey]) {
    _drawerDomElementsCache[typeKey] = {
      toggle: document.getElementById(`wtoggle-${typeKey}`),
      box: document.getElementById(`wbox-${typeKey}`),
      statusLabel: document.getElementById(`wstatus-${typeKey}`),
      amountInput: document.getElementById(`amount-${typeKey}`),
      equivGroup: document.getElementById(`equivGroup-${typeKey}`),
      fromSurah: document.getElementById(`fromSurah-${typeKey}`),
      fromAyah: document.getElementById(`fromAyah-${typeKey}`),
      toSurah: document.getElementById(`toSurah-${typeKey}`),
      toAyah: document.getElementById(`toAyah-${typeKey}`),
      noteInput: document.getElementById(`note-${typeKey}`),
      upcomingToggle: document.getElementById(`isUpcoming-${typeKey}`)
    };
  }
  return _drawerDomElementsCache[typeKey];
}

function renderDrawerStudentHeader(student, index) {
  if (!student) return;

  const studentIdInput = document.getElementById('StudentId');
  if (studentIdInput && studentIdInput.value !== String(student.id)) studentIdInput.value = student.id;

  const nameEl = document.getElementById('drawerStudentName');
  if (nameEl && nameEl.textContent !== student.name) nameEl.textContent = student.name;

  const avatarEl = document.getElementById('drawerAvatar');
  if (avatarEl) {
    const targetHtml = student.initials
      ? `<span>${student.initials}</span>`
      : `<i class='bx bx-user'></i>`;
    if (avatarEl.innerHTML !== targetHtml) {
      avatarEl.innerHTML = targetHtml;
    }
  }

  const counterEl = document.getElementById('drawerStudentCounter');
  if (counterEl) {
    const totalCount = window.classStudents ? window.classStudents.length : 1;
    const targetCounter = `طالب ${index + 1} من ${totalCount}`;
    if (counterEl.textContent !== targetCounter) counterEl.textContent = targetCounter;
  }

  const levelTag = document.getElementById('drawerStudentLevelTag');
  if (levelTag) {
    const targetLevel = student.level ? `• ${student.level}` : '';
    if (levelTag.textContent !== targetLevel) levelTag.textContent = targetLevel;
  }
}

function renderStudentWirdsFields(student) {
  if (!student || !student.wirds) return;

  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const wird = student.wirds[typeKey];
    if (!wird) return;

    const el = getWirdDomRef(typeKey);

    // 1. Active toggle & Card styling (Dirty Check)
    if (el.toggle && el.toggle.checked !== wird.active) {
      el.toggle.checked = wird.active;
    }
    if (el.box) {
      el.box.classList.toggle('is-disabled', !wird.active);
    }
    if (el.statusLabel) {
      const targetLabel = wird.active ? 'مفعل' : 'معطل';
      if (el.statusLabel.textContent !== targetLabel) {
        el.statusLabel.textContent = targetLabel;
        el.statusLabel.style.color = wird.active ? '#16a34a' : 'var(--text-medium)';
      }
    }

    // 2. Amount & Unit
    const targetAmount = wird.amount ? String(wird.amount) : '';
    if (el.amountInput && el.amountInput.value !== targetAmount) {
      el.amountInput.value = targetAmount;
    }

    if (typeof updateUnitRadioUI === 'function') {
      updateUnitRadioUI(typeKey, wird.amountUnit);
    }

    // 3. Equivalent pages chips
    if (el.equivGroup) {
      const shouldShowEquiv = (wird.amountUnit === 1);
      const targetDisplay = shouldShowEquiv ? 'block' : 'none';
      if (el.equivGroup.style.display !== targetDisplay) {
        el.equivGroup.style.display = targetDisplay;
      }
      if (shouldShowEquiv) {
        const prevActiveChip = el.equivGroup.querySelector('.chip.is-active');
        const targetChip = el.equivGroup.querySelector(`.chip[data-val="${wird.equivalentPages}"]`);
        if (prevActiveChip !== targetChip) {
          if (prevActiveChip) prevActiveChip.classList.remove('is-active');
          if (targetChip) targetChip.classList.add('is-active');
        }
      }
    }

    // 4. Quran Surah & Ayah Range (Dirty Check)
    if (el.fromSurah && wird.fromSurah && el.fromSurah.value !== String(wird.fromSurah)) {
      el.fromSurah.value = wird.fromSurah;
    }
    const targetFromAyah = wird.fromAyah ? String(wird.fromAyah) : '';
    if (el.fromAyah && el.fromAyah.value !== targetFromAyah) {
      el.fromAyah.value = targetFromAyah;
    }

    if (el.toSurah && wird.toSurah && el.toSurah.value !== String(wird.toSurah)) {
      el.toSurah.value = wird.toSurah;
    }
    const targetToAyah = wird.toAyah ? String(wird.toAyah) : '';
    if (el.toAyah && el.toAyah.value !== targetToAyah) {
      el.toAyah.value = targetToAyah;
    }

    // 5. Rating
    if (typeof updateRatingUI === 'function') {
      updateRatingUI(typeKey, wird.rating);
    }

    // 6. Notes & Upcoming toggle
    const targetNote = wird.note || '';
    if (el.noteInput && el.noteInput.value !== targetNote) {
      el.noteInput.value = targetNote;
    }

    const isUpcomingBool = !!wird.isUpcoming;
    if (el.upcomingToggle && el.upcomingToggle.checked !== isUpcomingBool) {
      el.upcomingToggle.checked = isUpcomingBool;
    }

    if (typeof syncUpcomingRatingState === 'function') {
      syncUpcomingRatingState(typeKey, isUpcomingBool);
    }
  });
}
