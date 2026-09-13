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
    unitGroup.querySelectorAll('.unit-radio-option').forEach(opt => {
        const radio = opt.querySelector('input[type="radio"]');
        const isMatched = radio && parseInt(radio.value, 10) === unit;
        opt.classList.toggle('is-selected', isMatched);
        if (radio) radio.checked = isMatched;
    });
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
        const buttons = group.querySelectorAll('.rating-pill-btn');
        buttons.forEach(btn => {
            btn.disabled = isUpcoming;
            btn.style.opacity = isUpcoming ? '0.4' : '1';
            btn.style.cursor = isUpcoming ? 'not-allowed' : 'pointer';
            if (isUpcoming) btn.classList.remove('is-selected');
        });
    }

    if (badge) {
        if (isUpcoming) {
            badge.className = 'wird-rating-badge rate-upcoming';
            badge.innerHTML = "<i class='bx bx-calendar-star'></i> ورد قادم (مجدول ولم يُسمّع بعد)";
            badge.style.background = '#ede9fe';
            badge.style.color = '#6b21a8';
            badge.style.border = '1px solid #c4b5fd';
            badge.style.fontWeight = '700';
        } else {
            badge.style.background = '';
            badge.style.color = '';
            badge.style.border = '';
            badge.style.fontWeight = '';
        }
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

    group.querySelectorAll('.rating-pill-btn').forEach(btn => {
        const grade = btn.dataset.grade;
        btn.classList.toggle('is-selected', !!(meta && meta.grade === grade));
    });
}
