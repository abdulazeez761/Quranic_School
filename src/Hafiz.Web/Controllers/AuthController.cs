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
                ModelState.AddModelError(string.Empty, "كلمة السر أو اسم المستخدم غير صحيح.");
                return View(dto);
            }

            if (user.InstituteId is null && user.Role != UserRole.SuperAdmin)
            {
                ModelState.AddModelError(string.Empty, "لا يوجد مركز تحفيظ مرتبط بهذا الحساب.");
                return View(dto);
            }

            if (user.Institute is not null && user.Institute.IsDeleted)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "المعهد المرتبط بحسابك غير مفعّل أو تم إيقافه مؤقتاً."
                );
                return View(dto);
            }

            await SignInUserAsync(user);

            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                return LocalRedirect(ReturnUrl);

            return RedirectByUserRole(user.Role);
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.SecondName}"),
                new("Username", user.Username),
                new(ClaimTypes.Role, user.Role.ToString()),
            };

            if (user.InstituteId.HasValue)
            {
                claims.Add(new Claim("InstituteId", user.InstituteId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddDays(30),
                }
            );
        }

        private IActionResult RedirectByUserRole(UserRole role) =>
            role switch
            {
                UserRole.Admin => RedirectToAction("Index", "Home", new { area = "Admin" }),
                UserRole.Teacher => RedirectToAction("Index", "Home", new { area = "Teacher" }),
                UserRole.Student => RedirectToAction("Index", "Student"),
                UserRole.Parent => RedirectToAction("Index", "Parent"),
                UserRole.SuperAdmin => RedirectToAction(
                    "Index",
                    "Home",
                    new { area = "SuperAdmin" }
                ),
                _ => RedirectToAction("Index", "Home"),
            };

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
