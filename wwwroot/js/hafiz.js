const videoData = [
  {
    id: 'video1',
    title: "Ahmad's Hifz Journey",
    description: 'From beginner to completing full Quran memorization',
    category: 'hifz',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
  {
    id: 'video2',
    title: "Teacher Fatima's Experience",
    description: 'How our teaching methods transformed students',
    category: 'teacher',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
  {
    id: 'video3',
    title: 'Parent Testimonial',
    description: "A mother shares her child's progress story",
    category: 'parent',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
  {
    id: 'video4',
    title: "Aisha's Success Story",
    description: 'Completing Hifz while excelling in academics',
    category: 'hifz',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
  {
    id: 'video5',
    title: "Ustadh Ibrahim's Method",
    description: 'Innovative teaching techniques for Quran memorization',
    category: 'teacher',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
  {
    id: 'video6',
    title: 'Family Transformation',
    description: 'How Hifz changed our entire family dynamic',
    category: 'parent',
    youtubeId: 'dQw4w9WgXcQ', // Replace with actual YouTube video ID
    thumbnail: null,
  },
];

// Create video HTML element
function createVideoElement(video, tabPrefix = '') {
  const uniqueId = tabPrefix ? `${tabPrefix}-${video.id}` : video.id;
  return `
                <div class="video-container" data-category="${video.category}" id="container-${uniqueId}">
                    <div class="video-placeholder" id="placeholder-${uniqueId}" onclick="loadVideo('${uniqueId}', '${video.youtubeId}')">
                        <i class="fas fa-play-circle"></i>
                        <h4>${video.title}</h4>
                        <p>${video.description}</p>
                    </div>
                    <iframe id="iframe-${uniqueId}" class="video-iframe" style="display: none;" 
                            src="" frameborder="0" allowfullscreen></iframe>
                </div>
            `;
}

// Load video function
function loadVideo(uniqueId, youtubeId) {
  const placeholder = document.getElementById(`placeholder-${uniqueId}`);
  const videoElement = document.getElementById(`iframe-${uniqueId}`);

  if (placeholder && videoElement) {
    // Hide placeholder and show video
    placeholder.style.display = 'none';
    videoElement.style.display = 'block';

    // Set video source (YouTube embed)
    videoElement.src = `https://www.youtube.com/embed/${youtubeId}?autoplay=1`;
  }
}

// Populate video grids
function populateVideoGrids() {
  const allGrid = document.getElementById('allVideosGrid');
  const hifzGrid = document.getElementById('hifzVideosGrid');
  const teacherGrid = document.getElementById('teacherVideosGrid');
  const parentGrid = document.getElementById('parentVideosGrid');

  // Clear existing content
  allGrid.innerHTML = '';
  hifzGrid.innerHTML = '';
  teacherGrid.innerHTML = '';
  parentGrid.innerHTML = '';

  // Populate grids
  videoData.forEach((video) => {
    // For all videos tab
    const allVideoHTML = createVideoElement(video, `all-${video.category}`);
    allGrid.innerHTML += allVideoHTML;

    // For category-specific tabs
    const categoryVideoHTML = createVideoElement(video, video.category);

    // Add to category-specific grids
    switch (video.category) {
      case 'hifz':
        hifzGrid.innerHTML += categoryVideoHTML;
        break;
      case 'teacher':
        teacherGrid.innerHTML += categoryVideoHTML;
        break;
      case 'parent':
        parentGrid.innerHTML += categoryVideoHTML;
        break;
    }
  });
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', function () {
  populateVideoGrids();
});

// Optional: Add function to dynamically add more videos
function addVideo(videoObj) {
  videoData.push(videoObj);
  populateVideoGrids();
}

// Optional: Filter videos by search
function filterVideos(searchTerm) {
  const videoContainers = document.querySelectorAll('.video-container');
  videoContainers.forEach((container) => {
    const title = container.querySelector('h4').textContent.toLowerCase();
    const description = container.querySelector('p').textContent.toLowerCase();

    if (
      title.includes(searchTerm.toLowerCase()) ||
      description.includes(searchTerm.toLowerCase())
    ) {
      container.style.display = 'block';
    } else {
      container.style.display = 'none';
    }
  });
}

function getCookie(name) {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) return parts.pop().split(';').shift();
}

const rawCultureCookie = getCookie('.AspNetCore.Culture');
let language = '';
if (rawCultureCookie) {
  // Decode the URL-encoded string
  const decoded = decodeURIComponent(rawCultureCookie); // => "c=ar|uic=ar"
  console.log('Raw Culture Cookie:', rawCultureCookie); // ".AspNetCore.Culture=c=ar|uic=ar"
  console.log('Decoded Culture Cookie:', decoded); // "c=ar|uic=ar"
  // Split it to extract values
  const parts = decoded.split('|'); // ["c=ar", "uic=ar"]
  const culture = parts.find((p) => p.startsWith('c=')).split('=')[1];
  const uiCulture = parts.find((p) => p.startsWith('uic=')).split('=')[1];

  console.log('Culture:', culture);
  console.log('UI Culture:', uiCulture);
  language = culture; // Assuming you want to use the culture value
} else language = 'ar'; // Default to English if cookie is not set
