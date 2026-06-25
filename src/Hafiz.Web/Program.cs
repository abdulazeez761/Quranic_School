using System.Security.Claims;
using System.Threading.RateLimiting;
using Hafiz.Application;
using Hafiz.Application.Extensions;
using Hafiz.Infrastructure.Extensions;
using Hafiz.Web.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//adding services to the container
builder.Services.AddHafizServices(builder.Environment.EnvironmentName);

// Custom service registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Scheduled daily database backup → Google Drive
builder.Services.AddHostedService<Hafiz.Web.BackgroundServices.DailyBackupService>();

var app = builder.Build();
app.UseHafizMiddlewares();

// Seed SuperAdmin if it doesn't exist
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Hafiz.Data.ApplicationDbContext>();
    var passwordHasher =
        scope.ServiceProvider.GetRequiredService<Hafiz.Application.Interfaces.IPasswordHasher>();

    // Apply migrations if needed
    context.Database.Migrate();

    if (!context.Users.Any(u => u.Role == Hafiz.Models.UserRole.SuperAdmin))
    {
        var superAdmin = new Hafiz.Models.User
        {
            Username = "superadmin",
            FirstName = "سوبر",
            SecondName = "أدمن",
            PhoneNumber = "0790000000",
            Email = "superadmin@hafiz.com",
            Password = passwordHasher.HashPassword("SuperAdmin123!"),
            Role = Hafiz.Models.UserRole.SuperAdmin,
        };
        context.Users.Add(superAdmin);
        context.SaveChanges();
    }
}

app.Run();
