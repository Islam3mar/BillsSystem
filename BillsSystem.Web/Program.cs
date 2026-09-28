using System.Globalization;
using System.Threading.RateLimiting;
using BillsSystem.Application;
using BillsSystem.Domain.Common;
using BillsSystem.Infrastructure;
using BillsSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // FluentValidation هو مصدر الحقيقة، فمنع MVC من اعتبار أي string غير Nullable "Required" بالافتراضي
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// أي Controller/Action محمي تلقائي، إلا اللي عليه [AllowAnonymous]
builder.Services.AddAuthorization(options =>
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

// أقصى 5 محاولات Login في الدقيقة من نفس الـ IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// ورا Nginx / IIS / Azure / Docker: من غير ده كل المستخدمين بيبانوا بنفس الـ IP (IP الـ Proxy)
// فالـ Rate Limiter هيحسبهم كلهم كواحد
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// لو المنطقة الزمنية مش موجودة (Linux/Docker من غير tzdata) التطبيق يقع هنا مش في نص الشغل
_ = AppClock.Now;

app.UseForwardedHeaders();   // لازم أول Middleware

// الـ Migration + Seed: تلقائي في الـ Development بس. في الـ Production فعّله بـ RunMigrationsOnStartup=true
// بعد ما تاخد Backup، وسيبه شغال على نسخة واحدة بس من التطبيق
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
    await app.MigrationAndSeedAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { new CultureInfo("en-US") },
    SupportedUICultures = new[] { new CultureInfo("en-US") }
});

app.UseRouting();

app.UseRateLimiter();      // قبل Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();