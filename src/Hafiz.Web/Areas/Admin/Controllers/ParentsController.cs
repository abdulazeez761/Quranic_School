using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hafiz.Application.Common;
using Hafiz.Application.Extensions;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ParentsController : Controller
    {
        private readonly IParentService _parentService;

        public ParentsController(IParentService parentService)
        {
            _parentService = parentService;
        }

        private Guid? GetInstituteId()
        {
            var claim = User.FindFirstValue("InstituteId");
            return claim != null ? Guid.Parse(claim) : null;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            bool archived = false,
            int page = 1,
            int pageSize = 12,
            string? search = null
        )
        {
            var instituteId = GetInstituteId();
            IEnumerable<Models.Parent> parents;

            if (archived)
            {
                parents = instituteId.HasValue
                    ? await _parentService.GetArchivedParentsByInstituteAsync(instituteId.Value)
                    : await _parentService.GetArchivedParentsAsync();
            }
            else
            {
                parents = instituteId.HasValue
                    ? await _parentService.GetAllByInstituteAsync(instituteId.Value)
                    : await _parentService.GetAllAsync();
            }

            var parentsList = parents.ToList();
            var totalParents = parentsList.Count;
            var parentsWithChildren = parentsList.Count(p => p.Students != null && p.Students.Any());
            var parentsWithoutChildren = totalParents - parentsWithChildren;
            var totalChildren = parentsList.Sum(p => p.Students?.Count ?? 0);

            ViewBag.TotalParents = totalParents;
            ViewBag.ParentsWithChildren = parentsWithChildren;
            ViewBag.ParentsWithoutChildren = parentsWithoutChildren;
            ViewBag.TotalChildren = totalChildren;

            var filtered = parentsList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                filtered = filtered.Where(p =>
                    p.ParentInfo != null && (
                        (p.ParentInfo.FirstName != null && p.ParentInfo.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (p.ParentInfo.SecondName != null && p.ParentInfo.SecondName.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (p.ParentInfo.Username != null && p.ParentInfo.Username.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                        (p.ParentInfo.PhoneNumber != null && p.ParentInfo.PhoneNumber.Contains(term, StringComparison.OrdinalIgnoreCase))
                    )
                );
            }

            var pagedParents = filtered.ToPagedResult(page, pageSize);

            var (activeCount, archivedCount) = await _parentService.GetCountsAsync(instituteId);
            ViewBag.IsArchived = archived;
            ViewBag.ActiveCount = activeCount;
            ViewBag.ArchivedCount = archivedCount;
            ViewBag.Search = search;

            return View(pagedParents);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterParentDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var instituteId = GetInstituteId();
            var (Success, ErrorMessage) = await _parentService.AddAsync(registerDto, instituteId);
            if (!Success)
            {
                ModelState.AddModelError("", ErrorMessage);
                return View(registerDto);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var instituteId = GetInstituteId();
            var parent = await _parentService.GetByIdAsync(id, instituteId);

            if (parent is null)
                return NotFound();

            EditParentDto editParentDto = new EditParentDto
            {
                ParentID = id,
                FirstName = parent.FirstName,
                SecondName = parent.SecondName,
                Email = parent.Email,
                PhoneNumber = parent.PhoneNumber,
                Username = parent.Username,
            };

            return View(editParentDto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditParentDto newData)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(newData);

                var instituteId = GetInstituteId();
                if (newData.ParentID.HasValue)
                {
                    var existingParent = await _parentService.GetByIdAsync(newData.ParentID.Value, instituteId);
                    if (existingParent == null)
                        return Forbid();
                }

                await _parentService.UpdateAsync(newData);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(newData);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var instituteId = GetInstituteId();
            var deleted = await _parentService.DeleteAsync(id, instituteId);
            if (!deleted)
            {
                TempData["ErrorMessage"] = "غير مصرح لك بحذف ولي الأمر هذا أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت أرشفة ولي الأمر بنجاح، ويمكنك استعادته في أي وقت من قسم الأرشيف.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Restore(Guid id)
        {
            var instituteId = GetInstituteId();
            var restored = await _parentService.RestoreParentAsync(id, instituteId);
            if (!restored)
            {
                TempData["ErrorMessage"] = "غير مصرح لك باستعادة ولي الأمر هذا أو أنه غير موجود.";
            }
            else
            {
                TempData["SuccessMessage"] = "تمت استعادة ولي الأمر بنجاح وإعادة تفعيله.";
            }
            return RedirectToAction(nameof(Index), new { archived = true });
        }
    }
}
