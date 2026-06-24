// ===================================
// NOTES EDITING FUNCTIONALITY
// ===================================

// Edit note button click (icon buttons with data-assignment-id)
document.querySelectorAll('.card-action-btn[data-assignment-id]').forEach((btn) => {
  btn.addEventListener('click', function () {
    const assignmentId = this.dataset.assignmentId;
    const editSection = document.getElementById(`notes-edit-${assignmentId}`);

    if (
      editSection.style.display === 'none' ||
      editSection.style.display === ''
    ) {
      editSection.style.display = 'block';
      editSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    } else {
      editSection.style.display = 'none';
    }
  });
});

// Save note button click
document.querySelectorAll('.save-note-btn').forEach((btn) => {
  btn.addEventListener('click', async function () {
    const assignmentId = this.dataset.assignmentId;
    const noteText = document.getElementById(
      `notes-textarea-${assignmentId}`,
    ).value;

    try {
      let token = document.querySelector(
        'input[name="__RequestVerificationToken"]',
      ).value;
      const response = await fetch(`/Teacher/Wird/UpdateWirdNote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token,
        },
        body: JSON.stringify({ Id: assignmentId, Note: noteText }),
      }).then((res) => res.json());

      if (response.success) {
        const card = btn.closest('.wird-card');

        card.classList.add('updated');

        setTimeout(() => {
          card.classList.remove('updated');
        }, 1500);

        // Hide edit section, show note display
        document.getElementById(`notes-edit-${assignmentId}`).style.display =
          'none';

        // Update the note display in the wird-note section
        const wirdNoteSection = card.querySelector('.wird-note');
        if (noteText.trim()) {
          if (wirdNoteSection) {
            wirdNoteSection.querySelector('p').textContent = noteText;
          } else {
            const notesEdit = document.getElementById(`notes-edit-${assignmentId}`);
            const noteHtml = `
              <div class="wird-note">
                <i class='bx bx-note'></i>
                <p>${noteText}</p>
              </div>
            `;
            notesEdit.insertAdjacentHTML('beforebegin', noteHtml);
          }
        } else {
          // Remove note section if note is empty
          if (wirdNoteSection) {
            wirdNoteSection.remove();
          }
        }
      } else {
        const card = btn.closest('.wird-card');

        card.classList.add('failed-to-update');

        setTimeout(() => {
          card.classList.remove('failed-to-update');
        }, 1500);
      }
    } catch (error) {
      console.error('Error saving note:', error);
      alert('An error occurred. Please try again.');
    }
  });
});

// Cancel note button click
document.querySelectorAll('.cancel-note-btn').forEach((btn) => {
  btn.addEventListener('click', function () {
    const assignmentId = this.dataset.assignmentId;
    document.getElementById(`notes-edit-${assignmentId}`).style.display =
      'none';
  });
});

setTimeout(function () {
  const successAlert = document.getElementById('successAlert');
  const errorAlert = document.getElementById('errorAlert');

  if (successAlert) {
    successAlert.style.opacity = '0';

    setTimeout(() => (successAlert.style.display = 'none'), 500);
  }
  if (errorAlert) {
    errorAlert.style.opacity = '0';

    setTimeout(() => (errorAlert.style.display = 'none'), 500);
  }
}, 5000);
//fetching assignment
function fetchWirdAssignmentById(id) {
  fetch(`/Teacher/Wird/GetWirdAssignmentById?id=${id}`)
    .then((response) => response.json())
    .then((data) => {
      // 1. تعبئة الـ ID الخاص بالواجب (لغايات التعديل)
      document.getElementById('AssignmentId').value = data.id;

      // 2. تعبئة نوع الواجب
      document.getElementById('Type').value = data.type;

      // 2.1 تعبئة المقدار والوحدة
      const amountEl = document.getElementById('Amount');
      const amountUnitEl = document.getElementById('AmountUnit');
      const equivalentEl = document.getElementById('EquivalentPages');
      if (amountEl) amountEl.value = data.amount ?? '';
      if (equivalentEl) equivalentEl.value = data.equivalentPages ?? '';
      if (amountUnitEl) {
        amountUnitEl.value = data.amountUnit ?? '';
        // Re-apply the min/step rules for the loaded unit (whole vs. fractional).
        amountUnitEl.dispatchEvent(new Event('change'));
      }

      // 3. تعبئة قسم "من" (From)
      document.getElementById('FromJuz').value = data.fromJuz;
      document.getElementById('FromPage').value = data.fromPage;
      document.getElementById('FromSurah').value = data.fromSurah;
      document.getElementById('FromAyah').value = data.fromAyah;

      // 4. تعبئة قسم "إلى" (To)
      document.getElementById('ToJuz').value = data.toJuz;
      document.getElementById('ToPage').value = data.toPage;
      document.getElementById('ToSurah').value = data.toSurah;
      document.getElementById('ToAyah').value = data.toAyah;

      // 5. تعبئة التقييم (الـ ID في كودك هو Grade)
      document.getElementById('Grade').value = data.status;

      // 6. تعبئة الملاحظات (نضع نصاً فارغاً إذا كانت الملاحظة غير موجودة لتجنب طباعة 'null')
      document.getElementById('Note').value = data.note || '';
    })
    .catch((error) => {
      console.error('Error fetching assignment:', error);
      // يفضل هنا إضافة Alert تخبر المستخدم بوجود خطأ في جلب البيانات
    });
}
