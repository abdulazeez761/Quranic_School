using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [Authorize(Roles = "SuperAdmin")]
    public class BackupController : Controller
    {
        private readonly IBackupService _backupService;
        private readonly ILogger<BackupController> _logger;

        public BackupController(IBackupService backupService, ILogger<BackupController> logger)
        {
            _backupService = backupService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(CancellationToken ct)
        {
            try
            {
                var fileName = await _backupService.RunBackupAsync(ct);
                TempData["SuccessMessage"] =
                    $"تم إنشاء النسخة الاحتياطية ورفعها إلى Google Drive بنجاح: {fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل إنشاء النسخة الاحتياطية");
                TempData["ErrorMessage"] = $"فشل إنشاء النسخة الاحتياطية: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
