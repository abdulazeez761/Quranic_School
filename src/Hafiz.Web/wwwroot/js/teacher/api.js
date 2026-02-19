async function saveAttendance(TeacherId, Status, Hours = 0) {
  const attendanceDate = document.getElementById('attendanceDate').value;
  const ClassID = document.getElementById('classSession').value;

  const data = {
    TeacherId,
    Date: attendanceDate,
    ClassID,
    Status: isNaN(parseInt(Status)) ? 4 : parseInt(Status),
    Hours,
  };

  // Validation
  if (Hours < 0) {
    return showSaveIndicator('✗ يجب تحديد رقم أكبر من 0', 'error');
  }

  if (!ClassID) {
    return showSaveIndicator('✗ يجب اختيار الحلقة أولاً', 'error');
  }

  try {
    const response = await fetch('TeacherAttendance/SaveAttendance', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });

    if (response.ok) {
      showSaveIndicator('✓ تم الحفظ بنجاح', 'success');
      updateStatistics();
    } else {
      showSaveIndicator('✗ فشل الحفظ', 'error');
    }
  } catch (error) {
    console.error('Error:', error);
    showSaveIndicator('✗ حدث خطأ', 'error');
  }
}

async function getTeachers(classID) {
  if (!classID) {
    teacherTable.innerHTML = `
      <tr>
        <td colspan="7">
          <div class="table-empty">
            <i class='bx bx-info-circle'></i>
            <h3 class="table-empty-title">الرجاء اختيار حلقة</h3>
          </div>
        </td>
      </tr>
    `;
    return;
  }

  // Show loading
  teacherTable.innerHTML = `
    <tr>
      <td colspan="7" style="text-align:center; padding:40px;">
        <div class="spinner spinner-lg" style="margin: 0 auto;"></div>
        <p class="text-medium mt-md">جارٍ التحميل...</p>
      </td>
    </tr>
  `;

  try {
    const url = 'TeacherAttendance/GetTeachersByClass';
    const res = await fetch(
      `${url}?classId=${classID}&date=${attendanceDateForSelection}`
    );

    const teachers = await res.json();
    console.log('Fetched teachers:', teachers);

    if (!teachers || !teachers.length) {
      teacherTable.innerHTML = `
        <tr>
          <td colspan="7">
            <div class="table-empty">
              <i class='bx bx-user-x'></i>
              <h3 class="table-empty-title">لا توجد بيانات معلمين</h3>
              <p class="text-medium">لا يوجد معلمين في هذه الحلقة</p>
            </div>
          </td>
        </tr>
      `;
      document.getElementById('totalTeachers').textContent = '0';
      updateStatistics();
      return;
    }

    // Update total count
    document.getElementById('totalTeachers').textContent = teachers.length;

    // Build table rows
    teacherTable.innerHTML = teachers
      .map((t, index) => {
        let presentChecked = '';
        let lateChecked = '';
        let absentChecked = '';
        let excuseChecked = '';
        let hoursValue = 0;
        let rowClass = '';

        if (t.prevAttendance) {
          const status = t.prevAttendance.status;
          hoursValue = t.prevAttendance.workingHours ?? 0;
          rowClass = 'has-attendance';

          switch (status) {
            case 0:
              presentChecked = 'checked';
              rowClass += ' status-present';
              break;
            case 1:
              lateChecked = 'checked';
              rowClass += ' status-late';
              break;
            case 2:
              absentChecked = 'checked';
              rowClass += ' status-absent';
              break;
            case 3:
              excuseChecked = 'checked';
              rowClass += ' status-excuse';
              break;
          }
        }

        const initial = t.firstName ? t.firstName.charAt(0) : 'م';
        const hoursDisplay =
          hoursValue > 0
            ? `<div class="fs-xs text-medium">${hoursValue} ساعة</div>`
            : '';

        return `
          <tr data-teacher-id="${t.id}" class="${rowClass}">
            <td class="text-center text-medium">${index + 1}</td>
            <td>
              <div class="d-flex align-center gap-sm">
                <div class="avatar-sm" style="background: var(--secondary-green); color: white; display: flex; align-items: center; justify-content: center; border-radius: 50%;">
                  ${initial}
                </div>
                <div>
                  <div class="fw-medium">${t.firstName} ${t.secondName}</div>
                  ${hoursDisplay}
                </div>
              </div>
            </td>
            <td class="text-center">
              <label class="attendance-radio">
                <input type="radio" 
                       name="attendance_${t.id}" 
                       class="attendance-checkbox status-present" 
                       data-teacher-id="${t.id}"
                       data-status="0" 
                       ${presentChecked}>
                <span class="radio-custom present"></span>
              </label>
            </td>
            <td class="text-center">
              <label class="attendance-radio">
                <input type="radio" 
                       name="attendance_${t.id}" 
                       class="attendance-checkbox status-late" 
                       data-teacher-id="${t.id}"
                       data-status="1" 
                       ${lateChecked}>
                <span class="radio-custom late"></span>
              </label>
            </td>
            <td class="text-center">
              <label class="attendance-radio">
                <input type="radio" 
                       name="attendance_${t.id}" 
                       class="attendance-checkbox status-absent" 
                       data-teacher-id="${t.id}"
                       data-status="2" 
                       ${absentChecked}>
                <span class="radio-custom absent"></span>
              </label>
            </td>
            <td class="text-center">
              <label class="attendance-radio">
                <input type="radio" 
                       name="attendance_${t.id}" 
                       class="attendance-checkbox status-excuse" 
                       data-teacher-id="${t.id}"
                       data-status="3" 
                       ${excuseChecked}>
                <span class="radio-custom excuse"></span>
              </label>
            </td>
            <td>
              <div class="input-group">
                <input type="number" 
                       class="form-input form-input-sm hours-input" 
                       data-teacher-id="${t.id}" 
                       data-hours="${hoursValue}"
                       min="0" 
                       max="24" 
                       step="0.5" 
                       value="${hoursValue}" 
                       placeholder="0"
                       style="text-align: center;">
                <span class="input-group-text">
                  <i class='bx bx-time'></i>
                </span>
              </div>
            </td>
          </tr>
        `;
      })
      .join('');

    // Reattach event listeners
    attachEventListeners();

    // Update statistics
    updateStatistics();
  } catch (err) {
    console.error('Error fetching teachers:', err);
    showSaveIndicator('✗ حدث خطأ أثناء جلب بيانات المعلمين', 'error');
    teacherTable.innerHTML = `
      <tr>
        <td colspan="7">
          <div class="table-empty">
            <i class='bx bx-error-circle' style="color: var(--danger);"></i>
            <h3 class="table-empty-title" style="color: var(--danger);">حدث خطأ</h3>
            <p class="text-medium">تعذر جلب بيانات المعلمين</p>
            <button class="btn btn-primary mt-md" onclick="getTeachers(classSession.value)">
              <i class='bx bx-refresh'></i> إعادة المحاولة
            </button>
          </div>
        </td>
      </tr>
    `;
  }
}
