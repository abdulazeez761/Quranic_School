/**
 * ============================================================================
 * Student Routine Plan - Modal Controller & Lifecycle Management
 * ============================================================================
 * Manages modal visibility, loading student profiles, handling backdrop and
 * keyboard events, and coordinating form population.
 * ============================================================================
 */

window.routineCurrentStudent = {
    id: null,
    name: '',
    level: '',
    hasCustomPlan: false,
    equivPages: 0.5
};

/**
 * Open the Student Routine Plan Modal
 * @param {string} studentId 
 * @param {string} studentName 
 * @param {string} [levelName]
 */
async function openStudentPlanModal(studentId, studentName, levelName) {
    if (!studentId) return;

    window.routineCurrentStudent.id = studentId;
    window.routineCurrentStudent.name = studentName || 'الطالب';
    window.routineCurrentStudent.level = levelName || '';

    // Update Header Profile in Modal
    const nameEl = document.getElementById('routineStudentName');
    const levelEl = document.getElementById('routineStudentLevel');
    const avatarEl = document.getElementById('routineStudentAvatar');
    const badgeEl = document.getElementById('routinePlanStatusBadge');

    if (nameEl) nameEl.textContent = window.routineCurrentStudent.name;
    if (levelEl) levelEl.textContent = window.routineCurrentStudent.level ? `المستوى: ${window.routineCurrentStudent.level}` : '';
    if (avatarEl) {
        const initials = window.routineCurrentStudent.name.split(' ').map(w => w[0]).filter(Boolean).slice(0, 2).join('');
        avatarEl.textContent = initials || 'ط';
    }

    // Show Dialog & Backdrop with animation
    const backdrop = document.getElementById('routineModalBackdrop');
    const dialog = document.getElementById('routineModalDialog');
    if (backdrop) backdrop.classList.add('is-open');
    if (dialog) dialog.classList.add('is-open');
    document.body.style.overflow = 'hidden';

    // Show loading indicator on status badge
    if (badgeEl) {
        badgeEl.className = 'plan-status-pill default';
        badgeEl.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري التحميل...";
    }

    // Fetch Plan Data
    try {
        let planData = window.studentPlansCache[studentId];

        if (!planData) {
            const res = await fetch(`/Teacher/StudentRoutinePlan/GetPlanByStudentId?studentId=${encodeURIComponent(studentId)}`);
            const result = await res.json();
            if (result && result.success) {
                planData = result.data;
                if (planData) {
                    window.studentPlansCache[studentId] = planData;
                }
            }
        }

        if (planData) {
            window.routineCurrentStudent.hasCustomPlan = true;
            if (badgeEl) {
                badgeEl.className = 'plan-status-pill custom';
                badgeEl.innerHTML = "<i class='bx bx-check-circle'></i> خطة معتمدة للطالب";
            }
            if (typeof populateRoutineModal === 'function') {
                populateRoutineModal(planData);
            }
        } else {
            window.routineCurrentStudent.hasCustomPlan = false;
            if (badgeEl) {
                badgeEl.className = 'plan-status-pill default';
                badgeEl.innerHTML = "<i class='bx bx-info-circle'></i> لم تُحدد خطة بعد";
            }
            if (typeof clearRoutineModalInputs === 'function') {
                clearRoutineModalInputs();
            }
        }
    } catch (err) {
        console.error('Error fetching routine plan:', err);
        if (badgeEl) {
            badgeEl.className = 'plan-status-pill default';
            badgeEl.innerHTML = "<i class='bx bx-info-circle'></i> لم تُحدد خطة بعد";
        }
        if (typeof clearRoutineModalInputs === 'function') {
            clearRoutineModalInputs();
        }
    }
}

/**
 * Close the Student Routine Plan Modal
 */
function closeStudentPlanModal() {
    const backdrop = document.getElementById('routineModalBackdrop');
    const dialog = document.getElementById('routineModalDialog');
    if (backdrop) backdrop.classList.remove('is-open');
    if (dialog) dialog.classList.remove('is-open');
    document.body.style.overflow = '';
}

// Global keyboard navigation for the modal
document.addEventListener('keydown', function (e) {
    const dialog = document.getElementById('routineModalDialog');
    if (!dialog || !dialog.classList.contains('is-open')) return;

    if (e.key === 'Escape') {
        closeStudentPlanModal();
    } else if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
        e.preventDefault();
        if (typeof saveStudentPlan === 'function') {
            saveStudentPlan();
        }
    }
});
