var classes = document.querySelectorAll('.class-card');

const savedClassId = getCookie('selectedClassId');
if (savedClassId) {
  classes.forEach((c) => {
    if (c.dataset.classId === savedClassId) {
      c.classList.add('active');
      c.querySelector('.status-badge').style.display = 'block';
    }
  });
}

classes.forEach((card) => {
  card.addEventListener('click', (event) => {
    const classId = event.currentTarget.dataset.classId;
    const className = card.getAttribute('data-class-name');
    document.cookie = `selectedClassId=${classId}; path=/; max-age=${
      60 * 60 * 24 * 30
    }`;

    document.cookie = `selectedClassName=${className}; path=/; max-age=${
      60 * 60 * 24 * 30
    }`;

    window.location.href = '/Teacher/Student';
  });
});

function getCookie(name) {
  return document.cookie
    .split('; ')
    .find((row) => row.startsWith(name + '='))
    ?.split('=')[1];
}
