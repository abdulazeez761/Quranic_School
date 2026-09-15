using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Wird;
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

        Task<(bool IsSuccess, string Message)> AddWirdAsync(AssignWirdsBatchDto wird);
        Task<(bool IsSuccess, string Message)> UpdateWirdAsync(
            WirdAssignment wird,
            Guid? instituteId = null
        );
        Task<List<WirdAssignment>?> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            string? fromDate,
            string? toDate
        );
        Task<bool> UpdateStatus(Guid id, AssignmentStatus status, Guid? instituteId = null);
        Task<bool> UpdateWirdNote(Guid Id, string Note, Guid? instituteId = null);
        Task<bool> DeleteWirdAssignment(Guid id, Guid? instituteId = null);
        Task<WirdAssignment?> GetWirdAssignmentByIdAsync(Guid id, Guid? instituteId = null);
    }
}
