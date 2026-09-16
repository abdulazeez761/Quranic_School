/**
 * ============================================================================
 * Wird Drawer - Quick Actions & Auto-Calculation Helpers
 * ============================================================================
 * Handles applying approved student baseline routine plans, automatically
 * continuing Quran Ayahs from previous wirds in the database, and opening plan modal.
 * ============================================================================
 */

/**
 * Computes and updates the Quran range for a single wird type
 * @param {object} student - Selected student object
 * @param {string} typeKey - Wird type key ('memorization', 'recentRevision', 'oldRevision', 'recitation')
 * @param {Array} surahsList - List of Quran surahs from QURAN_SURAHS
 * @returns {Promise<boolean>} True if calculated successfully
 */
async function autoComputeSingleWird(student, typeKey, surahsList) {
  const wird = student?.wirds?.[typeKey];
  if (!wird) return false;

  // If the wird is not currently active, but the student has a previous record from yesterday, activate it!
  if (!wird.active) {
    if (student.lastWirds?.[typeKey]?.toSurah) {
      wird.active = true;
    } else {
      return false;
    }
  }

  let lastSurah = 1;
  let lastAyah = 0;

  // 1. Prioritize continuation from yesterday's recorded completion in the database on first run
  if (student.lastWirds?.[typeKey]?.toSurah && !wird._alreadyAdvanced) {
    lastSurah = student.lastWirds[typeKey].toSurah;
    lastAyah = student.lastWirds[typeKey].toAyah || 0;
    wird._alreadyAdvanced = true;
  } else {
    // 2. If already advanced once or no database record, advance from current input/state
    const inputToAyah = parseInt(
      document.getElementById(`toAyah-${typeKey}`)?.value,
      10,
    );
    const inputToSurah = parseInt(
      document.getElementById(`toSurah-${typeKey}`)?.value,
      10,
    );

    if (!isNaN(inputToAyah) && inputToAyah > 0 && !isNaN(inputToSurah) && inputToSurah > 0) {
      lastSurah = inputToSurah;
      lastAyah = inputToAyah;
    } else if (wird.toSurah && wird.toAyah) {
      lastSurah = wird.toSurah;
      lastAyah = parseInt(wird.toAyah, 10) || 0;
    } else if (wird.fromSurah && wird.fromAyah) {
      lastSurah = wird.fromSurah;
      lastAyah = Math.max(0, parseInt(wird.fromAyah, 10) - 1);
    }
  }

  // Next starting position (advance by 1 ayah)
  let nextSurahId = lastSurah;
  let nextAyah = lastAyah + 1;

  // Check if ayah advances past the end of the current surah
  const surahData = surahsList.find((s) => s.id === nextSurahId);
  if (surahData && nextAyah > surahData.ayahs) {
    if (nextSurahId < 114) {
      nextSurahId++;
      nextAyah = 1;
    } else {
      nextAyah = surahData.ayahs;
    }
  }

  // Resolve amount and amountUnit
  const inputAmount = parseFloat(document.getElementById(`amount-${typeKey}`)?.value);
  const fallbackAmount = wird.amount || (typeKey === 'recentRevision' ? 5.0 : 1.0);
  const amount = !isNaN(inputAmount) && inputAmount > 0 ? inputAmount : fallbackAmount;
  wird.amount = amount;

  const defaultConfigUnit = window.WIRD_TYPE_CONFIG?.[typeKey]?.defaultUnit ?? 0;
  const amountUnit = wird.amountUnit !== undefined ? wird.amountUnit : defaultConfigUnit;

  // Compute target destination range
  const target = await computeTargetRange(
    nextSurahId,
    nextAyah,
    amount,
    amountUnit,
  );

  if (!target) return false;

  wird.fromSurah = nextSurahId;
  wird.fromAyah = nextAyah.toString();
  wird.toSurah = target.toSurah;
  wird.toAyah = target.toAyah;

  return true;
}

/**
 * Quick Action: 📋 استكمال من الأمس
 * Automatically computes next ranges for all active wirds based on previous database records.
 * Provides real-time disabled state and rotating spinner indicator while calculating.
 */
async function autoComputeNextWirds(triggerBtn) {
  const btn =
    triggerBtn ||
    document.getElementById('btnAutoComputeWirds') ||
    document.querySelector('.drawer-quick-actions button[onclick*="autoComputeNextWirds"]');

  if (btn) {
    if (btn.disabled || btn.classList.contains('is-loading')) return;
    btn.disabled = true;
    btn.classList.add('is-loading');
    if (!btn.dataset.originalHtml) {
      btn.dataset.originalHtml = btn.innerHTML;
    }
    btn.innerHTML =
      "<i class='bx bx-loader-alt bx-spin' style='color: #0284c7; font-size: 1.2rem;'></i> <span>جاري الحساب...</span>";
  }

  const startTime = Date.now();

  try {
    const student = window.classStudents[window.selectedStudentIndex];
    if (!student || !student.wirds) return;

    // Ensure student database context is loaded before computing
    if (typeof ensureStudentContextLoaded === 'function' && !student.isContextLoaded) {
      await ensureStudentContextLoaded(student);
    }

    const surahsList = typeof QURAN_SURAHS !== 'undefined' ? QURAN_SURAHS : [];
    const wirdKeys = Object.keys(student.wirds);

    let anyComputed = false;

    for (const typeKey of wirdKeys) {
      const success = await autoComputeSingleWird(student, typeKey, surahsList);
      if (success) {
        anyComputed = true;
      }
    }

    if (typeof renderStudentWirdsFields === 'function') {
      renderStudentWirdsFields(student);
    }

    // Ensure smooth perceivable UI indicator (at least 350ms display)
    const elapsed = Date.now() - startTime;
    if (elapsed < 350) {
      await new Promise((resolve) => setTimeout(resolve, 350 - elapsed));
    }

    if (typeof showDrawerToast === 'function') {
      showDrawerToast(
        anyComputed
          ? '📋 تم استكمال نطاق الآيات من قاعدة البيانات بنجاح'
          : '⚠️ لا توجد أوراد سابقة أو بيانات للاستكمال',
        anyComputed ? 'info' : 'warning',
      );
    }
  } catch (err) {
    console.error('Error auto-computing next wirds:', err);
    if (typeof showDrawerToast === 'function') {
      showDrawerToast('⚠️ حدث خطأ أثناء محاولة استكمال الورد', 'error');
    }
  } finally {
    if (btn) {
      btn.disabled = false;
      btn.classList.remove('is-loading');
      btn.innerHTML =
        btn.dataset.originalHtml ||
        "<i class='bx bx-sync' style='color: #0284c7;'></i> <span>استكمال من الأمس</span>";
    }
  }
}

/**
 * Quick Action: ضبط الخطة ⚙️
 * Dispatches call to open the student routine plan modal cleanly
 */
function showBaselinePresetInfo() {
  const student = window.classStudents[window.selectedStudentIndex];
  if (!student) return;
  if (typeof openStudentPlanModal === 'function') {
    openStudentPlanModal(student.id, student.name, student.level);
    closeWirdDrawer();
  } else if (typeof showDrawerToast === 'function') {
    showDrawerToast('جاري تجهيز نافذة الخطة...', 'info');
  }
}
