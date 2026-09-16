/**
 * ============================================================================
 * Wird Drawer - Form Controls & Interactive Helpers
 * ============================================================================
 * Manages card activation toggles, quantity and unit radios, equivalent chips,
 * and rating button states for each wird type in the drawer.
 * ============================================================================
 */

function toggleWirdCard(typeKey, isActive) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].active = isActive;
        const box = document.getElementById(`wbox-${typeKey}`);
        const statusLabel = document.getElementById(`wstatus-${typeKey}`);
        if (box) box.classList.toggle('is-disabled', !isActive);
        if (statusLabel) {
            statusLabel.textContent = isActive ? 'مفعل' : 'معطل';
            statusLabel.style.color = isActive ? '#16a34a' : 'var(--text-medium)';
        }
    }
}

function onAmountChanged(typeKey, val) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        const rawNum = parseFloat(val);
        student.wirds[typeKey].amount = !isNaN(rawNum) && rawNum > 0 ? rawNum : null;
        if (!student.wirds[typeKey].active && !isNaN(rawNum) && rawNum > 0) {
            toggleWirdCard(typeKey, true);
            const toggle = document.getElementById(`wtoggle-${typeKey}`);
            if (toggle) toggle.checked = true;
        }
    }
}

function onUnitChanged(typeKey, unitVal) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        const unit = parseInt(unitVal, 10);
        student.wirds[typeKey].amountUnit = unit;
        updateUnitRadioUI(typeKey, unit);

        const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
        if (equivGroup) {
            equivGroup.style.display = unit === 1 ? 'block' : 'none';
        }
    }
}

function updateUnitRadioUI(typeKey, unitVal) {
    const unitGroup = document.getElementById(`unitGroup-${typeKey}`);
    if (!unitGroup) return;

    const unit = parseInt(unitVal, 10);
    const targetRadio = unitGroup.querySelector(`input[type="radio"][value="${unit}"]`);
    const targetOpt = targetRadio ? targetRadio.closest('.unit-radio-option') : null;
    const prevOpt = unitGroup.querySelector('.unit-radio-option.is-selected');

    if (prevOpt !== targetOpt) {
        if (prevOpt) prevOpt.classList.remove('is-selected');
        if (targetOpt) targetOpt.classList.add('is-selected');
    }
    if (targetRadio && !targetRadio.checked) {
        targetRadio.checked = true;
    }
}

function setEquivalentPageChip(typeKey, val) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].equivalentPages = val;
        const equivGroup = document.getElementById(`equivGroup-${typeKey}`);
        if (equivGroup) {
            equivGroup.querySelectorAll('.chip').forEach(c => {
                c.classList.toggle('is-active', parseFloat(c.dataset.val) === val);
            });
        }
    }
}

function setRatingGrade(typeKey, gradeKey, statusNumber) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (!student || !student.wirds[typeKey]) return;

    // If card was in upcoming mode, selecting a grade means it is now evaluated and no longer upcoming
    if (student.wirds[typeKey].isUpcoming) {
        student.wirds[typeKey].isUpcoming = false;
        const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
        if (upcomingToggle) upcomingToggle.checked = false;
        syncUpcomingRatingState(typeKey, false);
    }

    const currentGrade = student.wirds[typeKey].rating;
    const newGrade = (currentGrade === gradeKey) ? null : gradeKey;
    const newStatus = newGrade ? statusNumber : 0;

    student.wirds[typeKey].rating = newGrade;
    student.wirds[typeKey].status = newStatus;

    if (newGrade && !student.wirds[typeKey].active) {
        toggleWirdCard(typeKey, true);
        const toggle = document.getElementById(`wtoggle-${typeKey}`);
        if (toggle) toggle.checked = true;
    }

    updateRatingUI(typeKey, newGrade);

    if (newGrade && typeof showDrawerToast === 'function') {
        const meta = window.RATING_GRADES[newGrade];
        showDrawerToast(`⭐ تم تقييم ${window.WIRD_TYPE_CONFIG[typeKey].title} بـ (${meta.label})`, 'info');
    }
}

function clearWirdRating(typeKey) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (student && student.wirds[typeKey]) {
        student.wirds[typeKey].rating = null;
        student.wirds[typeKey].status = 0;
        updateRatingUI(typeKey, null);
    }
}

function toggleUpcomingWird(typeKey, isUpcoming) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (!student || !student.wirds[typeKey]) return;

    student.wirds[typeKey].isUpcoming = isUpcoming;

    if (isUpcoming) {
        // Upcoming wird cannot have a completion grade - reset rating
        student.wirds[typeKey].rating = null;
        student.wirds[typeKey].status = 0;
        updateRatingUI(typeKey, null);

        // Ensure card is active so it gets saved
        if (!student.wirds[typeKey].active) {
            toggleWirdCard(typeKey, true);
            const toggle = document.getElementById(`wtoggle-${typeKey}`);
            if (toggle) toggle.checked = true;
        }

        if (typeof showDrawerToast === 'function') {
            showDrawerToast(`📅 تم تفعيل (${window.WIRD_TYPE_CONFIG[typeKey].title}) كوِرد قادم للجلسة القادمة`, 'info');
        }
    }

    syncUpcomingRatingState(typeKey, isUpcoming);
}

function syncUpcomingRatingState(typeKey, isUpcoming) {
    const group = document.getElementById(`ratingGroup-${typeKey}`);
    const badge = document.getElementById(`ratingBadge-${typeKey}`);
    const cardBox = document.getElementById(`wbox-${typeKey}`);

    if (cardBox) {
        cardBox.classList.toggle('is-upcoming-box', isUpcoming);
    }

    if (group) {
        group.classList.toggle('is-upcoming-mode', isUpcoming);
        if (isUpcoming) {
            const selectedBtn = group.querySelector('.rating-pill-btn.is-selected');
            if (selectedBtn) selectedBtn.classList.remove('is-selected');
        }
    }

    if (badge && isUpcoming) {
        badge.className = 'wird-rating-badge rate-upcoming';
        badge.innerHTML = "<i class='bx bx-calendar-star'></i> ورد قادم (مجدول ولم يُسمّع بعد)";
    }
}

function updateRatingUI(typeKey, ratingKey) {
    const group = document.getElementById(`ratingGroup-${typeKey}`);
    const badge = document.getElementById(`ratingBadge-${typeKey}`);
    const clearBtn = document.getElementById(`ratingClear-${typeKey}`);
    if (!group) return;

    const meta = ratingKey ? window.RATING_GRADES[ratingKey] : null;

    if (badge) {
        badge.className = `wird-rating-badge ${meta ? 'rate-' + meta.classSuffix : 'rate-none'}`;
        badge.innerHTML = meta ? `<i class='bx ${meta.icon}'></i> ${meta.label}` : 'لم يُسمّع / لم يُقيّم بعد';
    }

    if (clearBtn) {
        clearBtn.style.display = meta ? 'inline-flex' : 'none';
    }

    const prevSelected = group.querySelector('.rating-pill-btn.is-selected');
    const targetBtn = meta ? group.querySelector(`.rating-pill-btn[data-grade="${meta.grade}"]`) : null;

    if (prevSelected !== targetBtn) {
        if (prevSelected) prevSelected.classList.remove('is-selected');
        if (targetBtn) targetBtn.classList.add('is-selected');
    }
}
