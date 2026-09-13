/**
 * ============================================================================
 * Wird Drawer - Persistence & Backend Communication Service
 * ============================================================================
 * Gathers active wird values, persists to localStorage drafts cache,
 * posts JSON/FormData payloads to backend API, and handles toast alerts.
 * ============================================================================
 */

function saveDrawerWirds(shouldClose = false) {
    const student = window.classStudents[window.selectedStudentIndex];
    if (!student) return;

    // Collect DOM input values into student state before saving
    Object.keys(window.WIRD_TYPE_CONFIG).forEach(typeKey => {
        const wird = student.wirds[typeKey];
        if (!wird) return;

        const amountInput = document.getElementById(`amount-${typeKey}`);
        if (amountInput) wird.amount = parseFloat(amountInput.value) || wird.amount;

        const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
        if (fromSurah) wird.fromSurah = parseInt(fromSurah.value, 10) || wird.fromSurah;

        const fromAyah = document.getElementById(`fromAyah-${typeKey}`);
        if (fromAyah) wird.fromAyah = fromAyah.value;

        const toSurah = document.getElementById(`toSurah-${typeKey}`);
        if (toSurah) wird.toSurah = parseInt(toSurah.value, 10) || wird.toSurah;

        const toAyah = document.getElementById(`toAyah-${typeKey}`);
        if (toAyah) wird.toAyah = parseInt(toAyah.value, 10) || wird.toAyah;

        const noteInput = document.getElementById(`note-${typeKey}`);
        if (noteInput) wird.note = noteInput.value;

        const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
        if (upcomingToggle) wird.isUpcoming = upcomingToggle.checked;
    });

    const classId = document.getElementById('ClassId')?.value || null;
    const activeWirdsList = [];

    Object.keys(window.WIRD_TYPE_CONFIG).forEach(typeKey => {
        const w = student.wirds[typeKey];
        if (w && w.active) {
            activeWirdsList.push({
                StudentId: student.id,
                ClassId: classId,
                Type: window.WIRD_TYPE_CONFIG[typeKey].typeCode,
                Amount: w.amount,
                AmountUnit: w.amountUnit,
                EquivalentPages: w.equivalentPages,
                FromSurah: w.fromSurah,
                FromAyah: w.fromAyah,
                ToSurah: w.toSurah,
                ToAyah: w.toAyah,
                Status: w.status,
                Rating: w.rating,
                Note: w.note,
                IsUpcoming: w.isUpcoming,
                AssignedDate: new Date().toISOString()
            });
        }
    });

    const batchInput = document.getElementById('BatchPayloadJson');
    if (batchInput) batchInput.value = JSON.stringify(activeWirdsList);

    if (typeof saveDraftsToCache === 'function') {
        saveDraftsToCache();
    }
    sendWirdsPayloadToBackend(student.id, activeWirdsList);

    showDrawerToast(`✔️ تم حفظ أوراد الطالب (${student.name}) بنجاح!`, 'success');

    if (shouldClose && typeof closeWirdDrawer === 'function') {
        closeWirdDrawer();
    }
}

async function sendWirdsPayloadToBackend(studentId, payloadList) {
    if (!payloadList || payloadList.length === 0) return;

    try {
        const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenEl ? tokenEl.value : '';

        const primaryWird = payloadList[0];
        const formData = new FormData();
        formData.append('StudentId', studentId);
        formData.append('ClassId', primaryWird.ClassId || '');
        formData.append('Type', primaryWird.Type);
        formData.append('Amount', primaryWird.Amount);
        formData.append('AmountUnit', primaryWird.AmountUnit);
        if (primaryWird.EquivalentPages) formData.append('EquivalentPages', primaryWird.EquivalentPages);
        formData.append('FromSurah', primaryWird.FromSurah || '');
        formData.append('FromAyah', primaryWird.FromAyah || '');
        formData.append('ToSurah', primaryWird.ToSurah || '');
        formData.append('ToAyah', primaryWird.ToAyah || '');
        formData.append('Status', primaryWird.Status || 0);
        formData.append('Note', primaryWird.Note || '');
        formData.append('IsUpcoming', primaryWird.IsUpcoming);
        formData.append('BatchPayloadJson', JSON.stringify(payloadList));

        if (token) formData.append('__RequestVerificationToken', token);

        await fetch('/Teacher/Student/AssignWird', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });
    } catch (err) {
        console.warn('Asynchronous wird saving failed:', err);
    }
}

function showDrawerToast(message, type = 'success') {
    let container = document.getElementById('drawerToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'drawerToastContainer';
        container.className = 'toast-container';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    const icon = type === 'success' ? 'bx-check-circle' : (type === 'info' ? 'bxs-star' : 'bx-info-circle');
    toast.innerHTML = `<i class='bx ${icon}' style="font-size: 1.25rem;"></i><span>${message}</span>`;

    container.appendChild(toast);
    setTimeout(() => toast.classList.add('show'), 10);
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3500);
}
