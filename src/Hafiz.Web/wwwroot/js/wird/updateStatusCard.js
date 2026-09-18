/**
 * Hafiz Platform - Card Status Quick Rating Handler
 * Manages instant segmented rating buttons on Wird & Matn cards with snappy UI feedback.
 */
document.addEventListener('click', async function (e) {
  const btn = e.target.closest('.status-button');
  if (!btn) return;

  e.preventDefault();

  const card = btn.closest('.wird-card');
  if (!card) return;

  const id = btn.getAttribute('data-id');
  const status = btn.getAttribute('data-status');
  const originalText = btn.innerText.trim();
  const lowerStatus = (status || '').toLowerCase();

  // If already active and not retrying, allow re-click or subtle feedback
  const siblingButtons = card.querySelectorAll('.status-button');
  const prevActiveBtn = card.querySelector('.status-button.active');
  const cardStatusBadge = card.querySelector('.wird-status-badge');
  const prevBadgeText = cardStatusBadge ? cardStatusBadge.innerText : '';
  const prevBadgeClass = cardStatusBadge ? cardStatusBadge.className : '';
  const prevCardStatus = card.dataset.status || '';

  // 1. Instant Optimistic UI Update: Toggle active button & play delight bounce effect
  siblingButtons.forEach((b) => {
    b.classList.remove('active');
    b.classList.remove('seg-pop');
  });

  btn.classList.add('active', 'seg-pop');
  setTimeout(() => btn.classList.remove('seg-pop'), 450);

  // Optimistically remove upcoming state because rating and upcoming are mutually exclusive
  const upcomingBadge = card.querySelector('.upcoming-badge');
  if (upcomingBadge) {
    upcomingBadge.style.display = 'none';
  }
  card.classList.remove('is-upcoming');
  card.dataset.upcoming = 'false';

  // 2. Update or create status badge immediately
  let badgeToUpdate = cardStatusBadge;
  if (!badgeToUpdate) {
    const badgesWrap = card.querySelector('.wird-header-title .d-flex.gap-1') ||
                       card.querySelector('.wird-header-title .d-flex:last-child');
    if (badgesWrap) {
      badgeToUpdate = document.createElement('span');
      badgeToUpdate.className = `wird-status-badge status-${lowerStatus}`;
      badgesWrap.appendChild(badgeToUpdate);
    }
  }

  if (badgeToUpdate) {
    badgeToUpdate.style.display = '';
    badgeToUpdate.innerText = originalText;
    badgeToUpdate.className = `wird-status-badge status-${lowerStatus}`;
    badgeToUpdate.classList.add('badge-pop');
    setTimeout(() => badgeToUpdate.classList.remove('badge-pop'), 400);
  }

  // 3. Update card classes
  const allStatuses = ['excellent', 'verygood', 'good', 'fair', 'poor', 'notset'];
  allStatuses.forEach((s) => card.classList.remove(s));
  card.classList.add(lowerStatus);
  card.dataset.status = lowerStatus;

  // 4. Send AJAX Update
  const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
  const token = tokenInput ? tokenInput.value : '';

  try {
    const res = await fetch('/Teacher/Wird/UpdateStatus', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        RequestVerificationToken: token,
      },
      body: JSON.stringify({ Id: id, Status: status }),
    });

    const data = await res.json();

    if (data && data.success) {
      // Grading clears upcoming flag
      if (lowerStatus !== 'notset') {
        card.classList.remove('is-upcoming');
        card.dataset.upcoming = 'false';
        const upcomingBadge = card.querySelector('.upcoming-badge');
        if (upcomingBadge) upcomingBadge.remove();
      }

      // Success micro-glow on card
      card.classList.add('updated');
      setTimeout(() => {
        card.classList.remove('updated');
      }, 700);
    } else {
      // Server rejected -> Rollback
      rollbackRating(siblingButtons, prevActiveBtn, cardStatusBadge, prevBadgeText, prevBadgeClass, card, prevCardStatus);
    }
  } catch (error) {
    console.error('Error updating status:', error);
    // Network error -> Rollback
    rollbackRating(siblingButtons, prevActiveBtn, cardStatusBadge, prevBadgeText, prevBadgeClass, card, prevCardStatus);
  }
});

function rollbackRating(buttons, prevActiveBtn, badge, prevText, prevClass, card, prevStatus) {
  buttons.forEach((b) => b.classList.remove('active'));
  if (prevActiveBtn) prevActiveBtn.classList.add('active');

  if (badge) {
    badge.innerText = prevText;
    badge.className = prevClass;
  }

  if (card) {
    const allStatuses = ['excellent', 'verygood', 'good', 'fair', 'poor', 'notset'];
    allStatuses.forEach((s) => card.classList.remove(s));
    if (prevStatus) card.classList.add(prevStatus);
    card.dataset.status = prevStatus;

    card.classList.add('failed-to-update');
    setTimeout(() => card.classList.remove('failed-to-update'), 800);
  }
}
