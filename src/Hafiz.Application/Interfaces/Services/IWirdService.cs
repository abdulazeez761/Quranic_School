using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.DTOs.Reports;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IWirdService
    {
        /// <summary>
        /// يبني تقرير الأوراد لصفحة العرض: الإحصائيات، ترتيب الطلاب، وصفحة واحدة من الأسطر
        /// التفصيلية مُرقّمة في قاعدة البيانات وفق Page/PageSize في التصفية.
        /// </summary>
        Task<WirdReportViewModel> GetWirdReportAsync(WirdReportFilterDto filter);

        /// <summary>
        /// يبني تقرير الأوراد للتصدير: نفس الإحصائيات مع كل الأسطر التفصيلية المطابقة (بدون ترقيم).
        /// </summary>
        Task<WirdReportViewModel> GetWirdReportForExportAsync(WirdReportFilterDto filter);

        Task<(bool IsSuccess, string Message)> AddWirdAsync(WirdAssignment wird);
        Task<(bool IsSuccess, string Message)> UpdateWirdAsync(WirdAssignment wird);
        Task<List<WirdAssignment>?> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            string? fromDate,
            string? toDate
        );
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status);
        Task<bool> UpdateWirdNote(Guid Id, string Note);
        Task<bool> DeleteWirdAssignment(Guid id);
        Task<WirdAssignment?> GetWirdAssignmentByIdAsync(Guid id);
    }
}
