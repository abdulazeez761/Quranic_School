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
        /// صفحة واحدة من أسطر تقرير الأوراد (مع بيانات الطالب وشُعبه)، مُرقّمة على مستوى
        /// قاعدة البيانات وفق Page/PageSize في التصفية ومرتّبة بالأحدث.
        /// </summary>
        Task<List<WirdAssignment>> GetWirdReportDetailsPageAsync(WirdReportFilterDto filter);

        /// <summary>
        /// كل أسطر تقرير الأوراد المطابقة للتصفية (بدون ترقيم) — للتصدير إلى Excel.
        /// </summary>
        Task<List<WirdAssignment>> GetWirdReportDetailsAsync(WirdReportFilterDto filter);

        /// <summary>
        /// إحصائيات التقرير محسوبة داخل قاعدة البيانات (عدّ ومجاميع) دون جلب الأسطر.
        /// </summary>
        Task<(
            int Total,
            int Completed,
            int Upcoming,
            decimal TotalPages,
            decimal CompletedPages
        )> GetWirdReportAggregatesAsync(WirdReportFilterDto filter);

        /// <summary>
        /// إسقاط خفيف بالحقول اللازمة لحساب ترتيب الطلاب فقط (بدون تحميل الكيانات الكاملة).
        /// </summary>
        Task<List<WirdRankingSourceRow>> GetWirdReportRankingSourceAsync(
            WirdReportFilterDto filter
        );

        Task<bool> AddWirdAsync(WirdAssignment wird);
        Task<(bool IsSuccess, string Message)> AddWirdsBatchAtomicAsync(
            List<WirdAssignment> wirds,
            Guid studentId,
            decimal totalMemDelta,
            decimal totalRevDelta
        );
        Task<(
            WirdAssignment? memorization,
            WirdAssignment? recentRevision,
            WirdAssignment? oldRevision,
            WirdAssignment? recitation
        )> GetLatestWirdsForContextAsync(Guid studentId, DateTime todayDate);
        Task<List<WirdAssignment>> GetTodayWirdsAsync(Guid studentId, DateTime todayDate);
        Task<bool> UpdateWirdAsync(WirdAssignment wird, Guid? instituteId = null);
        Task<bool> UpdateWirdWithProgressDeltaAsync(
            WirdAssignment wird,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        );
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
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status, Guid? instituteId = null);
        Task<bool> UpdateStatusWithProgressDeltaAsync(
            Guid id,
            AssignmentStatus status,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        );
        Task<WirdAssignment?> GetWirdByID(Guid Id, Guid? instituteId = null);
        Task<bool> UpdateNote(Guid Id, string note, Guid? instituteId = null);
        Task<bool> DeleteWirdAssignment(Guid id, Guid? instituteId = null);
        Task<bool> DeleteWirdWithProgressDeltaAsync(
            Guid id,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        );
    }
}
