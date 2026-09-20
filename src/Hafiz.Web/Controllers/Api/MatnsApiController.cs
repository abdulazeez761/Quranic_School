using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Web.Controllers.Api;

[ApiController]
[Route("api/matns")]
[Authorize]
public class MatnsApiController : ControllerBase
{
    private readonly IMatnReferenceService _referenceService;

    public MatnsApiController(IMatnReferenceService referenceService)
    {
        _referenceService = referenceService;
    }

    // GET /api/matns
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var matns = await _referenceService.GetAllAsync();
        var response = matns.Select(m => new
        {
            id = m.Id,
            name = m.Name,
            author = m.Author,
            category = m.Category,
            chapters = (m.Chapters ?? System.Array.Empty<Hafiz.Application.Models.Matn.MatnChapterDefinition>())
                .Select(c => new { number = c.Number, name = c.Name }).ToList(),
            units = (m.Units ?? System.Array.Empty<Hafiz.Application.Models.Matn.MatnUnitDefinition>())
                .Select(u => new { number = u.Number, name = u.Name, singular = u.Singular }).ToList()
        });

        return Ok(response);
    }

    // GET /api/matns/{matnId}
    [HttpGet("{matnId}")]
    public async Task<IActionResult> GetById(string matnId)
    {
        if (string.IsNullOrWhiteSpace(matnId))
        {
            return BadRequest(new { message = "معرف المتن مطلوب" });
        }

        var matn = await _referenceService.GetByIdOrNameAsync(matnId);
        if (matn == null)
        {
            return NotFound(new { message = "تعذر العثور على بيانات المتن المطلوب" });
        }

        return Ok(new
        {
            id = matn.Id,
            name = matn.Name,
            author = matn.Author,
            category = matn.Category,
            chapters = (matn.Chapters ?? System.Array.Empty<Hafiz.Application.Models.Matn.MatnChapterDefinition>())
                .Select(c => new { number = c.Number, name = c.Name }).ToList(),
            units = (matn.Units ?? System.Array.Empty<Hafiz.Application.Models.Matn.MatnUnitDefinition>())
                .Select(u => new { number = u.Number, name = u.Name, singular = u.Singular }).ToList()
        });
    }
}
