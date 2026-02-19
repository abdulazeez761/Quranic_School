// Filter toggle functionality
document.getElementById('filterToggle')?.addEventListener('click', function () {
  const content = document.getElementById('filterContent');
  const toggle = this;

  if (content.style.display === 'none') {
    content.style.display = 'grid';
    toggle.textContent = '▼';
  } else {
    content.style.display = 'none';
    toggle.textContent = '▶';
  }
});

// Apply filters
document
  .getElementById('applyFilters')
  ?.addEventListener('click', async function () {
    const studentFilter = document
      .getElementById('filterStudent')
      .value.toLowerCase();
    const typeFilter = document
      .getElementById('filterType')
      .value.toLowerCase();
    const statusFilter = document
      .getElementById('filterStatus')
      .value.toLowerCase();
    const dateFromFilter = document.getElementById('filterDateFrom').value;
    const dateToFilter = document.getElementById('filterDateTo').value;

    const cards = document.querySelectorAll('.wird-card');
    let visibleCount = 0;

    if (dateFromFilter || dateToFilter) {
      window.location.href = `/Teacher/Wird/Index?fromDate=${dateFromFilter}&toDate=${dateToFilter}`;
    }

    cards.forEach((card) => {
      const student = card.dataset.student.toLowerCase();
      const type = card.dataset.type.toLowerCase();
      const status = card.dataset.status.toLowerCase();

      let showCard = true;

      // Student name filter
      if (studentFilter && !student.includes(studentFilter)) {
        showCard = false;
      }

      // Type filter
      if (typeFilter && type !== typeFilter) {
        showCard = false;
      }

      // Status filter
      if (statusFilter && status !== statusFilter) {
        showCard = false;
      }

      if (showCard) {
        card.style.display = 'block';
        visibleCount++;
      } else {
        card.style.display = 'none';
      }
    });

    // Update results count
    document.getElementById(
      'resultsCount'
    ).innerHTML = `Showing <strong>${visibleCount}</strong> of <strong>${cards.length}</strong> assignments`;
  });

// Reset filters
document.getElementById('resetFilters')?.addEventListener('click', function () {
  document.getElementById('filterStudent').value = '';
  document.getElementById('filterType').value = '';
  document.getElementById('filterStatus').value = '';
  document.getElementById('filterDateFrom').value = '';
  document.getElementById('filterDateTo').value = '';

  const cards = document.querySelectorAll('.wird-card');
  cards.forEach((card) => {
    card.style.display = 'block';
  });

  // Update results count
  document.getElementById(
    'resultsCount'
  ).innerHTML = `Showing <strong>${cards.length}</strong> assignments`;
});

// Real-time search on student name
document
  .getElementById('filterStudent')
  ?.addEventListener('input', function () {
    document.getElementById('applyFilters').click();
  });

// ===================================
// NOTES EDITING FUNCTIONALITY
// ===================================

// Edit note button click
document.querySelectorAll('.edit-note-btn').forEach((btn) => {
  btn.addEventListener('click', function () {
    const assignmentId = this.dataset.assignmentId;
    const editSection = document.getElementById(`notes-edit-${assignmentId}`);

    // Toggle the edit section visibility
    if (
      editSection.style.display === 'none' ||
      editSection.style.display === ''
    ) {
      editSection.style.display = 'block';
      // Scroll to the edit section
      editSection.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    } else {
      editSection.style.display = 'none';
    }
  });
});

// Save note button click
document.querySelectorAll('.save-note-btn').forEach((btn) => {
  btn.addEventListener('click', async function () {
    const assignmentId = this.dataset.assignmentId;
    const noteText = document.getElementById(
      `notes-textarea-${assignmentId}`
    ).value;

    try {
      const response = await fetch(`/Teacher/Wird/UpdateWirdNote`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ Id: assignmentId, Note: noteText }),
      }).then((res) => res.json());

      if (response.success) {
        const card = btn.closest('.wird-card');

        // Add success effect with border color change
        card.classList.add('updated');
        const originalBorderColor = card.style.borderLeftColor;
        card.style.borderLeftColor = '#28a745'; // Green for success

        setTimeout(() => {
          card.classList.remove('updated');
          card.style.borderLeftColor = originalBorderColor;
        }, 1500);

        // Hide edit section, show note display
        document.getElementById(`notes-edit-${assignmentId}`).style.display =
          'none';

        // Update the note display in the wird-note section
        const wirdNoteSection = card.querySelector('.wird-note');
        if (noteText.trim()) {
          if (wirdNoteSection) {
            wirdNoteSection.querySelector(
              'p'
            ).textContent = `Note: ${noteText}`;
          } else {
            // Create note section if it doesn't exist
            const wirdFooter = card.querySelector('.wird-footer');
            const noteHtml = `
              <div class="wird-note">
                <i class='bx bx-note'></i>
                <p>Note: ${noteText}</p>
              </div>
            `;
            wirdFooter.insertAdjacentHTML('beforebegin', noteHtml);
          }
        } else {
          // Remove note section if note is empty
          if (wirdNoteSection) {
            wirdNoteSection.remove();
          }
        }
      } else {
        const card = btn.closest('.wird-card');

        // Add failure effect with border color change
        card.classList.add('failed-to-update');
        const originalBorderColor = card.style.borderLeftColor;
        card.style.borderLeftColor = '#dc3545'; // Red for failure

        setTimeout(() => {
          card.classList.remove('failed-to-update');
          card.style.borderLeftColor = originalBorderColor;
        }, 1500);
      }
    } catch (error) {
      console.error('Error saving note:', error);
      alert('An error occurred. Please try again.');
    }
  });
});

//updating wird status
document.querySelectorAll('.status-button').forEach((btn) => {
  btn.addEventListener('click', async () => {
    let id = btn.getAttribute('data-id');
    let status = btn.getAttribute('data-status');
    console.log(id + ' ' + status);
    await fetch('/Teacher/Wird/UpdateStatus', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ Id: id, Status: status }),
    })
      .then((res) => res.json())
      .then((data) => {
        if (data.success) {
          const card = btn.closest('.wird-card');
          let cardStatus = card.querySelector('.wird-status-badge');
          cardStatus.innerText = status;
          const currentStyleClass = cardStatus.classList.item(1);

          cardStatus.classList.replace(
            currentStyleClass,
            `status-${status.toLocaleLowerCase()}`
          );

          // Update card border color based on status
          const statusColors = {
            excellent: '#28a745',
            verygood: '#17a2b8',
            good: '#fd7e14',
            fair: '#ffc107',
            poor: '#dc3545',
            notset: '#d9d9d9',
          };

          const statusKey = status.toString().toLowerCase().replace(' ', '');
          const borderColor = statusColors[statusKey] || '#34654d';

          // Add success effect
          card.classList.add('updated');
          card.style.borderLeftColor = '#28a745'; // Green flash

          setTimeout(() => {
            card.classList.remove('updated');
            card.style.borderLeftColor = borderColor; // Then set to status color
          }, 1000);
        } else {
          const card = btn.closest('.wird-card');

          // Add failure effect
          card.classList.add('failed-to-update');
          const originalBorderColor = card.style.borderLeftColor;
          card.style.borderLeftColor = '#dc3545'; // Red for failure

          setTimeout(() => {
            card.classList.remove('failed-to-update');
            card.style.borderLeftColor = originalBorderColor;
          }, 1000);
        }
      });
  });
});
// Cancel note button click
document.querySelectorAll('.cancel-note-btn').forEach((btn) => {
  btn.addEventListener('click', function () {
    const assignmentId = this.dataset.assignmentId;
    document.getElementById(`notes-edit-${assignmentId}`).style.display =
      'none';
  });
});
