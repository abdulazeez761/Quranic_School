// Search and Filter Functionality
const searchInput = document.getElementById('searchInput');
const classFilter = document.getElementById('classFilter');
const clearFiltersBtn = document.getElementById('clearFilters');
const tableRows = document.querySelectorAll('.table-row');
const visibleCountSpan = document.getElementById('visibleCount');
// const totalCount = @totalTeachers;

function filterTable() {
  const searchTerm = searchInput.value.toLowerCase().trim();
  const selectedClass = classFilter.value.toLowerCase();
  let visibleCount = 0;

  tableRows.forEach((row) => {
    const firstName = row.dataset.firstname || '';
    const secondName = row.dataset.secondname || '';
    const classes = row.dataset.classes || '';

    const matchesSearch =
      !searchTerm ||
      firstName.includes(searchTerm) ||
      secondName.includes(searchTerm);

    const matchesClass =
      !selectedClass || classes.toLowerCase().includes(selectedClass);

    const isVisible = matchesSearch && matchesClass;
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
  const tbody = document.getElementById('teachersTableBody');
  let noResultsRow = tbody.querySelector('.no-results-row');

  if (!noResultsRow) {
    noResultsRow = document.createElement('tr');
    noResultsRow.className = 'no-results-row';
    noResultsRow.innerHTML = `
                <td colspan="7">
                    <div class="table-empty">
                        <i class='bx bx-search-alt'></i>
                        <h3 class="table-empty-title">لا توجد نتائج</h3>
                        <p class="text-medium">لم يتم العثور على مدرسين يطابقون معايير البحث</p>
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
  classFilter.value = '';
  filterTable();
}

// Event Listeners
searchInput.addEventListener('input', filterTable);
classFilter.addEventListener('change', filterTable);
clearFiltersBtn.addEventListener('click', clearFilters);

// Make clearFilters available globally
window.clearFilters = clearFilters;

const exportBtn = document.getElementById('exportBtn');
if (exportBtn) {
  exportBtn.addEventListener('click', function () {
    alert('سيتم إضافة وظيفة التصدير قريباً');
  });
}

console.log('✓ Teachers List Page Ready');
