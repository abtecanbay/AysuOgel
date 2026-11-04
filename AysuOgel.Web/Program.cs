using AysuOgel.Business.Abstract;
using AysuOgel.Business.Concrete;
using AysuOgel.DataAccess.Contexts;
using AysuOgel.Web.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("DefaultConnection bulunamadı.");

builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IServicesPageService, StaticServicesPageService>();
builder.Services.AddScoped<IHomePageService, StaticHomePageService>();
builder.Services.AddScoped<IAboutPageService, AboutPageService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IAppointmentSlotService, DbAppointmentSlotService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
builder.Services.AddScoped<IEmailSender, NotConfiguredEmailSender>();
builder.Services.AddScoped<IMessageReplyService, MessageReplyService>();
builder.Services.AddScoped<ISuccessStoryService, SuccessStoryService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminPageService, AdminPageService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddMemoryCache();

// IP başına kayan pencere (sliding window) limitleri. Sabit pencerenin aksine sayaç
// pencere sınırında birden sıfırlanmaz; "son X dakikada kaç istek" her an hesaplanır,
// böylece sınırdan hemen önce ve hemen sonra art arda deneme yapılamaz.
var rateLimitWindows = new Dictionary<string, TimeSpan>
{
    ["contact"] = TimeSpan.FromMinutes(10),      // iletişim formu: 10 dakikada 5 gönderim
    ["admin-login"] = TimeSpan.FromMinutes(15),  // yönetici girişi: 15 dakikada 5 deneme
    ["member-login"] = TimeSpan.FromMinutes(15)  // üye girişi (seans sorgulama): 15 dakikada 10 deneme
};

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (ctx, ct) =>
    {
        var http = ctx.HttpContext;

        // Kayan pencere limiter'ı kalan süreyi bildirmez. Limiti dolduran bir IP en geç
        // pencere süresi kadar sonra tekrar hak kazanır; bu güvenli üst sınırı bildiriyoruz.
        var policy = http.GetEndpoint()?.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName;
        var message = "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin.";
        if (policy is not null && rateLimitWindows.TryGetValue(policy, out var window))
        {
            http.Response.Headers.RetryAfter = ((int)window.TotalSeconds).ToString();
            message = $"Çok fazla deneme yaptınız. Lütfen {(int)window.TotalMinutes} dakika sonra tekrar deneyin.";
        }

        http.Response.ContentType = "text/plain; charset=utf-8";
        await http.Response.WriteAsync(message, ct);
    };

    void AddIpSlidingPolicy(string name, int permitLimit) =>
        o.AddPolicy(name, http =>
            RateLimitPartition.GetSlidingWindowLimiter(
                http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = rateLimitWindows[name],
                    SegmentsPerWindow = 5,
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));

    AddIpSlidingPolicy("contact", permitLimit: 5);
    AddIpSlidingPolicy("admin-login", permitLimit: 5);
    AddIpSlidingPolicy("member-login", permitLimit: 10);
});

// Panelden eklenen yöneticiler yalnızca kendilerine atanan sayfalara erişebilir;
// paneldeki her değişiklik işlem geçmişine yazılır.
builder.Services.AddControllersWithViews(o =>
{
    o.Filters.Add<AdminPageAccessFilter>();
    o.Filters.Add<AdminActivityLogFilter>();
});

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Admin/Account/Login";
        o.AccessDeniedPath = "/Admin/Account/AccessDenied";
        o.Cookie.Name = "__Host-AysuOgel.Admin";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Path = "/";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;

        // Panelden eklenen bir yönetici silinir veya pasife alınırsa açık oturumu da hemen sonlanır.
        o.Events.OnValidatePrincipal = async ctx =>
        {
            var adminId = ctx.Principal?.GetAdminUserId();
            if (adminId is null) return;

            var admins = ctx.HttpContext.RequestServices.GetRequiredService<IAdminUserService>();
            var admin = await admins.GetByIdAsync(adminId.Value);
            if (admin is null || !admin.IsActive)
            {
                ctx.RejectPrincipal();
                await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
        };
    })
    // Üyelerin seans sorgulama oturumu; yönetici oturumundan tamamen ayrıdır.
    .AddCookie(MemberAuth.Scheme, o =>
    {
        o.LoginPath = "/seans-sorgula";
        o.AccessDeniedPath = "/seans-sorgula";
        o.Cookie.Name = "__Host-AysuOgel.Member";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Path = "/";
        o.ExpireTimeSpan = TimeSpan.FromHours(2);
        o.SlidingExpiration = true;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Services}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();