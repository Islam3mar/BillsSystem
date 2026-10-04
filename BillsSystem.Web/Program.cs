using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using BillsSystem.Application;
using BillsSystem.Domain.Common;
using BillsSystem.Infrastructure;
using BillsSystem.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // FluentValidation هو مصدر الحقيقة، فمنع MVC من اعتبار أي string غير Nullable "Required" بالافتراضي
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;

    // أي POST/PUT/DELETE جديد بيتفحص الـ AntiForgery Token تلقائي (مش لازم نفتكر الـ Attribute في كل Action)
    // الـ StripeWebhookController لازم يكون عليه [IgnoreAntiforgeryToken] وإلا Stripe هيتحجب
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
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
    options.OnRejected = (context, _) =>
    {
        context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("LoginRateLimit")
            .LogWarning("Login rate limit exceeded from {Ip}", context.HttpContext.Connection.RemoteIpAddress);
        return ValueTask.CompletedTask;
    };
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 5,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// ورا Nginx / IIS / Azure / Docker: الـ Proxy لازم يتحط في ForwardedHeaders:KnownProxies (IP واحد)
// أو ForwardedHeaders:KnownNetworks (نطاق CIDR زي 10.0.0.0/8).
// (مش بنعمل KnownProxies.Clear() لأن ده بيخلي أي حد يزوّر X-Forwarded-For ويتخطى الـ Rate Limit)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;

    var proxies = builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? Array.Empty<string>();
    foreach (var proxy in proxies)
    {
        if (IPAddress.TryParse(proxy, out var address))
            options.KnownProxies.Add(address);
    }

    // .NET 10: KnownNetworks بقت Obsolete، البديل KnownIPNetworks من النوع System.Net.IPNetwork
    // (الاسم كامل لأن Microsoft.AspNetCore.HttpOverrides فيها IPNetwork قديم هيعمل Ambiguity)
    var networks = builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? Array.Empty<string>();
    foreach (var cidr in networks)
    {
        if (System.Net.IPNetwork.TryParse(cidr, out var network))
            options.KnownIPNetworks.Add(network);
    }
});

var app = builder.Build();

// لو المنطقة الزمنية مش موجودة (Linux/Docker من غير tzdata) التطبيق يقع هنا مش في نص الشغل
_ = AppClock.Now;

// App:PublicBaseUrl بيتبني منه لينك الرجوع من Stripe. فاضي = بنستخدم Host الطلب، لكن لو اتكتب لازم يكون URL سليم
// (ده بيكشف الـ Placeholder اللي في appsettings.Production.json أول ما التطبيق يشتغل بدل ما PayWithStripe يفشل بعدين)
if (!app.Environment.IsDevelopment())
{
    var baseUrl = app.Configuration["App:PublicBaseUrl"];
    if (!string.IsNullOrWhiteSpace(baseUrl))
    {
        var valid = Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri)
                    && baseUri.Scheme == Uri.UriSchemeHttps
                    && (baseUri.Host.Contains('.') || baseUri.Host == "localhost");
        if (!valid)
            throw new InvalidOperationException(
                "App:PublicBaseUrl must be a valid https URL (e.g. https://bills.example.com), or left empty");
    }
}

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

// Security Headers (مفيش CSP عن قصد: الموقع فيه سكربتات inline ومحتاج Nonce)
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US"),
    SupportedCultures = new[] { new CultureInfo("en-US") },
    SupportedUICultures = new[] { new CultureInfo("en-US") }
});

app.UseRouting();

app.UseRateLimiter();      // بعد UseRouting (عشان [EnableRateLimiting] على الـ Endpoint) وقبل Authentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();