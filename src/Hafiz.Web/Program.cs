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

var builder = WebApplication.CreateBuilder(args);

//adding services to the container
builder.Services.AddHafizServices(builder.Environment.EnvironmentName);

// Custom service registrations
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseHafizMiddlewares();
app.Run();
