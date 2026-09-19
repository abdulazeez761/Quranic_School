/**
 * ============================================================================
 * Wird Drawer - Stepper Navigation, Tab Filtering & Keyboard Controller
 * ============================================================================
 * Handles sequential student stepping (prev/next), category tab filtering,
 * and keyboard shortcuts (Escape, Ctrl+Enter, Ctrl+S).
 * ============================================================================
 */

async function prevStudent() {
  const prevBtn = document.getElementById('drawerPrevBtn');
  if (prevBtn?.disabled) return;

  if (window.selectedStudentIndex > 0) {
    if (typeof saveDrawerWirds === 'function') {
      await saveDrawerWirds(false);
    }
    window.selectedStudentIndex--;
    if (typeof loadStudentIntoDrawer === 'function') {
      loadStudentIntoDrawer(window.selectedStudentIndex);
    }
  } else if (typeof showDrawerToast === 'function') {
    showDrawerToast('أنت في بداية قائمة طلاب الحلقة', 'info');
  }
}

async function saveAndNextStudent() {
  const nextBtn = document.getElementById('drawerSaveNextBtn');
  const saveBtn = document.getElementById('drawerSaveBtn');
  const prevBtn = document.getElementById('drawerPrevBtn');

  // Prevent double-click race condition
  if (nextBtn?.disabled) return;

  // Set loading state and lock buttons
  if (nextBtn) {
    nextBtn.disabled = true;
    nextBtn.dataset.originalHtml = nextBtn.innerHTML;
    nextBtn.innerHTML = "<i class='bx bx-loader-alt bx-spin'></i> جاري الحفظ...";
  }
  if (saveBtn) saveBtn.disabled = true;
  if (prevBtn) prevBtn.disabled = true;

  try {
    let success = true;
    if (typeof saveDrawerWirds === 'function') {
      success = await saveDrawerWirds(false);
    }

    if (success) {
      if (window.selectedStudentIndex < window.classStudents.length - 1) {
        window.selectedStudentIndex++;
        if (typeof loadStudentIntoDrawer === 'function') {
          loadStudentIntoDrawer(window.selectedStudentIndex);
        }
        const body = document.getElementById('drawerWirdsList');
        if (body) body.scrollTop = 0;
      } else {
        if (typeof closeWirdDrawer === 'function') {
          closeWirdDrawer();
        }
        if (typeof Swal !== 'undefined') {
          Swal.fire({
            icon: 'success',
            title: '🎉 اكتملت الحلقة بنجاح!',
            text: `تم حفظ وتقييم أوراد جميع طلاب الحلقة (${window.classStudents.length} طالباً) بنجاح تام.`,
            confirmButtonText: 'ممتاز',
            confirmButtonColor: '#059669',
          }).then(() => {
            window.location.reload();
          });
        } else {
          setTimeout(() => { window.location.reload(); }, 1000);
        }
      }
    }
  } catch (err) {
    console.error('Error during saveAndNextStudent:', err);
    if (typeof showDrawerToast === 'function') {
      showDrawerToast('تعذر إكمال الحفظ، يرجى إعادة المحاولة.', 'error');
    }
  } finally {
    if (nextBtn) {
      nextBtn.disabled = false;
      nextBtn.innerHTML = nextBtn.dataset.originalHtml || '<span id="drawerNextBtnText">حفظ والتالي</span> <i class=\'bx bx-chevron-left\'></i>';
    }
    if (saveBtn) saveBtn.disabled = false;
    if (prevBtn) {
      prevBtn.disabled = window.selectedStudentIndex === 0;
    }
  }
}

function setWirdFilterTab(tabKey, btn) {
  window.currentFilterTab = tabKey;
  document
    .querySelectorAll('.segmented-nav .segmented-btn')
    .forEach((b) => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const boxes = document.querySelectorAll('#drawerWirdsList .drawer-wird-box');
  boxes.forEach((box) => {
    const bType = box.dataset.type;
    const visible = tabKey === 'all' || tabKey === bType;
    box.style.display = visible ? 'block' : 'none';
  });
}

function setupDrawerKeyboardEvents() {
  document.addEventListener('keydown', async (e) => {
    const drawer = document.getElementById('drawerPanel');
    const isOpen =
      drawer &&
      (drawer.classList.contains('active') ||
        drawer.classList.contains('mobile-open') ||
        drawer.classList.contains('is-open'));

    if (!isOpen) return;

    if (e.key === 'Escape') {
      if (typeof closeWirdDrawer === 'function') {
        closeWirdDrawer();
      }
    } else if (e.ctrlKey && e.key === 'Enter') {
      e.preventDefault();
      await saveAndNextStudent();
    } else if (e.ctrlKey && (e.key === 's' || e.key === 'S' || e.key === 'س')) {
      e.preventDefault();
      if (typeof saveDrawerWirds === 'function') {
        await saveDrawerWirds(false);
      }
    }
  });
}
