const classSession = document.getElementById('classSession');
const studentTable = document.getElementById('studentsTable');
let attendanceDateForSelection =
  document.getElementById('attendanceDate').value;

// Handle attendance change
document.querySelectorAll('.attendance-checkbox').forEach((checkbox) => {
  checkbox.addEventListener('change', function () {
    const row = this.closest('tr');
    const studentId = this.dataset.studentId;
    const status = this.dataset.status;

    // Update row class
    row.className = 'has-attendance';
    if (this.checked) {
      row.classList.add(`status-${this.classList[1].replace('status-', '')}`);
    }

    // Update statistics
    updateStatistics();
  });
});

// Initialize Flatpickr
flatpickr('#attendanceDate', {
  dateFormat: 'Y-m-d',
  defaultDate: new Date(),
  locale: 'ar',
  wrap: false,
  theme: 'light',
  onChange: function (selectedDates, dateStr, instance) {},
});

// Initialize Select2
$('.select2-classes').select2({
  language: 'ar',
  dir: 'rtl',
  placeholder: 'اختر الحلقة',
  width: '100%',
});

// Search functionality
document.getElementById('studentSearch').addEventListener('input', function () {
  const searchTerm = this.value.toLowerCase();
  document.querySelectorAll('#studentsTable tr').forEach((row) => {
    const studentName = row
      .querySelector('td:nth-child(2)')
      ?.textContent.toLowerCase();
    row.style.display =
      studentName && studentName.includes(searchTerm) ? '' : 'none';
  });
});

// Initialize statistics on page load
updateStatistics();

// Load existing attendance script
attachEventListeners();
