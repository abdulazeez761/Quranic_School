function attachEventListeners() {
  // Attendance radio buttons
  const checkboxes = document.querySelectorAll('.attendance-checkbox');
  checkboxes.forEach((cb) => {
    cb.addEventListener('change', function () {
      if (!this.checked) return;

      const row = this.closest('tr');
      const teacherId = this.dataset.teacherId;
      const status = this.dataset.status;
      const hoursInput = row.querySelector('.hours-input');
      const hours = parseFloat(hoursInput?.value || 0);

      // Update row class
      row.className = 'has-attendance';
      const statusClass = this.classList.contains('status-present')
        ? 'status-present'
        : this.classList.contains('status-late')
        ? 'status-late'
        : this.classList.contains('status-absent')
        ? 'status-absent'
        : 'status-excuse';
      row.classList.add(statusClass);

      // Update hours display
      const hoursDisplay = row.querySelector('td:nth-child(2) .fs-xs');
      if (hours > 0) {
        if (hoursDisplay) {
          hoursDisplay.textContent = `${hours} ساعة`;
        } else {
          const nameDiv = row.querySelector('td:nth-child(2) > div > div');
          if (nameDiv) {
            nameDiv.innerHTML += `<div class="fs-xs text-medium">${hours} ساعة</div>`;
          }
        }
      }

      // Save attendance
      saveAttendance(teacherId, status, hours);
    });
  });

  // Hours input with debounce
  const hoursInputs = document.querySelectorAll('.hours-input');
  hoursInputs.forEach((input) => {
    let timeout;

    input.addEventListener('input', function (e) {
      clearTimeout(timeout);
      const inputEl = e.target;
      const teacherId = this.dataset.teacherId;
      const hours = parseFloat(this.value) || 0;

      // Validate input
      if (hours < 0) {
        this.value = 0;
        showSaveIndicator('✗ الساعات يجب أن تكون أكبر من 0', 'error');
        return;
      }

      if (hours > 24) {
        this.value = 24;
        showSaveIndicator('✗ الساعات يجب ألا تتجاوز 24', 'error');
        return;
      }

      // Update data attribute
      inputEl.dataset.hours = hours;

      // Update hours display
      const row = this.closest('tr');
      const nameCell = row.querySelector('td:nth-child(2)');
      let hoursDisplay = nameCell.querySelector('.fs-xs');

      if (hours > 0) {
        if (hoursDisplay) {
          hoursDisplay.textContent = `${hours} ساعة`;
        } else {
          const nameDiv = nameCell.querySelector('div > div');
          if (nameDiv) {
            nameDiv.innerHTML += `<div class="fs-xs text-medium">${hours} ساعة</div>`;
          }
        }
      } else {
        if (hoursDisplay) {
          hoursDisplay.remove();
        }
      }

      // Debounced save
      timeout = setTimeout(() => {
        const checkedBox = row.querySelector('.attendance-checkbox:checked');
        const status = checkedBox ? checkedBox.dataset.status : 'none';
        saveAttendance(teacherId, status, hours);
      }, 1000);
    });

    input.addEventListener('blur', function () {
      updateStatistics();
    });
  });
}

// ===================================
// Page Event Handlers
// ===================================

// Class selection change
$('#classSession').on('select2:select', async function (e) {
  const classId = e.params.data.id;
  await getTeachers(classId);
});

// Date change
let attDate = document.getElementById('attendanceDate');
attDate.addEventListener('change', (e) => {
  attendanceDateForSelection = e.target.value;
  const currentClassId = classSession.value;
  if (currentClassId) {
    getTeachers(currentClassId);
  }
});

// Search functionality
const teacherSearch = document.getElementById('teacherSearch');
if (teacherSearch) {
  teacherSearch.addEventListener('input', function () {
    const searchTerm = this.value.toLowerCase();
    const rows = document.querySelectorAll('#teacherTable tr');
    let visibleCount = 0;

    rows.forEach((row) => {
      const teacherName = row
        .querySelector('td:nth-child(2)')
        ?.textContent.toLowerCase();
      if (teacherName) {
        const isVisible = teacherName.includes(searchTerm);
        row.style.display = isVisible ? '' : 'none';
        if (isVisible) visibleCount++;
      }
    });

    // Update count
    const totalEl = document.getElementById('totalTeachers');
    if (totalEl) {
      if (!totalEl.dataset.original) {
        totalEl.dataset.original = totalEl.textContent;
      }
      totalEl.textContent = searchTerm
        ? visibleCount
        : totalEl.dataset.original;
    }
  });
}

// Initialize
attachEventListeners();
updateStatistics();
