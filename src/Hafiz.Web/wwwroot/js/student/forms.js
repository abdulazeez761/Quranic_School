// Student Form Shared JavaScript
$(document).ready(function () {
  // Initialize Select2 for classes dropdown
  $('.select2-classes').select2({
    placeholder: 'اختر الحلقات...',
    allowClear: true,
    width: '100%',
    language: 'ar',
    dir: 'rtl',
  });

  // Form submission with loading state
  $('#studentForm').on('submit', function () {
    const submitBtn = document.getElementById('submitBtn');
    if (typeof AdminDashboard !== 'undefined') {
      AdminDashboard.setLoadingState(submitBtn, true);
    }
  });

  // Password strength indicator
  $('#passwordInput').on('input', function () {
    const password = $(this).val();
    const strength = calculatePasswordStrength(password);
    updatePasswordStrength(strength);
  });

  // Confirm password match validation
  $('#confirmPasswordInput').on('input', function () {
    const password = $('#passwordInput').val();
    const confirm = $(this).val();

    if (confirm && password !== confirm) {
      $(this).addClass('is-invalid');
      $(this)
        .siblings('.form-feedback-invalid')
        .text('كلمات المرور غير متطابقة');
    } else {
      $(this).removeClass('is-invalid');
    }
  });

  // Phone number formatting
  $('input[name="PhoneNumber"]').on('input', function () {
    let value = $(this).val().replace(/\D/g, ''); // Remove non-digits
    if (value.startsWith('962')) {
      value = value.substring(3); // Remove country code if entered
    }
    if (value.startsWith('0')) {
      value = value.substring(1); // Remove leading 0
    }
    $(this).val(value);
  });
});

// Toggle password visibility
function togglePassword(inputId, button) {
  const input = document.getElementById(inputId);
  const icon = button.querySelector('i');

  if (input.type === 'password') {
    input.type = 'text';
    icon.className = 'bx bx-hide';
  } else {
    input.type = 'password';
    icon.className = 'bx bx-show';
  }
}

// Password strength calculator
function calculatePasswordStrength(password) {
  if (password.length === 0) return 0;
  if (password.length < 6) return 1; // Weak

  let strength = 0;
  if (password.length >= 8) strength++;
  if (/[a-z]/.test(password) && /[A-Z]/.test(password)) strength++;
  if (/\d/.test(password)) strength++;
  if (/[^a-zA-Z\d]/.test(password)) strength++;

  return Math.min(strength, 3);
}

function updatePasswordStrength(strength) {
  const container = document.querySelector('.password-strength');
  if (!container) {
    // Create strength indicator if doesn't exist
    const indicator = `
            <div class="password-strength">
                <div class="password-strength-bar"></div>
            </div>
        `;
    $('#passwordInput').after(indicator);
  }

  const bar = document.querySelector('.password-strength-bar');
  if (bar) {
    bar.className = 'password-strength-bar';
    if (strength === 1) bar.classList.add('password-strength-weak');
    else if (strength === 2) bar.classList.add('password-strength-medium');
    else if (strength === 3) bar.classList.add('password-strength-strong');
  }
}
