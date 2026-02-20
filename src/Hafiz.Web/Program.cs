using System.Security.Claims;
using Hafiz.Application;
using Hafiz.Application.Extensions;
using Hafiz.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder
    .Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedDtoResource));
    });
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder
    .Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/NotAuth";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.IsEssential = true;

        // Events to handle logout
        options.Events.OnSigningOut = async context =>
        {
            // Clear the cookie explicitly
            context.Response.Cookies.Delete(CookieAuthenticationDefaults.AuthenticationScheme);
            await Task.CompletedTask;
        };
    });

builder.Services.AddAuthorization(op =>
{
    op.AddPolicy(
        "AdminOrTeacher",
        policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "Teacher")
    );
    op.AddPolicy("Student", policy => policy.RequireClaim(ClaimTypes.Role, "Student"));
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

string[]? supportedCultures = { "en", "ar" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("ar")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures)
    .AddInitialRequestCultureProvider(new CookieRequestCultureProvider());
localizationOptions.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
app.UseRequestLocalization(localizationOptions);

// Smart cache control - only sw.js never cached, everything else uses versioning
app.Use(
    async (context, next) =>
    {
        var path = context.Request.Path.Value ?? "";
        var response = context.Response;

        // Service Worker: NEVER cache (always fresh from origin)
        if (path.EndsWith("/sw.js"))
        {
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate, max-age=0";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["CF-Cache-Control"] = "no-cache";
        }
        // Versioned static assets (with asp-append-version): Cache for 1 year (immutable)
        // These have content hashes in URLs so old versions never conflict
        else if (path.Contains("/lib/") || path.Contains("/icons/") || path.Contains("/images/"))
        {
            response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
            response.Headers["CF-Cache-Control"] = "max-age=31536000";
        }
        // CSS and JS (with asp-append-version): Cache for 30 days
        // When content changes, hash changes → new URL downloaded automatically
        else if (path.Contains("/css/") || path.Contains("/js/"))
        {
            response.Headers["Cache-Control"] = "public, max-age=2592000, immutable";
            response.Headers["CF-Cache-Control"] = "max-age=2592000";
        }
        // Static HTML files (e.g., offline.html): Short cache
        else if (path.EndsWith(".html"))
        {
            response.Headers["Cache-Control"] = "public, max-age=3600, must-revalidate";
            response.Headers["CF-Cache-Control"] = "max-age=3600";
        }
        // Other static files with extensions (e.g., .json, .xml): Cache for 5 minutes
        else if (path.Contains("."))
        {
            response.Headers["Cache-Control"] = "public, max-age=300, must-revalidate";
            response.Headers["CF-Cache-Control"] = "max-age=300";
        }

        await next();
    }
);

// Configure the HTTP request pipeline.


// Only use HTTPS redirection and HSTS in Development (Cloudflare tunnel handles HTTPS in production)
if (app.Environment.IsDevelopment())
{
    app.UseHsts(); //a web security policy that forces a browser to connect to a website only using an encrypted HTTPS connection.

    //when the exception occurs it handles it by going to the middle wares and fetching the exception then you can log the message
    app.UseExceptionHandler("/Home/Error");
}

// app.UseHttpsRedirection(); // Commented out - causes issues with Cloudflare tunnel
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
