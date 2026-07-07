// Load-more handler for the two dashboard activity feeds (wirds + attendance).
// Each feed is a self-contained <div data-activity-feed data-category="…">
// containing a list, a footer button, and a hidden state marker inside the AJAX response.
(function () {
  'use strict';

  const ENDPOINT = '/Admin/Home/LoadActivity';

  function findFeed(el) {
    return el.closest('[data-activity-feed]');
  }

  async function loadMore(button) {
    const feed = findFeed(button);
    if (!feed) return;
    const category = feed.getAttribute('data-category');
    const nextPage = parseInt(button.getAttribute('data-next-page') || '1', 10);
    const list = feed.querySelector('[data-activity-list]');
    if (!list) return;

    button.disabled = true;
    const originalLabel = button.innerHTML;
    button.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i><span>جاري التحميل...</span>";

    try {
      const url = `${ENDPOINT}?category=${encodeURIComponent(category)}&page=${nextPage}`;
      const res = await fetch(url, {
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        credentials: 'same-origin',
      });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const html = await res.text();

      // Parse the response, split items from the state marker, append to the list.
      const holder = document.createElement('div');
      holder.innerHTML = html;
      const state = holder.querySelector('.dashboard-activity-state');
      const hasMore = state?.getAttribute('data-has-more') === 'true';
      const followingPage = parseInt(state?.getAttribute('data-next-page') || '0', 10);
      state?.remove();

      // Everything left in holder is the new item DOM — append it.
      while (holder.firstChild) list.appendChild(holder.firstChild);

      if (hasMore) {
        button.setAttribute('data-next-page', String(followingPage));
        button.hidden = false;
        button.innerHTML = originalLabel;
        button.disabled = false;
      } else {
        button.remove();
      }
    } catch (err) {
      console.error('Failed to load more activity', err);
      button.innerHTML = originalLabel;
      button.disabled = false;
    }
  }

  document.addEventListener('click', function (e) {
    const button = e.target.closest('[data-activity-load-more]');
    if (!button) return;
    e.preventDefault();
    loadMore(button);
  });
})();
