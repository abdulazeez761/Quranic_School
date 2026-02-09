// Search and Filter Functionality for Parents
const searchInput = document.getElementById('searchInput');
const clearFiltersBtn = document.getElementById('clearFilters');
const tableRows = document.querySelectorAll('.table-row');
const visibleCountSpan = document.getElementById('visibleCount');

function filterTable() {
  const searchTerm = searchInput.value.toLowerCase().trim();
  let visibleCount = 0;

  tableRows.forEach((row) => {
    const firstName = row.dataset.firstname || '';
    const secondName = row.dataset.secondname || '';

    const matchesSearch =
      !searchTerm ||
      firstName.includes(searchTerm) ||
      secondName.includes(searchTerm);

    const isVisible = matchesSearch;
    row.style.display = isVisible ? '' : 'none';

    if (isVisible) visibleCount++;
  });

  // Update visible count
  visibleCountSpan.textContent = visibleCount;

  // Show message if no results
  if (visibleCount === 0) {
    showNoResultsMessage();
  } else {
    hideNoResultsMessage();
  }
}

function showNoResultsMessage() {
  const tbody = document.getElementById('parentsTableBody');
  let noResultsRow = tbody.querySelector('.no-results-row');

  if (!noResultsRow) {
    noResultsRow = document.createElement('tr');
    noResultsRow.className = 'no-results-row';
    noResultsRow.innerHTML = `
                <td colspan="7">
                    <div class="table-empty">
                        <i class='bx bx-search-alt'></i>
                        <h3 class="table-empty-title">لا توجد نتائج</h3>
                        <p class="text-medium">لم يتم العثور على أولياء أمور يطابقون معايير البحث</p>
                        <button class="btn btn-outline mt-md" onclick="clearFilters()">
                            <i class='bx bx-refresh'></i> مسح الفلاتر
                        </button>
                    </div>
                </td>
            `;
    tbody.appendChild(noResultsRow);
  }
  noResultsRow.style.display = '';
}

function hideNoResultsMessage() {
  const noResultsRow = document.querySelector('.no-results-row');
  if (noResultsRow) {
    noResultsRow.style.display = 'none';
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

// Initialize sorting if available
document.querySelectorAll('.sortable').forEach((header) => {
  header.style.cursor = 'pointer';
  header.addEventListener('click', function () {
    const columnIndex = Array.from(this.parentElement.children).indexOf(this);
    sortTable(columnIndex);
  });
});

function sortTable(columnIndex) {
  const tbody = document.getElementById('parentsTableBody');
  const rows = Array.from(tbody.querySelectorAll('.table-row'));

  // Toggle sort direction
  const currentSort = tbody.dataset.sortColumn;
  const currentDirection = tbody.dataset.sortDirection || 'asc';
  const newDirection =
    currentSort == columnIndex && currentDirection === 'asc' ? 'desc' : 'asc';

  tbody.dataset.sortColumn = columnIndex;
  tbody.dataset.sortDirection = newDirection;

  rows.sort((a, b) => {
    const aText = a.children[columnIndex]?.textContent.trim() || '';
    const bText = b.children[columnIndex]?.textContent.trim() || '';

    const comparison = aText.localeCompare(bText, 'ar');
    return newDirection === 'asc' ? comparison : -comparison;
  });

  // Reappend sorted rows
  rows.forEach((row) => tbody.appendChild(row));

  // Update visual indicators
  document.querySelectorAll('.sortable').forEach((h) => {
    h.classList.remove('sorted-asc', 'sorted-desc');
  });

  const sortedHeader = tbody.parentElement.querySelector(
    `thead tr th:nth-child(${columnIndex + 1})`
  );
  sortedHeader.classList.add(
    newDirection === 'asc' ? 'sorted-asc' : 'sorted-desc'
  );
}

console.log('✓ Parents List Page Ready');
