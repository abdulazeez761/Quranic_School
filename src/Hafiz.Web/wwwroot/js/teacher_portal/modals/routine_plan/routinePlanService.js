/**
 * ============================================================================
 * Student Routine Plan - Backend Service & Event Synchronization
 * ============================================================================
 * Manages API calls for retrieving, saving, and deleting student baseline
 * routine plans, managing CSRF tokens, caching, and toast notifications.
 * ============================================================================
 */

window.studentPlansCache = window.studentPlansCache || {};

function getCsrfToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : '';
}

function showRoutineToast(msg, type = 'info') {
    if (typeof Swal !== 'undefined') {
        const Toast = Swal.mixin({
            toast: true,
            position: 'top-end',
            showConfirmButton: false,
            timer: 3500,
            timerProgressBar: true
        });
        Toast.fire({
            icon: type === 'error' ? 'error' : (type === 'success' ? 'success' : 'info'),
            title: msg
        });
    } else {
        alert(msg);
    }
}

/**
 * Save the Student Routine Plan via AJAX POST
 */
async function saveStudentPlan() {
    if (!window.routineCurrentStudent || !window.routineCurrentStudent.id) {
        showRoutineToast('تعذر تحديد الطالب للحفظ', 'error');
        return;
    }

    const saveBtn = document.getElementById('btnSaveStudentPlan');
    const origBtnHtml = saveBtn ? saveBtn.innerHTML : '';
    if (saveBtn) {
        saveBtn.disabled = true;
        saveBtn.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري الحفظ...";
    }

    const memActive = document.getElementById('rtoggle-memorization')?.checked ?? true;
    const memAmount = parseFloat(document.getElementById('planMemAmount')?.value) || 1.0;
    const memUnit = typeof getSelectedUnit === 'function' ? getSelectedUnit('memorization') : 0;
    const memEquivalentPages = (memUnit === 1) ? (window.routineCurrentStudent.equivPages || 0.5) : null;

    const recentRevActive = document.getElementById('rtoggle-recentRevision')?.checked ?? true;
    const recentRevAmount = parseFloat(document.getElementById('planRecentRevAmount')?.value) || 5.0;
    const recentRevUnit = typeof getSelectedUnit === 'function' ? getSelectedUnit('recentRevision') : 0;

    const oldRevActive = document.getElementById('rtoggle-oldRevision')?.checked ?? true;
    const oldRevAmount = parseFloat(document.getElementById('planOldRevAmount')?.value) || 1.0;
    const oldRevUnit = typeof getSelectedUnit === 'function' ? getSelectedUnit('oldRevision') : 2;

    const recitationActive = document.getElementById('rtoggle-recitation')?.checked ?? true;
    const recitationAmount = parseFloat(document.getElementById('planRecitationAmount')?.value) || 2.0;
    const recitationUnit = typeof getSelectedUnit === 'function' ? getSelectedUnit('recitation') : 0;

    const defaultNote = document.getElementById('planDefaultNote')?.value?.trim() || null;

    const classIdMatch = document.cookie.match(/selectedClassId=([^;]+)/);
    const classId = classIdMatch ? classIdMatch[1] : null;

    const payload = {
        StudentId: window.routineCurrentStudent.id,
        ClassId: classId,
        MemActive: memActive,
        MemAmount: memAmount,
        MemUnit: memUnit,
        MemEquivalentPages: memEquivalentPages,
        RecentRevActive: recentRevActive,
        RecentRevAmount: recentRevAmount,
        RecentRevUnit: recentRevUnit,
        OldRevActive: oldRevActive,
        OldRevAmount: oldRevAmount,
        OldRevUnit: oldRevUnit,
        RecitationActive: recitationActive,
        RecitationAmount: recitationAmount,
        RecitationUnit: recitationUnit,
        DefaultNote: defaultNote
    };

    try {
        const response = await fetch('/Teacher/StudentRoutinePlan/SavePlan', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getCsrfToken()
            },
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (result && result.success) {
            const savedPlanObj = {
                studentId: window.routineCurrentStudent.id,
                classId: classId,
                memActive, memAmount, memUnit, memEquivalentPages,
                recentRevActive, recentRevAmount, recentRevUnit,
                oldRevActive, oldRevAmount, oldRevUnit,
                recitationActive, recitationAmount, recitationUnit,
                defaultNote
            };

            window.studentPlansCache[window.routineCurrentStudent.id] = savedPlanObj;

            // Dispatch global event for other decoupled components to react
            window.dispatchEvent(new CustomEvent('hafiz:studentPlanSaved', {
                detail: {
                    studentId: window.routineCurrentStudent.id,
                    plan: savedPlanObj
                }
            }));

            if (typeof closeStudentPlanModal === 'function') {
                closeStudentPlanModal();
            }
            showRoutineToast('✅ تم حفظ واعتماد خطة الطالب بنجاح!', 'success');
        } else {
            showRoutineToast(result?.message || 'تعذر حفظ الخطة، يرجى المحاولة لاحقاً', 'error');
        }
    } catch (err) {
        console.error('Save error:', err);
        showRoutineToast('حدث خطأ في الاتصال بالخادم أثناء الحفظ', 'error');
    } finally {
        if (saveBtn) {
            saveBtn.disabled = false;
            saveBtn.innerHTML = origBtnHtml;
        }
    }
}

/**
 * Reset student plan to general default
 */
async function resetStudentPlanToDefault() {
    if (!window.routineCurrentStudent || !window.routineCurrentStudent.id) return;

    const doReset = async () => {
        try {
            const res = await fetch(`/Teacher/StudentRoutinePlan/DeletePlan?studentId=${encodeURIComponent(window.routineCurrentStudent.id)}`, {
                method: 'POST',
                headers: {
                    'RequestVerificationToken': getCsrfToken()
                }
            });
            const result = await res.json();
            if (result && result.success) {
                delete window.studentPlansCache[window.routineCurrentStudent.id];
                if (typeof clearRoutineModalInputs === 'function') {
                    clearRoutineModalInputs();
                }
                const badgeEl = document.getElementById('routinePlanStatusBadge');
                if (badgeEl) {
                    badgeEl.className = 'plan-status-pill default';
                    badgeEl.innerHTML = "<i class='bx bx-info-circle'></i> لم تُحدد خطة بعد";
                }

                window.dispatchEvent(new CustomEvent('hafiz:studentPlanSaved', {
                    detail: {
                        studentId: window.routineCurrentStudent.id,
                        plan: null
                    }
                }));

                showRoutineToast('🔄 تمت إزالة الخطة المخصصة للطالب بنجاح', 'info');
            } else {
                showRoutineToast(result?.message || 'تعذر إعادة تعيين الخطة', 'error');
            }
        } catch (err) {
            console.error('Reset error:', err);
            showRoutineToast('حدث خطأ أثناء إعادة تعيين الخطة', 'error');
        }
    };

    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'استعادة الوضع الافتراضي؟',
            text: 'سيتم إلغاء الخطة المخصصة لهذا الطالب والرجوع إلى الوتيرة العامة للحلقة.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#059669',
            cancelButtonColor: '#94a3b8',
            confirmButtonText: 'نعم، استعد الافتراضي',
            cancelButtonText: 'إلغاء'
        }).then((res) => {
            if (res.isConfirmed) doReset();
        });
    } else {
        if (confirm('هل أنت متأكد من استعادة الخطة الافتراضية العامة لهذا الطالب؟')) {
            doReset();
        }
    }
}
