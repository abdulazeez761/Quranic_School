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
        student.wirds[typeKey].amount = parseFloat(val) || 0;
        if (!student.wirds[typeKey].active && parseFloat(val) > 0) {
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
