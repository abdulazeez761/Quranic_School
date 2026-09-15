async function saveDrawerWirds(shouldClose = false) {
  const student = window.classStudents[window.selectedStudentIndex];
  if (!student) return false;

  // Collect DOM input values into student state before saving
  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const wird = student.wirds[typeKey];
    if (!wird) return;

    const amountInput = document.getElementById(`amount-${typeKey}`);
    if (amountInput) {
      const rawVal = amountInput.value.trim();
      wird.amount = rawVal !== '' && !isNaN(rawVal) && Number(rawVal) > 0 ? parseFloat(rawVal) : null;
    }

    const fromSurah = document.getElementById(`fromSurah-${typeKey}`);
    if (fromSurah) {
      const val = parseInt(fromSurah.value, 10);
      wird.fromSurah = !isNaN(val) && val > 0 ? val : null;
    }

    const fromAyah = document.getElementById(`fromAyah-${typeKey}`);
    if (fromAyah) {
      const val = fromAyah.value.trim();
      wird.fromAyah = val.length > 0 ? val : null;
    }

    const toSurah = document.getElementById(`toSurah-${typeKey}`);
    if (toSurah) {
      const val = parseInt(toSurah.value, 10);
      wird.toSurah = !isNaN(val) && val > 0 ? val : null;
    }

    const toAyah = document.getElementById(`toAyah-${typeKey}`);
    if (toAyah) {
      const val = parseInt(toAyah.value.trim(), 10);
      wird.toAyah = !isNaN(val) && val > 0 ? val : null;
    }

    const noteInput = document.getElementById(`note-${typeKey}`);
    if (noteInput) {
      const val = noteInput.value.trim();
      wird.note = val.length > 0 ? val : null;
    }

    const upcomingToggle = document.getElementById(`isUpcoming-${typeKey}`);
    if (upcomingToggle) wird.isUpcoming = upcomingToggle.checked;
  });

  const classIdVal = document.getElementById('ClassId')?.value?.trim();
  // Ensure valid GUID format (36 chars) or null
  const classId = classIdVal && classIdVal.length === 36 ? classIdVal : null;

  const wirdsList = [];

  Object.keys(window.WIRD_TYPE_CONFIG).forEach((typeKey) => {
    const w = student.wirds[typeKey];
    if (!w || !w.active) return;

    const hasAmount = w.amount !== null && w.amount !== undefined && !isNaN(w.amount) && Number(w.amount) > 0;
    const hasAyahs = (w.fromAyah !== null && w.fromAyah !== undefined && w.fromAyah !== '') ||
                     (w.toAyah !== null && w.toAyah !== undefined && !isNaN(w.toAyah));
    const hasStatus = w.status !== null && w.status !== undefined && parseInt(w.status, 10) > 0;
    const hasNote = w.note !== null && w.note !== undefined && w.note.length > 0;
    const isUpcoming = !!w.isUpcoming;

    // Only include cards that have actual content or are explicitly scheduled as upcoming
    if (!hasAmount && !hasAyahs && !hasStatus && !hasNote && !isUpcoming) {
      return;
    }

    const amountUnit = (w.amountUnit !== null && w.amountUnit !== undefined && !isNaN(w.amountUnit))
      ? parseInt(w.amountUnit, 10)
      : 0;

    const equivPages = (amountUnit === 1 && w.equivalentPages !== null && w.equivalentPages !== undefined && !isNaN(w.equivalentPages))
      ? parseFloat(w.equivalentPages)
      : null;

    wirdsList.push({
      Type: window.WIRD_TYPE_CONFIG[typeKey].typeCode,
      Amount: hasAmount ? Number(w.amount) : null,
      AmountUnit: amountUnit,
      EquivalentPages: equivPages,
      FromSurah: w.fromSurah || null,
      FromAyah: w.fromAyah || null,
      ToSurah: w.toSurah || null,
      ToAyah: w.toAyah || null,
      Status: isUpcoming ? 0 : (parseInt(w.status, 10) || 0),
      Note: w.note || null,
      IsUpcoming: isUpcoming,
    });
  });

  if (wirdsList.length === 0) {
    showDrawerToast('يرجى تحديد مقدار الورد أو نطاق الآيات قبل الحفظ.', 'warning');
    return false;
  }

  // Construct payload matching AssignWirdsBatchDto exactly
  const payload = {
    StudentId: student.id,
    ClassId: classId,
    AssignedDate: new Date().toISOString(),
    Wirds: wirdsList,
  };

  const batchInput = document.getElementById('BatchPayloadJson');
  if (batchInput) batchInput.value = JSON.stringify(payload);

  const saveBtn = document.getElementById('drawerSaveBtn');
  let originalSaveBtnHtml = '';
  if (shouldClose && saveBtn) {
    if (saveBtn.disabled) return false;
    saveBtn.disabled = true;
    originalSaveBtnHtml = saveBtn.innerHTML;
    saveBtn.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري الحفظ...";
  }

  let success = false;
  try {
    success = await sendWirdsPayloadToBackend(payload);
    if (success) {
      // Invalidate student context cache so fresh data from DB is retrieved if needed
      if (window.studentContextCache && window.studentContextCache[student.id]) {
        delete window.studentContextCache[student.id];
      }
      showDrawerToast(`✔️ تم حفظ أوراد الطالب (${student.name}) بنجاح!`, 'success');
      if (shouldClose && typeof closeWirdDrawer === 'function') {
        closeWirdDrawer();
      }
    }
  } finally {
    if (shouldClose && saveBtn) {
      saveBtn.disabled = false;
      saveBtn.innerHTML = originalSaveBtnHtml || "<i class='bx bx-check'></i> حفظ فقط";
    }
  }

  return success;
}

async function sendWirdsPayloadToBackend(payload) {
  if (!payload || !payload.Wirds || payload.Wirds.length === 0) return false;

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
      let errorMsg = 'حدث خطأ أثناء حفظ الأوراد.';
      try {
        const errorData = await response.json();
        if (errorData && errorData.message) {
          errorMsg = errorData.message;
        }
      } catch (e) {
        const errorText = await response.text();
        if (errorText) errorMsg = errorText;
      }
      console.error('Backend returned an error:', response.status, errorMsg);
      showDrawerToast(`❌ ${errorMsg}`, 'error');
      return false;
    }
    return true;
  } catch (err) {
    console.warn('Asynchronous wird saving failed:', err);
    showDrawerToast('تعذر الاتصال بالسيرفر لحفظ الأوراد.', 'error');
    return false;
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
