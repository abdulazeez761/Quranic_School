// Student Layout JavaScript
document.addEventListener('DOMContentLoaded', function() {
    // Add any student-specific JavaScript functionality here
    console.log('Student portal loaded successfully');
    
    // Add active class animation
    const navLinks = document.querySelectorAll('.nav-tabs a');
    navLinks.forEach(link => {
        link.addEventListener('click', function() {
            // Remove active from all
            navLinks.forEach(l => l.parentElement.classList.remove('active'));
            // Add active to clicked
            this.parentElement.classList.add('active');
        });
    });
});
