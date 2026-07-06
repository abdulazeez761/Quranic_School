// Search and Filter Functionality
const searchInput = document.getElementById('searchInput');
const typeFilter = document.getElementById('genderFilter');
const clearFiltersBtn = document.getElementById('clearFilters');
const recordCards = document.querySelectorAll('.record-card');
const gridContainer = document.getElementById('classesGrid');
const visibleCountSpan = document.getElementById('visibleCount');

function filterTable() {
  const searchTerm = searchInput.value.toLowerCase().trim();
  const selectedType = typeFilter.value.toLowerCase();
  let visibleCount = 0;

  recordCards.forEach((card) => {
    const name = card.dataset.name || '';
    const gender = card.dataset.gender || '';

    const matchesSearch = !searchTerm || name.includes(searchTerm);
    const matchesType = !selectedType || gender.includes(selectedType);

    const isVisible = matchesSearch && matchesType;
    card.style.display = isVisible ? '' : 'none';

    if (isVisible) visibleCount++;
  });

  if (visibleCountSpan) visibleCountSpan.textContent = visibleCount;

  if (visibleCount === 0) showNoResultsMessage();
  else hideNoResultsMessage();
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
                <p class="text-medium">لم يتم العثور على حلقات مطابقة لمعايير البحث</p>
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
  if (noResults) noResults.style.display = 'none';
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

console.log('✓ Classes List Page Ready');
