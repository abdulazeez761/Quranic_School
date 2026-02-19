function updateStatistics() {
  const present = document.querySelectorAll('.status-present:checked').length;
  const late = document.querySelectorAll('.status-late:checked').length;
  const absent = document.querySelectorAll('.status-absent:checked').length;
  const excuse = document.querySelectorAll('.status-excuse:checked').length;

  document.getElementById('presentCount').textContent = present;
  document.getElementById('lateCount').textContent = late;
  document.getElementById('absentCount').textContent = absent;
  document.getElementById('excuseCount').textContent = excuse;
}

function showSaveIndicator(message, type = 'success') {
  const indicator = document.getElementById('saveIndicator');
  indicator.textContent = message;
  indicator.className = 'save-indicator show ' + type;

  setTimeout(() => indicator.classList.remove('show'), 2000);
}
