/**
 * ============================================================================
 * Wird Drawer - State & Student Discovery Module
 * ============================================================================
 * Manages drawer data structures, active student selection, draft caching,
 * and page-level student element discovery.
 * ============================================================================
 */

// Global State
window.classStudents = window.classStudents || [];
window.selectedStudentIndex = window.selectedStudentIndex || 0;
window.currentFilterTab = window.currentFilterTab || 'all';

// Wird Type Definitions matching backend enums
window.WIRD_TYPE_CONFIG = {
    memorization: {
        id: 'memorization',
        title: 'حفظ جديد (الورد اليومي)',
        typeCode: 1, // AssignmentType.Memorization
        color: '#2563eb',
        defaultAmount: 1.0,
        defaultUnit: 0 // Pages
    },
    recentRevision: {
        id: 'recentRevision',
        title: 'مراجعة صغرى (ورد قريب)',
        typeCode: 2, // AssignmentType.Revision
        color: '#059669',
        defaultAmount: 5.0,
        defaultUnit: 0 // Pages
    },
    oldRevision: {
        id: 'oldRevision',
        title: 'مراجعة كبرى (ورد بعيد)',
        typeCode: 2, // AssignmentType.Revision
        color: '#d97706',
        defaultAmount: 1.0,
        defaultUnit: 2 // Juz
    },
    recitation: {
        id: 'recitation',
        title: 'تجويد وإتقان التلاوة',
        typeCode: 3, // AssignmentType.Tajwid / Recitation
        color: '#7c3aed',
        defaultAmount: 2.0,
        defaultUnit: 0 // Pages
    }
};

window.RATING_GRADES = {
    excellent: { grade: 'excellent', label: 'ممتاز', status: 5, icon: 'bxs-star', classSuffix: 'excellent' },
    very_good: { grade: 'very_good', label: 'جيد جداً', status: 4, icon: 'bxs-like', classSuffix: 'very_good' },
    good: { grade: 'good', label: 'جيد', status: 3, icon: 'bx-check-circle', classSuffix: 'good' },
    acceptable: { grade: 'acceptable', label: 'مقبول', status: 2, icon: 'bx-minus-circle', classSuffix: 'acceptable' },
    weak: { grade: 'weak', label: 'ضعيف', status: 1, icon: 'bx-error-circle', classSuffix: 'weak' }
};

/**
 * Creates a clean student state representation without mock values
 */
function createStudentState(id, name, initials = '', level = '', juz = '') {
    return {
        id: id,
        name: name || 'طالب',
        initials: initials || (name ? name.split(' ').slice(0, 2).map(n => n[0]).join('') : 'ط'),
        level: level,
        juz: juz,
        routinePlan: null,
        isPlanLoaded: false,
        wirds: {
            memorization: {
                active: true,
                type: 1,
                amount: '',
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '',
                toSurah: 1,
                toAyah: '',
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            recentRevision: {
                active: false,
                type: 2,
                amount: '',
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '',
                toSurah: 1,
                toAyah: '',
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            oldRevision: {
                active: false,
                type: 2,
                amount: '',
                amountUnit: 2,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '',
                toSurah: 1,
                toAyah: '',
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            },
            recitation: {
                active: false,
                type: 3,
                amount: '',
                amountUnit: 0,
                equivalentPages: null,
                fromSurah: 1,
                fromAyah: '',
                toSurah: 1,
                toAyah: '',
                rating: null,
                status: 0,
                note: '',
                isUpcoming: false
            }
        }
    };
}

/**
 * Scans cards and table rows on the Student/Index page to populate classStudents
 */
function discoverStudentsFromPage() {
    window.classStudents = [];
    const discoveredMap = new Map();

    // 1. Scan Grid Cards
    const cards = document.querySelectorAll('#studentsGrid .student-card');
    cards.forEach(card => {
        const nameEl = card.querySelector('.student-name');
        const addBtn = card.querySelector('button[onclick*="openWirdModal"]');
        let studentId = '';
        let studentName = '';

        if (addBtn) {
            const match = addBtn.getAttribute('onclick').match(/openWirdModal\('([^']+)',\s*'([^']+)'\)/);
            if (match) {
                studentId = match[1];
                studentName = match[2];
            }
        }

        if (!studentId) {
            const detailLink = card.querySelector('a[href*="/Details/"]');
            if (detailLink) {
                const parts = detailLink.getAttribute('href').split('/');
                studentId = parts[parts.length - 1];
            }
        }

        if (!studentName && nameEl) {
            studentName = nameEl.childNodes[0]?.textContent?.trim() || nameEl.textContent.trim();
        }

        if (studentId && !discoveredMap.has(studentId)) {
            const initials = card.querySelector('.student-avatar span')?.textContent?.trim() || '';
            const tajwidTag = card.querySelector('.chip-tajwid')?.textContent?.trim() || '';
            const juzMemorized = card.querySelector('.memorization-value')?.textContent?.trim() || '';

            const studentObj = createStudentState(studentId, studentName, initials, tajwidTag, juzMemorized);
            discoveredMap.set(studentId, studentObj);
            window.classStudents.push(studentObj);
        }
    });

    // 2. Scan Table Rows as fallback
    const rows = document.querySelectorAll('#studentsTableWrap .student-row');
    rows.forEach(row => {
        const addBtn = row.querySelector('button[onclick*="openWirdModal"]');
        if (addBtn) {
            const match = addBtn.getAttribute('onclick').match(/openWirdModal\('([^']+)',\s*'([^']+)'\)/);
            if (match) {
                const studentId = match[1];
                const studentName = match[2];
                if (!discoveredMap.has(studentId)) {
                    const initials = row.querySelector('.row-avatar span')?.textContent?.trim() || '';
                    const tajwidTag = row.querySelector('.row-chip')?.textContent?.trim() || '';
                    const juzMemorized = row.querySelector('.row-juz-value')?.textContent?.trim() || '';

                    const studentObj = createStudentState(studentId, studentName, initials, tajwidTag, juzMemorized);
                    discoveredMap.set(studentId, studentObj);
                    window.classStudents.push(studentObj);
                }
            }
        }
    });

    loadCachedDrafts();

    // Prefetch all class baseline plans into cache for instant opening
    prefetchClassBaselinePlans();

    const pillBtn = document.getElementById('mobileOpenPillBtn');
    if (pillBtn && window.classStudents.length > 0) {
        pillBtn.style.display = window.innerWidth <= 1100 ? 'flex' : 'none';
    }
}

/**
 * Prefetches routine plans for all students in the active class
 */
async function prefetchClassBaselinePlans() {
    try {
        const classIdMatch = document.cookie.match(/selectedClassId=([^;]+)/);
        const classId = classIdMatch ? classIdMatch[1] : (document.getElementById('ClassId')?.value || null);
        if (!classId) return;

        const res = await fetch(`/Teacher/StudentRoutinePlan/GetPlansByClassId?classId=${encodeURIComponent(classId)}`);
        const json = await res.json();
        if (json && json.success && Array.isArray(json.data)) {
            window.studentPlansCache = window.studentPlansCache || {};
            json.data.forEach(p => {
                if (p && p.studentId) {
                    window.studentPlansCache[p.studentId] = p;
                    const student = window.classStudents.find(s => s.id === p.studentId);
                    if (student) {
                        student.routinePlan = p;
                    }
                }
            });
        }
    } catch (e) {
        // Silent fallback - individual students will fetch on demand
    }
}

/**
 * Applies a plan object to a student's wirds state
 */
function applyPlanToStudentWirds(student, plan) {
    if (!student || !plan) return;

    const memActive = plan.memActive !== undefined ? plan.memActive : (plan.MemActive !== undefined ? plan.MemActive : true);
    const memAmount = plan.memAmount ?? plan.MemAmount ?? '';
    const memUnit = plan.memUnit ?? plan.MemUnit ?? 0;
    const memEquivalentPages = plan.memEquivalentPages ?? plan.MemEquivalentPages ?? null;

    const recentRevActive = plan.recentRevActive !== undefined ? plan.recentRevActive : (plan.RecentRevActive !== undefined ? plan.RecentRevActive : true);
    const recentRevAmount = plan.recentRevAmount ?? plan.RecentRevAmount ?? '';
    const recentRevUnit = plan.recentRevUnit ?? plan.RecentRevUnit ?? 0;

    const oldRevActive = plan.oldRevActive !== undefined ? plan.oldRevActive : (plan.OldRevActive !== undefined ? plan.OldRevActive : true);
    const oldRevAmount = plan.oldRevAmount ?? plan.OldRevAmount ?? '';
    const oldRevUnit = plan.oldRevUnit ?? plan.OldRevUnit ?? 2;

    const recitationActive = plan.recitationActive !== undefined ? plan.recitationActive : (plan.RecitationActive !== undefined ? plan.RecitationActive : true);
    const recitationAmount = plan.recitationAmount ?? plan.RecitationAmount ?? '';
    const recitationUnit = plan.recitationUnit ?? plan.RecitationUnit ?? 0;

    const defaultNote = plan.defaultNote ?? plan.DefaultNote ?? '';

    student.wirds.memorization.active = memActive !== false;
    student.wirds.memorization.amount = memAmount;
    student.wirds.memorization.amountUnit = memUnit;
    if (memEquivalentPages) {
        student.wirds.memorization.equivalentPages = memEquivalentPages;
    }

    student.wirds.recentRevision.active = recentRevActive !== false;
    student.wirds.recentRevision.amount = recentRevAmount;
    student.wirds.recentRevision.amountUnit = recentRevUnit;

    student.wirds.oldRevision.active = oldRevActive !== false;
    student.wirds.oldRevision.amount = oldRevAmount;
    student.wirds.oldRevision.amountUnit = oldRevUnit;

    student.wirds.recitation.active = recitationActive !== false;
    student.wirds.recitation.amount = recitationAmount;
    student.wirds.recitation.amountUnit = recitationUnit;

    if (defaultNote && !student.wirds.memorization.note) {
        student.wirds.memorization.note = defaultNote;
    }
}

/**
 * Ensures a student's baseline routine plan is fetched and applied if present
 */
async function ensureStudentBaselineLoaded(student) {
    if (!student || student.isPlanLoaded) return;
    student.isPlanLoaded = true;

    let plan = student.routinePlan || (window.studentPlansCache && window.studentPlansCache[student.id]);
    if (!plan) {
        try {
            const res = await fetch(`/Teacher/StudentRoutinePlan/GetPlanByStudentId?studentId=${encodeURIComponent(student.id)}`);
            const json = await res.json();
            if (json && json.success && json.data) {
                plan = json.data;
                student.routinePlan = plan;
                window.studentPlansCache = window.studentPlansCache || {};
                window.studentPlansCache[student.id] = plan;
            }
        } catch (e) {
            console.warn('Could not auto-fetch routine plan for student:', e);
        }
    }

    if (plan) {
        // Only auto-apply if the student does not already have amounts typed in
        const hasCustomAmounts = !!(
            student.wirds.memorization.amount ||
            student.wirds.recentRevision.amount ||
            student.wirds.oldRevision.amount ||
            student.wirds.recitation.amount
        );

        if (!hasCustomAmounts) {
            applyPlanToStudentWirds(student, plan);
        }
    }
}

/**
 * LocalStorage draft management
 */
function saveDraftsToCache() {
    try {
        localStorage.setItem('hafiz_class_wird_drafts', JSON.stringify(window.classStudents));
    } catch (e) { }
}

function loadCachedDrafts() {
    try {
        const raw = localStorage.getItem('hafiz_class_wird_drafts');
        if (!raw) return;
        const cached = JSON.parse(raw);
        if (Array.isArray(cached)) {
            cached.forEach(savedStudent => {
                const existing = window.classStudents.find(s => s.id === savedStudent.id);
                if (existing && savedStudent.wirds) {
                    existing.wirds = savedStudent.wirds;
                }
            });
        }
    } catch (e) { }
}

// React to student plan updates dispatched by StudentRoutinePlanModal
window.addEventListener('hafiz:studentPlanSaved', (e) => {
    if (!e.detail || !e.detail.studentId) return;
    const student = window.classStudents.find(s => s.id === e.detail.studentId);
    if (student) {
        student.routinePlan = e.detail.plan;
        student.isPlanLoaded = true;
        if (e.detail.plan) {
            applyPlanToStudentWirds(student, e.detail.plan);
        }
        // If this student is currently being viewed in the drawer, re-render drawer inputs
        if (window.classStudents[window.selectedStudentIndex]?.id === student.id && typeof loadStudentIntoDrawer === 'function') {
            loadStudentIntoDrawer(window.selectedStudentIndex);
        }
    }
});
