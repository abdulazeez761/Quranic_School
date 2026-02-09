// Initialize Flatpickr
flatpickr('#attendanceDate', {
  dateFormat: 'Y-m-d',
  defaultDate: new Date(),
  locale: 'ar',
  wrap: false,
  theme: 'light',
});

// Initialize Select2
$('.select2-classes').select2({
  language: 'ar',
  dir: 'rtl',
  placeholder: 'اختر الحلقة',
  width: '100%',
});

// DOM Elements
const classSession = document.getElementById('classSession');
const teacherTable = document.getElementById('teacherTable');
let attendanceDateForSelection =
  document.getElementById('attendanceDate').value;
// // Initialize
// attachEventListeners();
// updateStatistics();
