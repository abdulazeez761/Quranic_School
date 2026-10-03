using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Domain.Enums;
using Hafiz.Web.Helpers;
using Hafiz.Web.Reporting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CertificatesManagementController : Controller
{
    private static readonly int[] AllowedPageSizes = { 25, 50, 100 };

    private static readonly HashSet<string> SortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "number", "student", "type", "template", "issued", "status"
    };

    private readonly ICertificateIssuanceService _certificates;

    public CertificatesManagementController(ICertificateIssuanceService certificates) =>
        _certificates = certificates;

    public async Task<IActionResult> Index(
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        string? sort = null,
        string? dir = null,
        int pageSize = 25
    )
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();

        var size = AllowedPageSizes.Contains(pageSize) ? pageSize : 25;
        var order = NormalizeSort(sort, dir);
        var currentPage = Math.Max(1, page);

        var total = await _certificates.GetHistoryCountAsync(instituteId, type: type, status: status, search: search);

        // Paging past the end would otherwise show an empty table with no way back.
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)size));
        if (currentPage > totalPages)
            currentPage = totalPages;

        var items = (await _certificates.GetHistoryAsync(
            instituteId, type: type, status: status, search: search,
            page: currentPage, pageSize: size, sort: order)).ToList();

        MapTimes(items);

        ViewBag.SelectedType = type;
        ViewBag.SelectedStatus = status;
        ViewBag.Search = search;
        ViewBag.Page = currentPage;
        ViewBag.PageSize = size;
        ViewBag.Sort = SortKey(order);
        ViewBag.Dir = order.EndsWith("_desc", StringComparison.Ordinal) ? "desc" : "asc";
        ViewBag.Total = total;
        ViewBag.TotalPages = totalPages;
        ViewBag.Stats = await _certificates.GetStatsAsync(
            instituteId, CurrentMonthStartUtc(), type: type, status: status, search: search);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id)
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();

        var audits = await _certificates.GetAuditTrailAsync(id, instituteId);
        if (audits is null)
            return Json(new { success = false, message = "الشهادة غير موجودة." });

        var userTimeZone = TimeZoneHelper.GetUserTimeZone(HttpContext);
        return Json(new
        {
            success = true,
            audits = audits.Select(a => new
            {
                action = a.ActionDisplayName,
                icon = a.ActionIcon,
                tone = a.ActionTone,
                details = a.Details,
                performedBy = a.PerformedByDisplay,
                performedAt = a.PerformedAtUserTime.ToUserTime(HttpContext).ToString("yyyy/MM/dd HH:mm"),
            })
        });
    }

    public async Task<IActionResult> Export(
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        string? sort = null,
        string? dir = null
    )
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();

        // Exports follow the on-screen filters, so the sheet matches what the admin is looking at.
        // The history query is paged and capped at 100, so walk the pages to reach the whole set.
        const int batch = 100;
        const int maxRows = 5000;
        var order = NormalizeSort(sort, dir);
        var items = new List<CertificateListItemDto>();

        for (var page = 1; items.Count < maxRows; page++)
        {
            var chunk = (await _certificates.GetHistoryAsync(
                instituteId, type: type, status: status, search: search,
                page: page, pageSize: batch, sort: order)).ToList();

            items.AddRange(chunk);
            if (chunk.Count < batch) break;
        }

        MapTimes(items);

        var bytes = CertificateReportExcelExporter.Build(items, BuildFilterSummary(type, status, search, items.Count));
        var fileName = $"certificates-{TimeZoneHelper.GetUserNow(HttpContext):yyyyMMdd-HHmm}.xlsx";
        return File(bytes, CertificateReportExcelExporter.ContentType, fileName);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(Guid id, string reason)
    {
        var instituteId = InstituteId();
        var userId = UserId();
        if (!instituteId.HasValue || !userId.HasValue)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "غير مصرح بهذا الإجراء." });
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            var msg = "يجب تحديد سبب إلغاء اعتماد الشهادة.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = msg });
            TempData["ErrorMessage"] = msg;
            return RedirectToAction(nameof(Index));
        }

        if (await _certificates.RevokeCertificateAsync(id, reason.Trim(), userId.Value, instituteId))
        {
            var msg = "تم إلغاء اعتماد الشهادة، ولم تعد سارية عند التحقق.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = msg });
            TempData["SuccessMessage"] = msg;
        }
        else
        {
            var msg = "تعذّر إلغاء اعتماد الشهادة؛ تأكد من أنها ما زالت سارية.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = msg });
            TempData["ErrorMessage"] = msg;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var instituteId = InstituteId();
        var userId = UserId();
        if (!instituteId.HasValue || !userId.HasValue)
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "غير مصرح بهذا الإجراء." });
            return Forbid();
        }

        var deleted = await _certificates.DeleteCertificateAsync(id, userId.Value, instituteId);
        if (deleted)
        {
            var msg = "تم حذف الشهادة وأرشفتها بنجاح.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = msg });
            TempData["SuccessMessage"] = msg;
        }
        else
        {
            var msg = "تعذّر حذف الشهادة؛ تأكد من صلاحياتك ووجود السجل.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = msg });
            TempData["ErrorMessage"] = msg;
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// The service hands back raw UTC, so the register is converted here where the viewer's
    /// time zone is known.
    /// </summary>
    private void MapTimes(List<CertificateListItemDto> items)
    {
        foreach (var item in items)
        {
            item.IssuedAtUserTime = item.IssuedAtUserTime.ToUserTime(HttpContext);
            if (item.RevokedAtUserTime.HasValue)
                item.RevokedAtUserTime = item.RevokedAtUserTime.Value.ToUserTime(HttpContext);
        }
    }

    /// <summary>Start of the viewer's current month, expressed in UTC for the database.</summary>
    private DateTime CurrentMonthStartUtc()
    {
        var userTimeZone = TimeZoneHelper.GetUserTimeZone(HttpContext);
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, userTimeZone);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(monthStart, userTimeZone);
    }

    private static string NormalizeSort(string? sort, string? dir)
    {
        if (string.IsNullOrWhiteSpace(sort) || !SortableColumns.Contains(sort))
            return "issued_desc";

        return string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? $"{sort}_desc" : sort;
    }

    private static string SortKey(string order) =>
        order.EndsWith("_desc", StringComparison.Ordinal) ? order[..^5] : order;

    private static string BuildFilterSummary(CertificateType? type, CertificateStatus? status, string? search, int count)
    {
        var parts = new List<string> { $"عدد السجلات: {count}" };
        if (type.HasValue) parts.Add($"النوع: {type.Value}");
        if (status.HasValue) parts.Add($"الحالة: {status.Value}");
        if (!string.IsNullOrWhiteSpace(search)) parts.Add($"البحث: {search.Trim()}");
        parts.Add($"تاريخ التصدير: {DateTime.Now:yyyy/MM/dd HH:mm}");
        return string.Join("  |  ", parts);
    }

    private Guid? InstituteId() =>
        Guid.TryParse(User.FindFirstValue("InstituteId"), out var id) ? id : null;

    private Guid? UserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
