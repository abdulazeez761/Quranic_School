/**
 * ============================================================================
 * Wird Drawer - Quick Actions & Auto-Calculation Helpers
 * ============================================================================
 * Handles applying approved student baseline routine plans, automatically
 * continuing Quran Ayahs from previous wirds, and opening the plan modal.
 * ============================================================================
 */

/**
 * Quick Action: 📋 استكمال من الأمس
 */
function autoComputeNextWirds() {
    const student = window.classStudents[window.selectedStudentIndex];
    if (!student) return;

    const surahsList = (typeof QURAN_SURAHS !== 'undefined') ? QURAN_SURAHS : [];

    // Memorization auto-continue
    const mem = student.wirds.memorization;
    if (mem && mem.active) {
        let currentSurahId = parseInt(document.getElementById('toSurah-memorization')?.value, 10) || mem.toSurah || 1;
        let currentAyah = parseInt(document.getElementById('toAyah-memorization')?.value, 10) || mem.toAyah || 1;

        const surahData = surahsList.find(s => s.id === currentSurahId);
        let nextSurahId = currentSurahId;
        let nextAyah = currentAyah + 1;

        if (surahData && nextAyah > surahData.ayahs) {
            nextSurahId = Math.min(114, currentSurahId + 1);
            nextAyah = 1;
        }

        const nextSurahData = surahsList.find(s => s.id === nextSurahId);
        const amount = mem.amount || 1.0;
        const toAyah = Math.min(nextSurahData?.ayahs || 50, nextAyah + Math.round(amount * 15));

        mem.fromSurah = nextSurahId;
        mem.fromAyah = nextAyah.toString();
        mem.toSurah = nextSurahId;
        mem.toAyah = toAyah;
    }

    // Recent revision auto-continue
    const rev = student.wirds.recentRevision;
    if (rev && rev.active) {
        let curToAyah = parseInt(document.getElementById('toAyah-recentRevision')?.value, 10) || rev.toAyah || 1;
        rev.fromAyah = (curToAyah + 1).toString();
        rev.toAyah = curToAyah + Math.round((rev.amount || 5) * 15);
    }

    if (typeof loadStudentIntoDrawer === 'function') {
        loadStudentIntoDrawer(window.selectedStudentIndex);
    }
    if (typeof showDrawerToast === 'function') {
        showDrawerToast('📋 تم استكمال نطاق الآيات تلقائياً بنجاح', 'info');
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
    } else if (typeof showDrawerToast === 'function') {
        showDrawerToast('جاري تجهيز نافذة الخطة...', 'info');
    }
}
