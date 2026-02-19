function showSaveIndicator(message, type = 'success') {
  const indicator = document.getElementById('saveIndicator');
  const iconClass =
    type === 'success'
      ? 'bx-check-circle'
      : type === 'error'
      ? 'bx-error-circle'
      : 'bx-loader-alt bx-spin';

  indicator.innerHTML = `<i class='bx ${iconClass}'></i><span>${message}</span>`;
  indicator.className = 'save-indicator show ' + type;

  if (type !== 'loading') {
    setTimeout(() => indicator.classList.remove('show'), 2000);
  }
}

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
