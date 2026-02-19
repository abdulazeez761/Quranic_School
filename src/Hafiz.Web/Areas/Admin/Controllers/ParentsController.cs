using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Models.Parent>? parents = await _parentService.GetAllAsync();
            return View(parents);
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

            var (Success, ErrorMessage) = await _parentService.AddAsync(registerDto);
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
            var parent = await _parentService.GetByIdAsync(id);

            if (parent is null)
                return NotFound();

            EditParentDto editParentDto = new EditParentDto
            {
                ParentID = id,
                FirstName = parent.FirstName,
                SecondName = parent.SecondName,
                Email = parent.Email,
                PhoneNumber = parent.PhoneNumber,
                Username = parent.Username
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

                await _parentService.UpdateAsync(newData);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(newData);
            }
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            await _parentService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
