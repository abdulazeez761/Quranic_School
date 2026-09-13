/**
 * ============================================================================
 * Student Routine Plan - Form Fields & Component Interactivity
 * ============================================================================
 * Handles populating and clearing form inputs, unit radio toggles,
 * equivalent pages chips, and active/inactive card states.
 * ============================================================================
 */

function populateRoutineModal(data) {
    if (!data) return;

    // Memorization
    setRoutineWirdToggle('memorization', data.memActive !== false);
    setRoutineInputValue('planMemAmount', data.memAmount !== null && data.memAmount !== undefined ? data.memAmount : '');
    setRoutineUnitOption('memorization', data.memUnit ?? 0);
    if (data.memEquivalentPages) {
        setRoutineEquivChip(data.memEquivalentPages);
    }

    // Recent Revision
    setRoutineWirdToggle('recentRevision', data.recentRevActive !== false);
    setRoutineInputValue('planRecentRevAmount', data.recentRevAmount !== null && data.recentRevAmount !== undefined ? data.recentRevAmount : '');
    setRoutineUnitOption('recentRevision', data.recentRevUnit ?? 0);

    // Old Revision
    setRoutineWirdToggle('oldRevision', data.oldRevActive !== false);
    setRoutineInputValue('planOldRevAmount', data.oldRevAmount !== null && data.oldRevAmount !== undefined ? data.oldRevAmount : '');
    setRoutineUnitOption('oldRevision', data.oldRevUnit ?? 2); // default 2=Juz

    // Recitation
    setRoutineWirdToggle('recitation', data.recitationActive !== false);
    setRoutineInputValue('planRecitationAmount', data.recitationAmount !== null && data.recitationAmount !== undefined ? data.recitationAmount : '');
    setRoutineUnitOption('recitation', data.recitationUnit ?? 0);

    // Default Note
    const noteEl = document.getElementById('planDefaultNote');
    if (noteEl) noteEl.value = data.defaultNote || '';
}

function clearRoutineModalInputs() {
    setRoutineWirdToggle('memorization', true);
    setRoutineInputValue('planMemAmount', '');
    setRoutineUnitOption('memorization', 0);

    setRoutineWirdToggle('recentRevision', true);
    setRoutineInputValue('planRecentRevAmount', '');
    setRoutineUnitOption('recentRevision', 0);

    setRoutineWirdToggle('oldRevision', true);
    setRoutineInputValue('planOldRevAmount', '');
    setRoutineUnitOption('oldRevision', 2);

    setRoutineWirdToggle('recitation', false);
    setRoutineInputValue('planRecitationAmount', '');
    setRoutineUnitOption('recitation', 0);

    const noteEl = document.getElementById('planDefaultNote');
    if (noteEl) noteEl.value = '';
}

function toggleRoutineWirdCard(type, isActive) {
    const card = document.getElementById(`rcard-${type}`);
    const statusLabel = document.getElementById(`rstatus-${type}`);
    
    if (card) {
        card.classList.toggle('is-disabled', !isActive);
    }

    if (statusLabel) {
        statusLabel.textContent = isActive ? 'مفعل' : 'معطل';
        statusLabel.style.color = isActive ? 'var(--plan-primary)' : 'var(--plan-text-sub)';
    }
}

function setRoutineWirdToggle(type, isActive) {
    const toggle = document.getElementById(`rtoggle-${type}`);
    if (toggle) {
        toggle.checked = isActive;
        toggleRoutineWirdCard(type, isActive);
    }
}

function setRoutineInputValue(id, val) {
    const el = document.getElementById(id);
    if (el) el.value = val;
}

function setRoutineUnitOption(type, unit) {
    unit = parseInt(unit, 10);
    const group = document.getElementById(`runit-${type}`);
    if (!group) return;

    const options = group.querySelectorAll('.routine-unit-option');
    options.forEach(opt => {
        const input = opt.querySelector('input');
        if (input) {
            const isMatched = parseInt(input.value, 10) === unit;
            input.checked = isMatched;
            opt.classList.toggle('is-selected', isMatched);
        }
    });

    if (type === 'memorization') {
        const equivBox = document.getElementById('routineEquivBox');
        if (equivBox) {
            equivBox.style.display = (unit === 1) ? 'block' : 'none';
        }
    }
}

function setRoutineEquivChip(val) {
    if (!window.routineCurrentStudent) return;
    window.routineCurrentStudent.equivPages = parseFloat(val);
    const chips = document.querySelectorAll('.routine-equiv-chip');
    chips.forEach(chip => {
        const match = parseFloat(chip.getAttribute('data-val')) === window.routineCurrentStudent.equivPages;
        chip.classList.toggle('is-active', match);
    });
}

function getSelectedUnit(type) {
    const checked = document.querySelector(`input[name="runit-opt-${type}"]:checked`);
    return checked ? parseInt(checked.value, 10) : 0;
}
