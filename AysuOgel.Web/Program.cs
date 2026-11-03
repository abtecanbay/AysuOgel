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

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.ContentType = "text/plain; charset=utf-8";
        await ctx.HttpContext.Response.WriteAsync(
            "Çok fazla deneme yaptınız. Lütfen biraz sonra tekrar deneyin.", ct);
    };

    // Aynı IP'den iletişim formuna 10 dakikada en fazla 5 gönderim.
    o.AddPolicy("contact", http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Aynı IP'den yönetici girişine 15 dakikada en fazla 5 deneme.
    o.AddPolicy("admin-login", http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    // Aynı IP'den üye girişine (seans sorgulama) 15 dakikada en fazla 10 deneme.
    o.AddPolicy("member-login", http =>
        RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
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