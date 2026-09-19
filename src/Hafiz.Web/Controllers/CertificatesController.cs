using System;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Web.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class CertificatesController : Controller
{
    private readonly ICertificateService _certificateService;

    public CertificatesController(ICertificateService certificateService)
    {
        _certificateService = certificateService;
    }

    // GET: /Certificates/Matn/{id}
    [HttpGet("Certificates/Matn/{id:guid}")]
    public async Task<IActionResult> Matn(Guid id)
    {
        var model = await _certificateService.GetMatnCertificateAsync(id);
        if (model == null)
        {
            TempData["ErrorMessage"] = "تعذر إصدار الشهادة: الطالب لم يتمم بعد متطلبات دراسة أو حفظ هذا المتن.";
            return View("CertificateNotFound");
        }

        return View("CertificateFrame", model);
    }

    // GET: /Certificates/Quran/{studentId}?fromJuz=...&toJuz=...
    [HttpGet("Certificates/Quran/{studentId:guid}")]
    public async Task<IActionResult> Quran(Guid studentId, [FromQuery] int? fromJuz = null, [FromQuery] int? toJuz = null)
    {
        var model = await _certificateService.GetQuranCertificateAsync(studentId, fromJuz, toJuz);
        if (model == null)
        {
            TempData["ErrorMessage"] = "تعذر إصدار شهادة القرآن: بيانات الطالب غير موجودة.";
            return View("CertificateNotFound");
        }

        return View("CertificateFrame", model);
    }
}
