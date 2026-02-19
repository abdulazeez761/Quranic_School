async function saveAttendance(studentId, Status) {
    const attendanceDate = document.getElementById('attendanceDate').value;
    const ClassID = document.getElementById('classSession').value;

    const data = {
        studentId,
        Date: attendanceDate,
        ClassID,
        Status: isNaN(parseInt(Status)) ? 4 : parseInt(Status),
    };
   
    try {
        const response = await fetch('StudentAttendance/SaveAttendance', {
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify(data),
        });

        if (response.ok) {
            {
                showSaveIndicator('✓ تم الحفظ بنجاح', 'success');
                updateStatistics();
            }
        } else showSaveIndicator('✗ فشل الحفظ', 'error');
    } catch (error) {
        console.error('Error:', error);
        showSaveIndicator('✗ حدث خطأ', 'error');
    }
}

async function getstudents(classID) {
    try {
        const url = 'StudentAttendance/GetStudentsByClassId';
        const res = await fetch(
            `${url}?classId=${classID}&date=${attendanceDateForSelection}`
        );

        const students = await res.json();
        if (!students.length) {
            studentTable.innerHTML = `
                    <tr>
                        <td colspan="6" style="text-align:center; padding:30px; color:var(--text-light);">لا توجد بيانات طلاب</td>
                    </tr>
                `;
            return;
        }
        // Build table rows dynamically
        studentTable.innerHTML = students
            .map((t, i) => {
                let presentChecked = t.prevAttendance?.status == 0 ? 'checked' : '';
                let lateChecked = t.prevAttendance?.status == 1 ? 'checked' : '';
                let absentChecked = t.prevAttendance?.status == 2 ? 'checked' : '';
                let excuseChecked = t.prevAttendance?.status == 3 ? 'checked' : '';

                let rowClass = '';
                if (t.prevAttendance) {
                    rowClass = 'has-attendance status-';
                    switch (t.prevAttendance.status) {
                        case 0:
                            rowClass += 'present';
                            break;
                        case 1:
                            rowClass += 'late';
                            break;
                        case 2:
                            rowClass += 'absent';
                            break;
                        case 3:
                            rowClass += 'excuse';
                            break;
                    }
                }

                return `
        <tr data-student-id="${t.id}" class="${rowClass}">
            <td class="text-center text-medium">${i + 1}</td>
            <td>
                <div class="d-flex align-center gap-sm">
                    <div class="avatar-sm bg-primary text-white d-flex align-center justify-center rounded-full">
                        ${t.firstName.charAt(0)}
                    </div>
                    <span class="fw-medium">${t.firstName} ${
                    t.secondName
                }</span>
                </div>
            </td>
            <td class="text-center">
                <label class="attendance-radio">
                    <input type="radio" name="attendance_${
                    t.id
                }" class="attendance-checkbox status-present"
                    data-student-id="${t.id}" data-status="0" ${presentChecked}>
                    <span class="radio-custom present"></span>
                </label>
            </td>
            <td class="text-center">
                <label class="attendance-radio">
                    <input type="radio" name="attendance_${
                    t.id
                }" class="attendance-checkbox status-late"
                    data-student-id="${t.id}" data-status="1" ${lateChecked}>
                    <span class="radio-custom late"></span>
                </label>
            </td>
            <td class="text-center">
                <label class="attendance-radio">
                    <input type="radio" name="attendance_${
                    t.id
                }" class="attendance-checkbox status-absent"
                    data-student-id="${t.id}" data-status="2" ${absentChecked}>
                    <span class="radio-custom absent"></span>
                </label>
            </td>
            <td class="text-center">
                <label class="attendance-radio">
                    <input type="radio" name="attendance_${
                    t.id
                }" class="attendance-checkbox status-excuse"
                    data-student-id="${t.id}" data-status="3" ${excuseChecked}>
                    <span class="radio-custom excuse"></span>
                </label>
            </td>
        </tr>
    `;
            })
            .join('');

        attachEventListeners(); // reattach listeners
    } catch (err) {
        console.error(err);
        showSaveIndicator('✗ حدث خطا اثناء جلب بيانات المعلمين ', 'error');
        studentTable.innerHTML = `
                    <tr>
                        <td colspan="6" style="text-align:center; padding:30px; color:var(--text-light);">✗ حدث خطا اثناء جلب بيانات المعلمين </td>
                    </tr>
                `;
    }
}