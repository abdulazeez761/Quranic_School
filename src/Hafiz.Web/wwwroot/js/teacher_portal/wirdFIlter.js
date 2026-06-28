// Filter toggle functionality
document.getElementById('filterToggle')?.addEventListener('click', function () {
  const content = document.getElementById('filterContent');
  const toggle = this;

  if (content.style.display === 'none') {
    content.style.display = 'block';
    toggle.classList.add('active');
  } else {
    content.style.display = 'none';
    toggle.classList.remove('active');
  }
});

// Apply filters
document
  .getElementById('applyFilters')
  ?.addEventListener('click', async function () {
    const studentFilter = document
      .getElementById('filterStudent')
      .value.toLowerCase();
    const typeFilter = document
      .getElementById('filterType')
      .value.toLowerCase();
    const statusFilter = document
      .getElementById('filterStatus')
      .value.toLowerCase();
    const upcomingFilter = document.getElementById('filterUpcoming').value;
    const dateFromFilter = document.getElementById('filterDateFrom').value;
    const dateToFilter = document.getElementById('filterDateTo').value;

    const cards = document.querySelectorAll('.wird-card');
    let visibleCount = 0;

    if (dateFromFilter || dateToFilter) {
      window.location.href = `/Teacher/Wird/Index?fromDate=${dateFromFilter}&toDate=${dateToFilter}`;
    }

    cards.forEach((card) => {
      const student = card.dataset.student.toLowerCase();
      const type = card.dataset.type.toLowerCase();
      const status = card.dataset.status.toLowerCase();
      const upcoming = card.dataset.upcoming;

      let showCard = true;

      if (studentFilter && !student.includes(studentFilter)) {
        showCard = false;
      }

      if (typeFilter && type !== typeFilter) {
        showCard = false;
      }

      if (statusFilter && status !== statusFilter) {
        showCard = false;
      }

      if (upcomingFilter && upcoming !== upcomingFilter) {
        showCard = false;
      }

      if (showCard) {
        card.style.display = 'flex';
        visibleCount++;
      } else {
        card.style.display = 'none';
      }
    });

    document.getElementById('resultsCount').innerHTML =
      `<strong>${visibleCount}</strong> of <strong>${cards.length}</strong> assignments`;
  });

// Reset filters
document.getElementById('resetFilters')?.addEventListener('click', function () {
  document.getElementById('filterStudent').value = '';
  document.getElementById('filterType').value = '';
  document.getElementById('filterStatus').value = '';
  document.getElementById('filterUpcoming').value = '';
  document.getElementById('filterDateFrom').value = '';
  document.getElementById('filterDateTo').value = '';

  const cards = document.querySelectorAll('.wird-card');
  cards.forEach((card) => {
    card.style.display = 'flex';
  });

  document.getElementById('resultsCount').innerHTML =
    `<strong>${cards.length}</strong> assignments`;
});

// Real-time search on student name
document
  .getElementById('filterStudent')
  ?.addEventListener('input', function () {
    document.getElementById('applyFilters').click();
  });

// Apply immediately when the upcoming filter changes
document
  .getElementById('filterUpcoming')
  ?.addEventListener('change', function () {
    document.getElementById('applyFilters').click();
  });
