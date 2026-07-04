// التقارير اليومية: طي/توسيع الشُعب، البحث بالاسم، والتصفية حسب حالة الحضور
(function () {
  const cards = Array.from(document.querySelectorAll('.dr-class-card'));
  const searchInput = document.getElementById('drClassSearch');
  const statusFilter = document.getElementById('drStatusFilter');
  const expandAllBtn = document.getElementById('drExpandAll');
  const collapseAllBtn = document.getElementById('drCollapseAll');
  const noMatch = document.getElementById('drNoMatch');

  function setExpanded(card, expanded) {
    card.classList.toggle('dr-expanded', expanded);
    const header = card.querySelector('.dr-class-header');
    if (header)
      header.setAttribute('aria-expanded', expanded ? 'true' : 'false');
  }

  // طي/توسيع عند الضغط على ترويسة الشعبة
  cards.forEach(function (card) {
    const header = card.querySelector('.dr-class-header');
    if (!header) return;
    header.addEventListener('click', function () {
      setExpanded(card, !card.classList.contains('dr-expanded'));
    });
    header.addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        setExpanded(card, !card.classList.contains('dr-expanded'));
      }
    });
  });

  function applyFilters() {
    const term = (searchInput.value || '').trim().toLowerCase();
    const status = statusFilter.value;
    let visibleCount = 0;

    cards.forEach(function (card) {
      const nameMatch = !term || (card.dataset.name || '').indexOf(term) !== -1;
      let statusMatch = true;

      const rows = card.querySelectorAll('.dr-student-row');
      if (status) {
        let matched = 0;
        rows.forEach(function (row) {
          const show = row.dataset.status === status;
          row.style.display = show ? '' : 'none';
          if (show) matched++;
        });
        statusMatch = matched > 0;
      } else {
        rows.forEach(function (row) {
          row.style.display = '';
        });
      }

      const show = nameMatch && statusMatch;
      card.classList.toggle('dr-hidden', !show);
      if (show) {
        visibleCount++;
        // عند وجود بحث أو تصفية حالة، وسّع الشعب المطابقة تلقائياً
        if (term || status) setExpanded(card, true);
      }
    });

    if (noMatch) noMatch.style.display = visibleCount === 0 ? 'block' : 'none';
  }

  if (searchInput) searchInput.addEventListener('input', applyFilters);
  if (statusFilter) statusFilter.addEventListener('change', applyFilters);

  if (expandAllBtn)
    expandAllBtn.addEventListener('click', function () {
      cards.forEach(function (c) {
        if (!c.classList.contains('dr-hidden')) setExpanded(c, true);
      });
    });
  if (collapseAllBtn)
    collapseAllBtn.addEventListener('click', function () {
      cards.forEach(function (c) {
        setExpanded(c, false);
      });
    });
})();
