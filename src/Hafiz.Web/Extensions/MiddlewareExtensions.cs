using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace Hafiz.Web.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseHafizMiddlewares(this WebApplication app)
        {
            // 1.الصحيح للمستخدم IP حتى نعرف ال
            //2. HTTPS حتى نعرف اذا الطلب اجا عن طريق
            app.UseForwardedHeaders();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // تم إيقافها لتجنب مشاكل Cloudflare Tunnel
            // app.UseHttpsRedirection();

            // 3. الترجمة واللغات
            string[]? supportedCultures = { "en", "ar" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("ar")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures)
                .AddInitialRequestCultureProvider(new CookieRequestCultureProvider());

            localizationOptions.RequestCultureProviders.Insert(
                0,
                new QueryStringRequestCultureProvider()
            );
            app.UseRequestLocalization(localizationOptions);

            // 4. Smart cache control (الكود الخاص بك هنا صحيح طالما أنه يضيف الـ Headers ويستدعي next)
            app.Use(
                async (context, next) =>
                {
                    var path = context.Request.Path.Value ?? "";
                    var response = context.Response;

                    if (path.EndsWith("/sw.js"))
                    {
                        response.Headers["Cache-Control"] =
                            "no-cache, no-store, must-revalidate, max-age=0";
                        response.Headers["Pragma"] = "no-cache";
                        response.Headers["CF-Cache-Control"] = "no-cache";
                    }
                    else if (
                        path.Contains("/lib/")
                        || path.Contains("/icons/")
                        || path.Contains("/images/")
                    )
                    {
                        response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
                        response.Headers["CF-Cache-Control"] = "max-age=31536000";
                    }
                    else if (path.Contains("/css/") || path.Contains("/js/"))
                    {
                        response.Headers["Cache-Control"] = "public, max-age=2592000, immutable";
                        response.Headers["CF-Cache-Control"] = "max-age=2592000";
                    }
                    else if (path.EndsWith(".html"))
                    {
                        response.Headers["Cache-Control"] = "public, max-age=3600, must-revalidate";
                        response.Headers["CF-Cache-Control"] = "max-age=3600";
                    }
                    else if (path.Contains("."))
                    {
                        response.Headers["Cache-Control"] = "public, max-age=300, must-revalidate";
                        response.Headers["CF-Cache-Control"] = "max-age=300";
                    }

                    await next();
                }
            );

            // 5. الترتيب القياسي لباقي الأنابيب (Pipeline)
            app.UseStaticFiles();

            app.UseRouting();

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute("areas", "{area:exists}/{controller=Home}/{action=Index}/{id?}");
            app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

            return app;
        }
    }
}
