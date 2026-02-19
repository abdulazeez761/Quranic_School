document.addEventListener('DOMContentLoaded', function () {
  const searchInput = document.getElementById('searchInput');
  const typeFilter = document.getElementById('typeFilter');
  const clearButton = document.getElementById('clearFilters');
  const resultCount = document.getElementById('resultCount');
  const rows = document.querySelectorAll('.table tbody tr');

  function filterRows() {
    const searchValue = searchInput.value.toLowerCase().trim();
    const selectedType = typeFilter.value.toLowerCase();
    let visibleCount = 0;

    rows.forEach((row) => {
      const name = row.getAttribute('data-name') || '';
      const gender = row.getAttribute('data-gender') || '';

      const nameMatch = !searchValue || name.includes(searchValue);
      const genderMatch = !selectedType || gender == selectedType;

      if (nameMatch && genderMatch) {
        row.style.display = '';
        visibleCount++;
      } else {
        row.style.display = 'none';
      }
    });

    if (searchValue || selectedType) {
      resultCount.textContent = `عرض ${visibleCount} من أصل ${rows.length}`;
    } else {
      resultCount.textContent = '';
    }
  }

  searchInput.addEventListener('input', filterRows);
  typeFilter.addEventListener('change', filterRows);

  clearButton.addEventListener('click', function () {
    searchInput.value = '';
    typeFilter.value = '';
    filterRows();
  });
});
