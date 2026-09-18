// Filter management for Teacher Wird/Matn Index page

document.addEventListener('DOMContentLoaded', () => {
    const filterToggle = document.getElementById('filterToggle');
    const filterCloseBtn = document.getElementById('filterCloseBtn');
    const filterContent = document.getElementById('filterContent');
    const studentInput = document.getElementById('filterStudent');
    const clearSearchBtn = document.getElementById('clearSearchBtn');
    const typeSelect = document.getElementById('filterType');
    const statusSelect = document.getElementById('filterStatus');
    const upcomingSelect = document.getElementById('filterUpcoming');
    const dateFromInput = document.getElementById('filterDateFrom');
    const dateToInput = document.getElementById('filterDateTo');
    const applyBtn = document.getElementById('applyFilters');
    const resetBtn = document.getElementById('resetFilters');
    const activeFiltersBadge = document.getElementById('activeFiltersBadge');

    // 1. Toggle advanced filter panel
    function toggleFilterPanel(open) {
        if (!filterContent) return;
        const isCurrentlyOpen = filterContent.style.display !== 'none' && filterContent.style.display !== '';
        const shouldOpen = open !== undefined ? open : !isCurrentlyOpen;
        filterContent.style.display = shouldOpen ? 'block' : 'none';
        if (filterToggle) {
            filterToggle.classList.toggle('active', shouldOpen);
        }
    }

    if (filterToggle) {
        filterToggle.addEventListener('click', (e) => {
            e.preventDefault();
            toggleFilterPanel();
        });
    }

    if (filterCloseBtn) {
        filterCloseBtn.addEventListener('click', (e) => {
            e.preventDefault();
            toggleFilterPanel(false);
        });
    }

    // 2. Count active filters and update badge
    function updateActiveFilterBadge() {
        if (!activeFiltersBadge) return;
        let count = 0;
        if (typeSelect && typeSelect.value) count++;
        if (statusSelect && statusSelect.value) count++;
        if (upcomingSelect && upcomingSelect.value) count++;

        const urlParams = new URLSearchParams(window.location.search);
        const hasCustomDateParam = urlParams.has('fromDate') || urlParams.has('toDate');
        if (hasCustomDateParam && urlParams.get('fromDate') !== '2020-01-01') {
            const todayStr = new Date().toISOString().split('T')[0];
            if (urlParams.get('fromDate') !== todayStr || urlParams.get('toDate') !== todayStr) {
                count++;
            }
        }

        if (count > 0) {
            activeFiltersBadge.textContent = count;
            activeFiltersBadge.style.display = 'inline-flex';
        } else {
            activeFiltersBadge.style.display = 'none';
        }
    }

    // 3. Search clear button visibility
    function updateClearSearchBtn() {
        if (!clearSearchBtn || !studentInput) return;
        clearSearchBtn.style.display = studentInput.value.trim().length > 0 ? 'flex' : 'none';
    }

    if (studentInput) {
        studentInput.addEventListener('input', () => {
            updateClearSearchBtn();
            applyClientFilters();
        });
    }

    if (clearSearchBtn) {
        clearSearchBtn.addEventListener('click', () => {
            if (studentInput) {
                studentInput.value = '';
                studentInput.focus();
            }
            updateClearSearchBtn();
            applyClientFilters();
        });
    }

    // 4. Reactive filter changes on dropdowns
    if (typeSelect) {
        typeSelect.addEventListener('change', () => {
            updateActiveFilterBadge();
            applyClientFilters();
        });
    }

    if (statusSelect) {
        statusSelect.addEventListener('change', () => {
            updateActiveFilterBadge();
            applyClientFilters();
        });
    }

    if (upcomingSelect) {
        upcomingSelect.addEventListener('change', () => {
            updateActiveFilterBadge();
            applyClientFilters();
        });
    }

    // 5. Main Filter function (client-side card filtering)
    window.applyClientFilters = function () {
        const studentFilter = (studentInput?.value || '').trim().toLowerCase();
        const typeFilter = (typeSelect?.value || '').trim().toLowerCase();
        const statusFilter = (statusSelect?.value || '').trim().toLowerCase();
        const upcomingFilter = (upcomingSelect?.value || '').trim();
        const workflow = window.currentWorkflowFilter || 'all';

        const cards = document.querySelectorAll('.wird-card');
        let visibleCount = 0;

        cards.forEach((card) => {
            const student = (card.dataset.student || '').toLowerCase();
            const type = (card.dataset.type || '').toLowerCase();
            const status = (card.dataset.status || '').toLowerCase();
            const upcoming = card.dataset.upcoming || 'false';
            const isToday = card.dataset.isToday === 'true' || card.getAttribute('data-is-today') === 'true';
            const isUnrated = card.dataset.isUnrated === 'true' || card.getAttribute('data-is-unrated') === 'true' || (status === 'notset' && upcoming !== 'true');
            const isCompleted = card.dataset.isCompleted === 'true' || card.getAttribute('data-is-completed') === 'true' || (status !== '' && status !== 'notset');

            let showCard = true;

            // Workflow filter tab
            if (workflow === 'unrated' && !isUnrated) showCard = false;
            else if (workflow === 'today' && !isToday) showCard = false;
            else if (workflow === 'completed' && !isCompleted) showCard = false;
            else if (workflow === 'upcoming' && upcoming !== 'true') showCard = false;

            // Student Name
            if (showCard && studentFilter && !student.includes(studentFilter)) {
                showCard = false;
            }

            // Type
            if (showCard && typeFilter && type !== typeFilter) {
                showCard = false;
            }

            // Status / Rating
            if (showCard && statusFilter) {
                if (statusFilter === 'notset') {
                    if (!isUnrated) showCard = false;
                } else if (status !== statusFilter) {
                    showCard = false;
                }
            }

            // Upcoming
            if (showCard && upcomingFilter) {
                if (upcoming !== upcomingFilter) {
                    showCard = false;
                }
            }

            if (showCard) {
                card.style.display = 'flex';
                visibleCount++;
            } else {
                card.style.display = 'none';
            }
        });

        // Results counter
        const resultsCount = document.getElementById('resultsCount');
        if (resultsCount) {
            resultsCount.innerHTML = `<strong>${visibleCount}</strong> من <strong>${cards.length}</strong> تسميع`;
        }

        // Empty state
        const noMatch = document.getElementById('noCardsMatchFilter');
        if (noMatch) {
            noMatch.style.display = (visibleCount === 0 && cards.length > 0) ? 'block' : 'none';
        }

        // Hide pagination if filtering
        const paginationWrap = document.getElementById('paginationWrap');
        if (paginationWrap) {
            const isFiltering = (workflow !== 'all' || !!studentFilter || !!typeFilter || !!statusFilter || !!upcomingFilter);
            paginationWrap.style.display = isFiltering ? 'none' : 'block';
        }
    };

    // 6. Apply Button: Handle custom date range submission or manual apply
    if (applyBtn) {
        applyBtn.addEventListener('click', (e) => {
            e.preventDefault();
            const dateFrom = (dateFromInput?.value || '').trim();
            const dateTo = (dateToInput?.value || '').trim();

            const urlParams = new URLSearchParams(window.location.search);
            const currentFrom = urlParams.get('fromDate') || '';
            const currentTo = urlParams.get('toDate') || '';

            // If dates changed compared to URL query string, navigate to fetch new data from server
            if ((dateFrom !== currentFrom || dateTo !== currentTo) && (dateFrom || dateTo)) {
                window.location.href = `/Teacher/Wird/Index?fromDate=${dateFrom}&toDate=${dateTo}`;
                return;
            }

            // Otherwise just apply client filters
            applyClientFilters();
            updateActiveFilterBadge();
            toggleFilterPanel(false);
        });
    }

    // 7. Reset Filters
    if (resetBtn) {
        resetBtn.addEventListener('click', (e) => {
            e.preventDefault();
            if (studentInput) studentInput.value = '';
            if (typeSelect) typeSelect.value = '';
            if (statusSelect) statusSelect.value = '';
            if (upcomingSelect) upcomingSelect.value = '';
            if (dateFromInput) dateFromInput.value = '';
            if (dateToInput) dateToInput.value = '';

            updateClearSearchBtn();
            updateActiveFilterBadge();

            // Check if custom date parameters are present in URL
            const urlParams = new URLSearchParams(window.location.search);
            if (urlParams.has('fromDate') || urlParams.has('toDate')) {
                // Return to clean Index (defaults to today)
                window.location.href = '/Teacher/Wird/Index';
                return;
            }

            window.currentWorkflowFilter = 'all';
            document.querySelectorAll('.wird-tabs-segmented .wird-tab-pill').forEach(btn => btn.classList.remove('active'));
            document.querySelector('.wird-tabs-segmented [data-filter="all"]')?.classList.add('active');

            const cards = document.querySelectorAll('.wird-card');
            cards.forEach(card => card.style.display = 'flex');

            const resultsCount = document.getElementById('resultsCount');
            if (resultsCount) {
                resultsCount.innerHTML = `<strong>${cards.length}</strong> تسميع`;
            }

            const noMatch = document.getElementById('noCardsMatchFilter');
            if (noMatch) noMatch.style.display = 'none';

            const paginationWrap = document.getElementById('paginationWrap');
            if (paginationWrap) paginationWrap.style.display = 'block';

            toggleFilterPanel(false);
        });
    }

    // Initial setup
    updateActiveFilterBadge();
    updateClearSearchBtn();
});
