document.addEventListener('click', async function (e) {
  const btn = e.target.closest('.status-button');

  if (btn) {
    e.preventDefault();

    let id = btn.getAttribute('data-id');
    let status = btn.getAttribute('data-status');
    let originalText = btn.innerText.trim();

    let tokenInput = document.querySelector(
      'input[name="__RequestVerificationToken"]',
    );
    let token = tokenInput ? tokenInput.value : '';

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
      const card = btn.closest('.wird-card');

      if (data.success) {
        let cardStatus = card.querySelector('.wird-status-badge');

        cardStatus.innerText = originalText;
        cardStatus.className = `wird-status-badge status-${status.toLowerCase()}`;

        const allStatuses = ['excellent', 'verygood', 'good', 'fair', 'poor', 'notset'];
        allStatuses.forEach((s) => card.classList.remove(s));
        card.classList.add(status.toLowerCase());
        card.dataset.status = status.toLowerCase();

        // Grading clears the "upcoming" flag server-side; mirror that in the DOM.
        if (status.toLowerCase() !== 'notset') {
          card.classList.remove('is-upcoming');
          card.dataset.upcoming = 'false';
          const upcomingBadge = card.querySelector('.upcoming-badge');
          if (upcomingBadge) upcomingBadge.remove();
        }

        card.classList.add('updated');

        setTimeout(() => {
          card.classList.remove('updated');
        }, 1000);
      } else {
        card.classList.add('failed-to-update');

        setTimeout(() => {
          card.classList.remove('failed-to-update');
        }, 1000);
      }
    } catch (error) {
      console.error('Error updating status:', error);

      const card = btn.closest('.wird-card');
      card.classList.add('failed-to-update');

      setTimeout(() => {
        card.classList.remove('failed-to-update');
      }, 1000);
    }
  }
});
