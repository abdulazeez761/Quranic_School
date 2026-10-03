using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Application.DTO.Certificate.TemplateConfiguration;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class CertificateTemplatesController : Controller
{
    private readonly ICertificateTemplateService _templates;

    public CertificateTemplatesController(ICertificateTemplateService templates) =>
        _templates = templates;

    public async Task<IActionResult> Index(CertificateType? type = null)
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        ViewBag.SelectedType = type;
        return View(await _templates.GetTemplatesByInstituteAsync(instituteId.Value, type));
    }

    public IActionResult Create() => View(new CreateTemplateDto { Type = CertificateType.Quran });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTemplateDto dto)
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        dto.InstituteId = instituteId.Value;
        if (!ModelState.IsValid)
            return View(dto);
        var created = await _templates.CreateTemplateAsync(dto, UserId());
        TempData["SuccessMessage"] = "تم إنشاء قالب الشهادة بنجاح، يمكنك الآن تصميمه.";
        return RedirectToAction(nameof(Edit), new { id = created.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound();
        return View(
            new UpdateTemplateConfigDto
            {
                TemplateId = id,
                Name = template.Name,
                Description = template.Description,
                IsDefault = template.IsDefault,
                IsActive = template.IsActive,
                ConfigurationJson = template.Configuration.ToJson(),
            }
        );
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateTemplateConfigDto dto)
    {
        if (await OwnTemplate(dto.TemplateId) is null)
            return NotFound();
        if (!ModelState.IsValid)
            return View(dto);
        try
        {
            await _templates.UpdateTemplateConfigAsync(dto, UserId());
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(dto.ConfigurationJson), ex.Message);
            return View(dto);
        }
        TempData["SuccessMessage"] = "تم حفظ الإعدادات وإنشاء نسخة جديدة من القالب.";
        // Back to the editor so the design can keep evolving without leaving the page.
        return RedirectToAction(nameof(Edit), new { id = dto.TemplateId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        if (await _templates.SetDefaultTemplateAsync(id, instituteId.Value))
            TempData["SuccessMessage"] = "تم تعيين القالب الافتراضي لهذا النوع.";
        else
            TempData["ErrorMessage"] = "تعذّر تعيين القالب الافتراضي.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        if (await _templates.DeleteTemplateAsync(id, instituteId))
            TempData["SuccessMessage"] = "تم حذف القالب.";
        else
            TempData["ErrorMessage"] = "تعذّر حذف القالب لوجود شهادات صادرة به.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Versions(Guid id)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound(new { message = "القالب غير موجود." });

        var versions = await _templates.GetTemplateVersionsAsync(id, InstituteId());
        return Json(new { success = true, templateName = template.Name, currentVersion = template.CurrentVersion, versions });
    }

    [HttpGet]
    public async Task<IActionResult> VersionConfig(Guid id, Guid versionId)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound(new { message = "القالب غير موجود." });

        var version = await _templates.GetTemplateVersionAsync(id, versionId, InstituteId());
        if (version is null)
            return NotFound(new { message = "النسخة غير موجودة." });

        return Json(new { success = true, version });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreVersion(Guid id, Guid versionId)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound();

        var restored = await _templates.RestoreVersionAsync(id, versionId, UserId(), InstituteId());
        if (restored)
        {
            TempData["SuccessMessage"] = "تمت استعادة النسخة بنجاح وإنشاء نسخة جديدة بالإعدادات المستعادة.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true, message = TempData["SuccessMessage"] });
        }
        else
        {
            TempData["ErrorMessage"] = "تعذّرت استعادة هذه النسخة.";
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return BadRequest(new { success = false, message = TempData["ErrorMessage"] });
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Duplicate(Guid id, string? newName = null)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound();

        var duplicated = await _templates.DuplicateTemplateAsync(id, newName, UserId(), InstituteId());
        if (duplicated != null)
        {
            TempData["SuccessMessage"] = $"تم نسخ القالب بنجاح: {duplicated.Name}";
            return RedirectToAction(nameof(Edit), new { id = duplicated.Id });
        }

        TempData["ErrorMessage"] = "تعذّر نسخ هذا القالب.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        var template = await OwnTemplate(id);
        if (template is null)
            return NotFound();

        if (await _templates.ToggleTemplateActiveAsync(id, InstituteId()))
            TempData["SuccessMessage"] = "تم تحديث حالة تفعيل القالب.";
        else
            TempData["ErrorMessage"] = "تعذّر تحديث حالة القالب.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Re-creates the built-in Quran/Matn templates when an institute has none.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SeedDefaults()
    {
        var instituteId = InstituteId();
        if (!instituteId.HasValue)
            return Forbid();
        await _templates.SeedDefaultTemplatesForInstituteAsync(instituteId.Value, UserId());
        TempData["SuccessMessage"] = "تمت إضافة القوالب الافتراضية الناقصة.";
        return RedirectToAction(nameof(Index));
    }

    private Guid? InstituteId() =>
        Guid.TryParse(User.FindFirstValue("InstituteId"), out var id) ? id : null;

    private Guid? UserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    private async Task<CertificateTemplateDto?> OwnTemplate(Guid id)
    {
        var instituteId = InstituteId();
        var template = await _templates.GetTemplateByIdAsync(id);
        return instituteId.HasValue && template?.InstituteId == instituteId.Value ? template : null;
    }
}
