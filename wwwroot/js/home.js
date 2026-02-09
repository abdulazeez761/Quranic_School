// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach((anchor) => {
  anchor.addEventListener('click', function (e) {
    e.preventDefault();
    const target = document.querySelector(this.getAttribute('href'));
    if (target) {
      target.scrollIntoView({
        behavior: 'smooth',
        block: 'start',
      });
    }
  });
});

// Video loading function
function loadVideo() {
  const videoPlaceholder = document.querySelector('.video-placeholder');
  const video = document.getElementById('testimonial-video');

  // Replace with actual video URL
  video.src = 'https://www.youtube.com/embed/YOUR_VIDEO_ID';
  video.style.display = 'block';
  videoPlaceholder.style.display = 'none';
}

// Scroll animations
window.addEventListener('scroll', () => {
  const scrolled = window.pageYOffset;
  const parallax = document.querySelector('.hero-section');
  const speed = scrolled * 0.5;

  if (parallax) {
    parallax.style.transform = `translateY(${speed}px)`;
  }
});

// Counter animation for stats
function animateCounter(element, target) {
  let current = 0;
  const increment = target / 50;
  const timer = setInterval(() => {
    current += increment;
    element.textContent = Math.floor(current);
    if (current >= target) {
      element.textContent = target;
      clearInterval(timer);
    }
  }, 30);
}

// Intersection Observer for animations
const observer = new IntersectionObserver((entries) => {
  entries.forEach((entry) => {
    if (entry.isIntersecting) {
      entry.target.classList.add('animate-in');

      // Animate counters
      if (entry.target.classList.contains('stat-number')) {
        const target = parseInt(entry.target.textContent.replace(/\D/g, ''));
        animateCounter(entry.target, target);
      }
    }
  });
});

// Observe elements for animation
document
  .querySelectorAll('.feature-card, .stat-card, .achievement-card')
  .forEach((el) => {
    observer.observe(el);
  });
