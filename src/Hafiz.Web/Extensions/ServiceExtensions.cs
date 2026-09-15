using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.RateLimiting;
using System.Threading.Tasks;
using Hafiz.Application;
using Hafiz.Application.Interfaces;
using Hafiz.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Hafiz.Web.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddHafizServices(
            this IServiceCollection services,
            string environmentName
        )
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUserTimeProvider, UserTimeProvider>();

            services
                .AddControllersWithViews(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                })
                .AddViewLocalization()
                .AddDataAnnotationsLocalization(options =>
                {
                    options.DataAnnotationLocalizerProvider = (type, factory) =>
                        factory.Create(typeof(SharedDtoResource));
                });

            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login";
                    options.LogoutPath = "/Auth/Logout";
                    options.AccessDeniedPath = "/Auth/NotAuth";
                    options.ExpireTimeSpan = TimeSpan.FromDays(30);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.IsEssential = true;

                    // Events to handle logout
                    options.Events.OnSigningOut = async context =>
                    {
                        // Clear the cookie explicitly
                        context.Response.Cookies.Delete(
                            CookieAuthenticationDefaults.AuthenticationScheme
                        );
                        await Task.CompletedTask;
                    };
                    options.Events.OnValidatePrincipal = async context =>
                    {
                        var dbContext =
                            context.HttpContext.RequestServices.GetRequiredService<Hafiz.Data.ApplicationDbContext>();
                        var userIdStr = context
                            .Principal?.FindFirst(ClaimTypes.NameIdentifier)
                            ?.Value;
                        if (Guid.TryParse(userIdStr, out var userId))
                        {
                            // نجلب حالة المستخدم وحالة مركزه باستعلام خفيف واحد
                            var userStatus = await dbContext
                                .Users.IgnoreQueryFilters()
                                .Where(u => u.Id == userId)
                                .Select(u => new
                                {
                                    u.IsDeleted,
                                    // إذا كان المستخدم يتبع مركزاً، نتأكد أن المركز ليس محذوفاً وليس معطلاً
                                    IsInstituteDeactivated = u.Institute != null
                                        && (u.Institute.IsDeleted || !u.Institute.IsActive),
                                })
                                .FirstOrDefaultAsync();
                            // يطرد المستخدم فوراً إذا:
                            // 1. المستخدم غير موجود
                            // 2. أو المستخدم محذوف شخصياً
                            // 3. أو المركز التابع له محذوف/معطل
                            if (
                                userStatus == null
                                || userStatus.IsDeleted
                                || userStatus.IsInstituteDeactivated
                            )
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync();
                            }
                        }
                        else
                        {
                            context.RejectPrincipal();
                            await context.HttpContext.SignOutAsync();
                        }
                    };
                });

            services.AddAuthorization(op =>
            {
                op.AddPolicy(
                    "AdminOrTeacher",
                    policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "Teacher")
                );
                op.AddPolicy("Student", policy => policy.RequireClaim(ClaimTypes.Role, "Student"));
            });

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Static files (css, js, images...) are excluded from rate limiting
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    context =>
                    {
                        var path = context.Request.Path.Value ?? "";
                        if (
                            path.Contains("/css/")
                            || path.Contains("/js/")
                            || path.Contains("/lib/")
                            || path.Contains("/images/")
                            || path.Contains("/icons/")
                            || path.Contains("/fonts/")
                            || path.EndsWith(".ico")
                            || path.EndsWith(".webmanifest")
                        )
                        {
                            return RateLimitPartition.GetNoLimiter("static");
                        }

                        return RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = 30,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                                TokensPerPeriod = 10,
                                AutoReplenishment = true,
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 0,
                            }
                        );
                    }
                );

                // Auth: Fixed Window per IP → max 5 login/register attempts per minute (anti brute-force)
                options.AddPolicy(
                    "Auth",
                    context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromMinutes(1),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 0,
                            }
                        )
                );
            });
            return services;
        }
    }
}
