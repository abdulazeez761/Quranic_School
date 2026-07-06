// Search and Filter Functionality for Parents
const searchInput = document.getElementById('searchInput');
const clearFiltersBtn = document.getElementById('clearFilters');
const recordCards = document.querySelectorAll('.record-card');
const gridContainer = document.getElementById('parentsGrid');
const visibleCountSpan = document.getElementById('visibleCount');

function filterTable() {
  const searchTerm = searchInput.value.toLowerCase().trim();
  let visibleCount = 0;

  recordCards.forEach((card) => {
    const firstName = card.dataset.firstname || '';
    const secondName = card.dataset.secondname || '';

    const matchesSearch =
      !searchTerm ||
      firstName.includes(searchTerm) ||
      secondName.includes(searchTerm);

    const isVisible = matchesSearch;
    card.style.display = isVisible ? '' : 'none';

    if (isVisible) visibleCount++;
  });

  // Update visible count
  if (visibleCountSpan) visibleCountSpan.textContent = visibleCount;

  // Show message if no results
  if (visibleCount === 0) {
    showNoResultsMessage();
  } else {
    hideNoResultsMessage();
  }
}

function showNoResultsMessage() {
  if (!gridContainer) return;
  let noResults = gridContainer.querySelector('.no-results-card');

  if (!noResults) {
    noResults = document.createElement('div');
    noResults.className = 'record-empty no-results-card';
    noResults.innerHTML = `
                <i class='bx bx-search-alt'></i>
                <h3 class="record-empty-title">لا توجد نتائج</h3>
                <p class="text-medium">لم يتم العثور على أولياء أمور يطابقون معايير البحث</p>
                <button class="btn btn-outline mt-md" onclick="clearFilters()">
                    <i class='bx bx-refresh'></i> مسح الفلاتر
                </button>
            `;
    gridContainer.appendChild(noResults);
  }
  noResults.style.display = '';
}

function hideNoResultsMessage() {
  const noResults = document.querySelector('.no-results-card');
  if (noResults) {
    noResults.style.display = 'none';
  }
}

function clearFilters() {
  searchInput.value = '';
  filterTable();
}

// Event Listeners
searchInput.addEventListener('input', filterTable);
clearFiltersBtn.addEventListener('click', clearFilters);

// Make clearFilters available globally
window.clearFilters = clearFilters;

// Export functionality (placeholder)
const exportBtn = document.getElementById('exportBtn');
if (exportBtn) {
  exportBtn.addEventListener('click', function () {
    if (confirm('هل تريد تصدير قائمة أولياء الأمور إلى Excel؟')) {
      AdminDashboard.showNotification(
        'سيتم إضافة وظيفة التصدير قريباً',
        'info'
      );
    }
  });
}

console.log('✓ Parents List Page Ready');
