using System;
using System.Threading.Tasks;
using System.Security.Claims;
using Hafiz.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class CertificatesController : Controller
{
    private readonly ICertificateIssuanceService _certificateService;

    public CertificatesController(ICertificateIssuanceService certificateService)
    {
        _certificateService = certificateService;
    }

    // GET: /Certificates/Matn/{id}
    [HttpGet("Certificates/Matn/{id:guid}")]
    public async Task<IActionResult> Matn(Guid id)
    {
        var model = await _certificateService.IssueMatnCertificateAsync(id, issuedBy: GetUserId(), baseUrl: GetBaseUrl());
        if (model == null)
        {
            TempData["ErrorMessage"] = "تعذر إصدار الشهادة: الطالب لم يتمم بعد متطلبات دراسة أو حفظ هذا المتن.";
            return View("CertificateNotFound");
        }

        return IsCurrentInstitute(model.InstituteId) ? View("CertificateFrame", model) : Forbid();
    }

    // GET: /Certificates/Quran/{studentId}?fromJuz=...&toJuz=...
    [HttpGet("Certificates/Quran/{studentId:guid}")]
    public async Task<IActionResult> Quran(Guid studentId, [FromQuery] int? fromJuz = null, [FromQuery] int? toJuz = null)
    {
        var model = await _certificateService.IssueQuranCertificateAsync(studentId, fromJuz, toJuz, issuedBy: GetUserId(), baseUrl: GetBaseUrl());
        if (model == null)
        {
            TempData["ErrorMessage"] = "تعذر إصدار شهادة القرآن: بيانات الطالب غير موجودة.";
            return View("CertificateNotFound");
        }

        return IsCurrentInstitute(model.InstituteId) ? View("CertificateFrame", model) : Forbid();
    }

    [HttpGet("Certificates/View/{id:guid}")]
    public async Task<IActionResult> ViewCertificate(Guid id)
    {
        var model = await _certificateService.GetCertificateModelAsync(id, GetBaseUrl());
        return model is null ? View("CertificateNotFound") : IsCurrentInstitute(model.InstituteId) ? View("CertificateFrame", model) : Forbid();
    }

    private Guid? GetUserId() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private bool IsCurrentInstitute(Guid instituteId) => User.IsInRole("SuperAdmin") ||
        (Guid.TryParse(User.FindFirstValue("InstituteId"), out var current) && current == instituteId);
    private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
}
