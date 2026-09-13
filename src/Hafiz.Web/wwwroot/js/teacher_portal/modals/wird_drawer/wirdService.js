function saveDrawerWirds(shouldClose = false) {
  const student = window.classStudents[window.selectedStudentIndex];
  if (!student) return;

  // Collect DOM input values into student state before saving
  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const wird = student.wirds[typeKey];
    if (!wird) return;

    const amountInput = document.getElementById(`amount-${typeKey}`);
    if (amountInput) wird.amount = parseFloat(amountInput.value) || wird.amount;

    const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
    if (fromSurah)
      wird.fromSurah = parseInt(fromSurah.value, 10) || wird.fromSurah;

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

  const classIdVal = document.getElementById('ClassId')?.value?.trim();
  // Ensure valid GUID or null (empty string will fail C# Guid? parsing)
  const classId = classIdVal ? classIdVal : null;

  const wirdsList = [];

  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const w = student.wirds[typeKey];
    if (w && w.active) {
      wirdsList.push({
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
      });
    }
  });

  // Construct payload matching AssignWirdsBatchDto exactly
  const payload = {
    StudentId: student.id,
    ClassId: classId,
    AssignedDate: new Date().toISOString(),
    Wirds: wirdsList,
  };

  const batchInput = document.getElementById('BatchPayloadJson');
  if (batchInput) batchInput.value = JSON.stringify(payload);

  if (typeof saveDraftsToCache === 'function') {
    saveDraftsToCache();
  }

  sendWirdsPayloadToBackend(payload);

  showDrawerToast(`✔️ تم حفظ أوراد الطالب (${student.name}) بنجاح!`, 'success');

  if (shouldClose && typeof closeWirdDrawer === 'function') {
    closeWirdDrawer();
  }
}

async function sendWirdsPayloadToBackend(payload) {
  if (!payload || !payload.Wirds || payload.Wirds.length === 0) return;

  try {
    const tokenEl = document.querySelector(
      'input[name="__RequestVerificationToken"]',
    );
    const headers = {
      'Content-Type': 'application/json',
      'X-Requested-With': 'XMLHttpRequest',
    };

    // ASP.NET Antiforgery token via header (Standard for JSON POSTs)
    if (tokenEl && tokenEl.value) {
      headers['RequestVerificationToken'] = tokenEl.value;
    }

    const response = await fetch('/Teacher/Student/AssignWirdsBatch', {
      method: 'POST',
      headers: headers,
      body: JSON.stringify(payload),
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error('Backend returned an error:', response.status, errorText);
    }
  } catch (err) {
    console.warn('Asynchronous wird saving failed:', err);
  }
}
// Wherever showDrawerToast is declared (or inside wirdService.js):
window.showDrawerToast = function showDrawerToast(message, type = 'success') {
  let container = document.getElementById('drawerToastContainer');
  if (!container) {
    container = document.createElement('div');
    container.id = 'drawerToastContainer';
    container.className = 'toast-container';
    document.body.appendChild(container);
  }

  const toast = document.createElement('div');
  toast.className = `toast toast-${type}`;
  const icon =
    type === 'success'
      ? 'bx-check-circle'
      : type === 'info'
        ? 'bxs-star'
        : 'bx-info-circle';
  toast.innerHTML = `<i class='bx ${icon}' style="font-size: 1.25rem;"></i><span>${message}</span>`;

  container.appendChild(toast);
  setTimeout(() => toast.classList.add('show'), 10);
  setTimeout(() => {
    toast.classList.remove('show');
    setTimeout(() => toast.remove(), 300);
  }, 3500);
};
