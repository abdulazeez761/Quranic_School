/**
 * ============================================================================
 * Wird Drawer - State & Student Context Module
 * ============================================================================
 * Manages drawer data structures, active student selection, in-memory caching,
 * and database-driven context loading (Student, Routine Plan, Last & Today Wirds).
 * ============================================================================
 */

// Global State
window.classStudents = window.classStudents || [];
window.selectedStudentIndex = window.selectedStudentIndex || 0;
window.currentFilterTab = window.currentFilterTab || 'all';
window.studentContextCache = window.studentContextCache || {};

// Wird Type Definitions matching backend enums
window.WIRD_TYPE_CONFIG = {
    memorization: {
        id: 'memorization',
        title: 'حفظ جديد (الورد اليومي)',
        typeCode: 1, // AssignmentType.Memorization
        defaultUnit: 0 // Pages
    },
    recentRevision: {
        id: 'recentRevision',
        title: 'مراجعة صغرى (ورد قريب)',
        typeCode: 2, // AssignmentType.Revision
        defaultUnit: 0 // Pages
    },
    oldRevision: {
        id: 'oldRevision',
        title: 'مراجعة كبرى (ورد بعيد)',
        typeCode: 2, // AssignmentType.Revision
        defaultUnit: 2 // Juz
    },
    recitation: {
        id: 'recitation',
        title: 'تجويد وإتقان التلاوة',
        typeCode: 3, // AssignmentType.Tajwid / Recitation
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

function getGradeKeyByStatus(status) {
    if (!status) return null;
    const num = parseInt(status, 10);
    const entry = Object.entries(window.RATING_GRADES).find(([_, g]) => g.status === num);
    return entry ? entry[0] : null;
}

/**
 * Factory for creating a single wird card state
 */
function createDefaultWird(typeCode, defaultUnit = 0, active = false) {
    return {
        active,
        type: typeCode,
        amount: '',
        amountUnit: defaultUnit,
        equivalentPages: null,
        fromSurah: 1,
        fromAyah: '',
        toSurah: 1,
        toAyah: '',
        rating: null,
        status: 0,
        note: '',
        isUpcoming: false
    };
}

/**
 * Creates a clean student state representation
 */
function createStudentState(id, name, initials = '', level = '') {
    const wirds = {};
    for (const [key, cfg] of Object.entries(window.WIRD_TYPE_CONFIG)) {
        wirds[key] = createDefaultWird(cfg.typeCode, cfg.defaultUnit, key === 'memorization');
    }

    return {
        id,
        name: name || 'طالب',
        initials: initials || (name ? name.split(' ').slice(0, 2).map(n => n[0]).join('') : 'ط'),
        level: level || '',
        routinePlan: null,
        lastWirds: null,
        isContextLoaded: false,
        wirds
    };
}

/**
 * Initializes classStudents cleanly from server-provided array or API
 */
async function initializeClassStudents() {
    if (Array.isArray(window.allClassStudents) && window.allClassStudents.length > 0) {
        window.classStudents = window.allClassStudents.map(s =>
            createStudentState(s.id, s.name, s.initials, s.level)
        );
        return;
    }

    try {
        const res = await fetch('/Teacher/Student/GetClassStudentsForDrawer');
        const json = await res.json();

        if (json?.success && Array.isArray(json.data)) {
            window.classStudents = json.data.map(s =>
                createStudentState(s.id, s.name, s.initials, s.level)
            );
        }
    } catch (e) {
        console.warn('Could not fetch class students for drawer:', e);
        window.classStudents = window.classStudents || [];
    }
}

/**
 * Loads student wird context directly from the Database
 * (Student details, routine baseline plan, latest completed wirds, and today's wirds)
 */
async function ensureStudentContextLoaded(student, forceRefresh = false) {
    if (!student?.id) return;
    if (student.isContextLoaded && !forceRefresh) return;

    if (!forceRefresh && window.studentContextCache[student.id]) {
        applyContextToStudentState(student, window.studentContextCache[student.id]);
        return;
    }

    try {
        const res = await fetch(`/Teacher/Student/GetStudentWirdContext?studentId=${encodeURIComponent(student.id)}`);
        const json = await res.json();

        if (json?.success) {
            window.studentContextCache[student.id] = json;
            applyContextToStudentState(student, json);
        }
    } catch (e) {
        console.error('Error fetching student wird context from database:', e);
    }
}

/**
 * Applies database context (plan, last wirds, today's existing wirds) to student state
 */
function applyContextToStudentState(student, contextData) {
    if (!student || !contextData) return;

    student.isContextLoaded = true;
    student.routinePlan = contextData.plan || null;
    student.lastWirds = contextData.lastWirds || null;

    if (contextData.student) {
        if (contextData.student.name) student.name = contextData.student.name;
        if (contextData.student.initials) student.initials = contextData.student.initials;
        if (contextData.student.level) student.level = contextData.student.level;
    }

    const today = contextData.todayWirds || {};
    const hasAnyTodayWirds = Object.values(today).some(Boolean);

    if (hasAnyTodayWirds) {
        for (const [typeKey, cfg] of Object.entries(window.WIRD_TYPE_CONFIG)) {
            const todayWird = today[typeKey];
            const wirdState = student.wirds[typeKey];
            if (!wirdState) continue;

            if (todayWird) {
                wirdState.active = true;
                wirdState.amount = todayWird.amount ?? '';
                wirdState.amountUnit = todayWird.amountUnit ?? cfg.defaultUnit;
                wirdState.equivalentPages = todayWird.equivalentPages ?? null;
                wirdState.fromSurah = todayWird.fromSurah || 1;
                wirdState.fromAyah = todayWird.fromAyah || '';
                wirdState.toSurah = todayWird.toSurah || 1;
                wirdState.toAyah = todayWird.toAyah || '';
                wirdState.status = todayWird.status || 0;
                wirdState.rating = getGradeKeyByStatus(todayWird.status);
                wirdState.note = todayWird.note || '';
                wirdState.isUpcoming = Boolean(todayWird.isUpcoming);
            } else {
                wirdState.active = false;
            }
        }
    } else if (student.routinePlan) {
        applyPlanToStudentWirds(student, student.routinePlan);
    }
}

// Map associating wird category to plan DTO property prefixes and fallback defaults
const PLAN_CATEGORY_MAP = {
    memorization: { prefix: 'mem', defaultUnit: 0 },
    recentRevision: { prefix: 'recentRev', defaultUnit: 0 },
    oldRevision: { prefix: 'oldRev', defaultUnit: 2 },
    recitation: { prefix: 'recitation', defaultUnit: 0 }
};

function getPlanProp(plan, prefix, prop) {
    const camel = `${prefix}${prop}`;
    const pascal = `${prefix.charAt(0).toUpperCase()}${prefix.slice(1)}${prop}`;
    return plan[camel] !== undefined ? plan[camel] : plan[pascal];
}

/**
 * Applies a plan object to a student's wirds state
 */
function applyPlanToStudentWirds(student, plan) {
    if (!student?.wirds || !plan) return;

    for (const [typeKey, { prefix, defaultUnit }] of Object.entries(PLAN_CATEGORY_MAP)) {
        const wird = student.wirds[typeKey];
        if (!wird) continue;

        const active = getPlanProp(plan, prefix, 'Active');
        wird.active = active !== undefined ? active !== false : true;
        wird.amount = getPlanProp(plan, prefix, 'Amount') ?? '';
        wird.amountUnit = getPlanProp(plan, prefix, 'Unit') ?? defaultUnit;

        const equivPages = getPlanProp(plan, prefix, 'EquivalentPages');
        if (equivPages !== undefined && equivPages !== null) {
            wird.equivalentPages = equivPages;
        }
    }

    const defaultNote = plan.defaultNote ?? plan.DefaultNote;
    if (defaultNote && !student.wirds.memorization?.note) {
        student.wirds.memorization.note = defaultNote;
    }
}

// React to student plan updates dispatched by StudentRoutinePlanModal
window.addEventListener('hafiz:studentPlanSaved', (e) => {
    const studentId = e.detail?.studentId;
    if (!studentId) return;

    const student = window.classStudents.find(s => s.id === studentId);
    if (!student) return;

    student.routinePlan = e.detail.plan;
    if (window.studentContextCache) {
        delete window.studentContextCache[student.id];
    }

    if (e.detail.plan) {
        applyPlanToStudentWirds(student, e.detail.plan);
    }

    if (window.classStudents[window.selectedStudentIndex]?.id === student.id && typeof renderStudentWirdsFields === 'function') {
        renderStudentWirdsFields(student);
    }
});
