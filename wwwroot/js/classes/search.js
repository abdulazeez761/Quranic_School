// Search and Filter Functionality
const searchInput = document.getElementById('searchInput');
const typeFilter = document.getElementById('genderFilter');
const clearFiltersBtn = document.getElementById('clearFilters');
const tableRows = document.querySelectorAll('.table-row');
const visibleCountSpan = document.getElementById('visibleCount');

function filterTable() {
  const searchTerm = searchInput.value.toLowerCase().trim();
  const selectedType = typeFilter.value.toLowerCase();
  let visibleCount = 0;

  tableRows.forEach((row) => {
    const name = row.dataset.name || '';
    const gender = row.dataset.gender || '';

    const matchesSearch = !searchTerm || name.includes(searchTerm);
    const matchesType = !selectedType || gender.includes(selectedType);

    const isVisible = matchesSearch && matchesType;
    row.style.display = isVisible ? '' : 'none';

    if (isVisible) visibleCount++;
  });

  visibleCountSpan.textContent = visibleCount;

  if (visibleCount === 0) showNoResultsMessage();
  else hideNoResultsMessage();
}

function showNoResultsMessage() {
  const tbody = document.getElementById('classesTableBody');
  let noResultsRow = tbody.querySelector('.no-results-row');

  if (!noResultsRow) {
    noResultsRow = document.createElement('tr');
    noResultsRow.className = 'no-results-row';
    noResultsRow.innerHTML = `
      <td colspan="6">
        <div class="table-empty">
          <i class='bx bx-search-alt'></i>
          <h3 class="table-empty-title">لا توجد نتائج</h3>
          <p class="text-medium">لم يتم العثور على حلقات مطابقة لمعايير البحث</p>
          <button class="btn btn-outline mt-md" onclick="clearFilters()">
            <i class='bx bx-refresh'></i> مسح الفلاتر
          </button>
        </div>
      </td>`;
    tbody.appendChild(noResultsRow);
  }

  noResultsRow.style.display = '';
}

function hideNoResultsMessage() {
  const noResultsRow = document.querySelector('.no-results-row');
  if (noResultsRow) noResultsRow.style.display = 'none';
}

function clearFilters() {
  searchInput.value = '';
  typeFilter.value = '';
  filterTable();
}

searchInput.addEventListener('input', filterTable);
typeFilter.addEventListener('change', filterTable);
clearFiltersBtn.addEventListener('click', clearFilters);
window.clearFilters = clearFilters;
