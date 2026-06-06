// Shared state — declared with var so it is hoisted and
// accessible from events.js which loads before this file finishes.
var attendanceDateForSelection =
  document.getElementById('attendanceDate').value;

// DOM Elements
const classSession = document.getElementById('classSession');
const teacherTable = document.getElementById('teacherTable');

// Initialize Flatpickr
// Use the onChange callback — flatpickr does NOT reliably fire the
// native DOM 'change' event, so we update the shared date variable
// and reload the teacher list directly here.
flatpickr('#attendanceDate', {
  dateFormat: 'Y-m-d',
  defaultDate: new Date(),
  locale: 'ar',
  wrap: false,
  theme: 'light',
  onChange: function (selectedDates, dateStr) {
    attendanceDateForSelection = dateStr;
    const currentClassId = classSession ? classSession.value : '';
    if (currentClassId) {
      getTeachers(currentClassId);
    }
  },
});

// Initialize Select2
$('.select2-classes').select2({
  language: 'ar',
  dir: 'rtl',
  placeholder: 'اختر الحلقة',
  width: '100%',
});
