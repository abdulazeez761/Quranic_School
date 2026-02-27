using System.Diagnostics;
using System.Security.Claims;
using Hafiz.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace Hafiz.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        // Console.WriteLine("role is: " + role);

        switch (role)
        {
            case nameof(UserRole.Admin):
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            case nameof(UserRole.Teacher):
                return RedirectToAction("Index", "Home", new { area = "Teacher" });
            case nameof(UserRole.Student):
                return RedirectToAction("Index", "Home", new { area = "Student" });
            case nameof(UserRole.Parent):
                return RedirectToAction("Index", "Home", new { area = "Parent" });
            default:
                return View();
        }
    }

    public IActionResult Contact()
    {
        return View();
    }

    public IActionResult Plans()
    {
        return View();
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );

        return LocalRedirect(returnUrl);
    }

    [HttpGet]
    public IActionResult Hafiz()
    {
        var role = User.FindFirstValue(ClaimTypes.Role);
        ViewData["Role"] = role;
        return View();
    }

    [Route("Home/Error")]
    public IActionResult Error()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        // Optionally log the exception here
        _logger.LogError(exception, "An error occurred.");

        ViewData["ErrorMessage"] = exception?.Message ?? "An unexpected error occurred.";
        return View();
    }
}
