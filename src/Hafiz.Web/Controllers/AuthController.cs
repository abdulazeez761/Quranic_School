using System.Security.Claims;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Hafiz.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: Login
        [HttpGet]
        public ActionResult Login() => View();

        [HttpPost]
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> Login(LoginDto dto, string? ReturnUrl)
        {
            if (!ModelState.IsValid)
                return View(dto);

            User? user = await _authService.LoginAsync(dto);

            if (user is null)
            {
                ModelState.AddModelError("", "كلمة السر أو اسم المستخدم غير صحيح.");
                return View(dto);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName + " " + user.SecondName),
                new Claim("Username", user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            if (ReturnUrl != null && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            switch (user.Role)
            {
                case UserRole.Admin:
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
                case UserRole.Teacher:
                    return RedirectToAction("Index", "Home", new { area = "Teacher" });
                case UserRole.Student:
                    return RedirectToAction("Index", "Student");
                case UserRole.Parent:
                    return RedirectToAction("Index", "Parent");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        // GET: Register
        [HttpGet]
        public ActionResult Register() => View();

        [HttpPost]
        [EnableRateLimiting("Auth")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authService.RegisterAsync(dto);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(dto);
            }

            return RedirectToAction("Login"); // will be change to redirected to admin create user page
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult NotAuth() => View();
    }
}
