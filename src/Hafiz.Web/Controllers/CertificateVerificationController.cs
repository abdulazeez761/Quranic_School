using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Web.Controllers;

/// <summary>Public endpoint used by certificate QR codes; it intentionally requires no login.</summary>
public class CertificateVerificationController : Controller
{
    private readonly ICertificateIssuanceService _certificates;
    public CertificateVerificationController(ICertificateIssuanceService certificates) => _certificates = certificates;

    [HttpGet("Certificates/Verify/{token}")]
    public async Task<IActionResult> Verify(string token)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        return View(await _certificates.VerifyCertificateAsync(token, baseUrl));
    }
}
