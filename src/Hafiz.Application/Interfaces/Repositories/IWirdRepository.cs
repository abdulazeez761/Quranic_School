using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs.Reports;
using Hafiz.Models;

namespace Hafiz.Repositories.Interfaces
{
    public interface IWirdRepository
    {
        /// <summary>
        /// يجلب الأوراد المطابقة لمعايير التقرير (مع بيانات الطالب وشُعبه) بعد تطبيق
        /// التصفية على مستوى قاعدة البيانات. التجميع والحساب يتمّان في طبقة الخدمة.
        /// </summary>
        Task<List<WirdAssignment>> GetWirdReportDataAsync(WirdReportFilterDto filter);

        Task<bool> AddWirdAsync(WirdAssignment wird);
        Task<bool> UpdateWirdAsync(WirdAssignment wird);
        Task<List<WirdAssignment>> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            DateTime from,
            DateTime to
        );
        Task<List<WirdAssignment>> GetWirdAssignmentsByStudentIdAsync(Guid studentID);
        Task<(
            List<WirdAssignment> wirds,
            int totalCount,
            int completedCount,
            int pendingCount,
            int upcomingCount
        )> GetWirdAssignmentsByStudentIdPaginatedAsync(
            Guid studentID,
            int pageNumber,
            int pageSize,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null,
            bool? isUpcoming = null
        );
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status);
        Task<WirdAssignment?> GetWirdByID(Guid Id);
        Task<bool> UpdateNote(Guid Id, string note);
        Task<bool> DeleteWirdAssignment(Guid id);
    }
}
