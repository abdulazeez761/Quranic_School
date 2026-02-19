function attachEventListeners() {
    const checkboxes = document.querySelectorAll('.attendance-checkbox');
    checkboxes.forEach((cb) => {
        cb.addEventListener('change', function () {
            
            const row = this.closest('tr');
            const studentId = this.dataset.studentId;
            const status = this.dataset.status;

            const rowCheckboxes = row.querySelectorAll('.attendance-checkbox');
            row.className = 'has-attendance';
            const statusClass = this.classList.contains('status-present')
                ? 'status-present'
                : this.classList.contains('status-late')
                    ? 'status-late'
                    : this.classList.contains('status-absent')
                        ? 'status-absent'
                        : 'status-excuse';

            if (this.checked) {
                row.classList.add(statusClass);

                saveAttendance(studentId, status);
            } else {
                saveAttendance(studentId, 'none');
                row.classList.remove('has-prev');
            }
        });
    });
    updateStatistics();
}

// For fetching students depending on class id
$('#classSession').on('select2:select', async function (e) {
    const classId = e.params.data.id; // or $(this).val()
    studentTable.innerHTML = `
            <tr>
                <td colspan="6" style="text-align:center; padding:20px;">جارٍ التحميل...</td>
            </tr>
        `;
    getstudents(classId);
});

let attDate = document.getElementById('attendanceDate');
attDate.addEventListener('change', (e) => {
    attendanceDateForSelection = e.target.value;
    studentTable.innerHTML = `
            <tr>
                <td colspan="6" style="text-align:center; padding:20px;">جارٍ التحميل...</td>
            </tr>
        `;
    getstudents(classSession.value);
});