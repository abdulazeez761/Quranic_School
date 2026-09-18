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

    const workflow = window.currentWorkflowFilter || 'all';

    cards.forEach((card) => {
      const student = (card.dataset.student || '').toLowerCase();
      const type = (card.dataset.type || '').toLowerCase();
      const status = (card.dataset.status || '').toLowerCase();
      const upcoming = card.dataset.upcoming || 'false';
      const isToday = card.dataset.isToday === 'true' || card.getAttribute('data-is-today') === 'true';
      const isUnrated = card.dataset.isUnrated === 'true' || card.getAttribute('data-is-unrated') === 'true' || (status === 'notset' && upcoming !== 'true');
      const isCompleted = card.dataset.isCompleted === 'true' || card.getAttribute('data-is-completed') === 'true' || (status !== '' && status !== 'notset');

      let showCard = true;

      // 1. Workflow filter tab
      if (workflow === 'unrated' && !isUnrated) showCard = false;
      else if (workflow === 'today' && !isToday) showCard = false;
      else if (workflow === 'completed' && !isCompleted) showCard = false;
      else if (workflow === 'upcoming' && upcoming !== 'true') showCard = false;

      // 2. Input filters
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

    const resultsCount = document.getElementById('resultsCount');
    if (resultsCount) {
      resultsCount.innerHTML = `<strong>${visibleCount}</strong> من <strong>${cards.length}</strong> تسميع`;
    }

    const noMatch = document.getElementById('noCardsMatchFilter');
    if (noMatch) {
      noMatch.style.display = (visibleCount === 0 && cards.length > 0) ? 'block' : 'none';
    }

    const paginationWrap = document.getElementById('paginationWrap');
    if (paginationWrap) {
      const isFiltering = (workflow !== 'all' || !!studentFilter || !!typeFilter || !!statusFilter || !!upcomingFilter);
      paginationWrap.style.display = isFiltering ? 'none' : 'block';
    }
  });

// Reset filters
document.getElementById('resetFilters')?.addEventListener('click', function () {
  const studentInput = document.getElementById('filterStudent');
  const typeSelect = document.getElementById('filterType');
  const statusSelect = document.getElementById('filterStatus');
  const upcomingSelect = document.getElementById('filterUpcoming');
  const dateFromInput = document.getElementById('filterDateFrom');
  const dateToInput = document.getElementById('filterDateTo');

  if (studentInput) studentInput.value = '';
  if (typeSelect) typeSelect.value = '';
  if (statusSelect) statusSelect.value = '';
  if (upcomingSelect) upcomingSelect.value = '';
  if (dateFromInput) dateFromInput.value = '';
  if (dateToInput) dateToInput.value = '';

  window.currentWorkflowFilter = 'all';
  document.querySelectorAll('.wird-tabs-segmented .wird-tab-pill').forEach(btn => btn.classList.remove('active'));
  document.querySelector('.wird-tabs-segmented [data-filter="all"]')?.classList.add('active');

  const cards = document.querySelectorAll('.wird-card');
  cards.forEach((card) => {
    card.style.display = 'flex';
  });

  const resultsCount = document.getElementById('resultsCount');
  if (resultsCount) {
    resultsCount.innerHTML = `<strong>${cards.length}</strong> تسميع`;
  }

  const noMatch = document.getElementById('noCardsMatchFilter');
  if (noMatch) noMatch.style.display = 'none';

  const paginationWrap = document.getElementById('paginationWrap');
  if (paginationWrap) paginationWrap.style.display = 'block';
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
