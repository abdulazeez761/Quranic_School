using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CertificatesManagementController : Controller
{
    private readonly ICertificateIssuanceService _certificates;

    public CertificatesManagementController(ICertificateIssuanceService certificates) =>
        _certificates = certificates;

    public async Task<IActionResult> Index(
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1
    )
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        ViewBag.SelectedType = type;
        ViewBag.SelectedStatus = status;
        ViewBag.Search = search;
        ViewBag.Page = Math.Max(1, page);
        ViewBag.Total = await _certificates.GetHistoryCountAsync(
            instituteId,
            type: type,
            status: status,
            search: search
        );
        return View(
            await _certificates.GetHistoryAsync(
                instituteId,
                type: type,
                status: status,
                search: search,
                page: page
            )
        );
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Revoke(Guid id, string reason)
    {
        var instituteId = InstituteId();
        var userId = UserId();
        if (!instituteId.HasValue || !userId.HasValue)
            return Forbid();
        if (await _certificates.RevokeCertificateAsync(id, reason, userId.Value, instituteId))
            TempData["SuccessMessage"] = "تم إلغاء اعتماد الشهادة، ولم تعد سارية عند التحقق.";
        else
            TempData["ErrorMessage"] = "تعذّر إلغاء اعتماد الشهادة؛ تأكد من أنها ما زالت سارية.";
        return RedirectToAction(nameof(Index));
    }

    private Guid? InstituteId() =>
        Guid.TryParse(User.FindFirstValue("InstituteId"), out var id) ? id : null;

    private Guid? UserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}
