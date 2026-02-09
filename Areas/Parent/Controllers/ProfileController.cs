using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParentModel = Hifz.Models.Parent;

namespace Hifz.Areas.Parent.Controllers
{
    [Authorize(Roles = "Parent")]
    [Area("Parent")]
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IParentService _parentService;

        public ProfileController(ILogger<ProfileController> logger, IParentService parentService)
        {
            _logger = logger;
            _parentService = parentService;
        }

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ParentModel? parent = await _parentService.GetParentByUserIdAsync(userId);

            if (parent == null)
            {
                return NotFound("Parent profile not found.");
            }

            return View(parent);
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ParentModel? parent = await _parentService.GetParentByUserIdAsync(userId);

            if (parent == null)
            {
                return NotFound("Parent profile not found.");
            }

            var dto = new EditParentDto
            {
                ParentID = parent.UserId,
                FirstName = parent.ParentInfo.FirstName,
                SecondName = parent.ParentInfo.SecondName,
                Username = parent.ParentInfo.Username,
                Email = parent.ParentInfo.Email,
                PhoneNumber = parent.ParentInfo.PhoneNumber,
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditParentDto dto)
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            dto.ParentID = userId;

            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            try
            {
                await _parentService.UpdateAsync(dto);
                TempData["Success"] = "ProfileUpdated";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating parent profile");
                TempData["Error"] = "ProfileUpdateError";
                return View(dto);
            }
        }
    }
}
