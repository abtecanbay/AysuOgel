using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AysuOgel.Business.Abstract;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AysuOgel.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly IConfiguration _config;
    private readonly IAdminUserService _admins;
    private readonly IAdminPageService _pages;
    private readonly IActivityLogService _logs;

    public AccountController(IConfiguration config, IAdminUserService admins, IAdminPageService pages, IActivityLogService logs)
    {
        _config = config;
        _admins = admins;
        _pages = pages;
        _logs = logs;
    }

    /// <summary>Girişten sonraki açılış: yöneticinin erişebildiği ilk sayfaya yönlendirir.</summary>
    [HttpGet, Authorize]
    public async Task<IActionResult> Start()
    {
        var first = (await _pages.GetAccessibleAsync(User.GetAdminUserId())).FirstOrDefault();
        return first is null
            ? RedirectToAction(nameof(AccessDenied))
            : RedirectToAction("Index", first.Controller, new { area = "Admin" });
    }

    [HttpGet, Authorize]
    public async Task<IActionResult> AccessDenied() =>
        View(await _pages.GetAccessibleAsync(User.GetAdminUserId()));

    [HttpGet, AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Start));

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken, EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        List<Claim>? claims = null;

        // 1) Ana (sistem) yönetici: appsettings / user-secrets içindeki Admin:Username + Admin:Password.
        var user = _config["Admin:Username"];
        var pass = _config["Admin:Password"];
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass) &&
            FixedEquals(model.Username, user) & FixedEquals(model.Password, pass))
        {
            claims = [new Claim(ClaimTypes.Name, user), new Claim(ClaimTypes.GivenName, user)];
        }
        else
        {
            // 2) Sayfa Yönetimi'nden eklenen yöneticiler.
            var admin = await _admins.ValidateCredentialsAsync(model.Username, model.Password);
            if (admin is not null)
            {
                claims =
                [
                    new Claim(ClaimTypes.Name, admin.Username),
                    new Claim(ClaimTypes.GivenName, admin.FullName),
                    new Claim(AdminClaims.AdminUserId, admin.Id.ToString())
                ];
            }
        }

        if (claims is null)
        {
            ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
            return View(model);
        }

        // Rol ayrımı ileride yapılacak; şimdilik tüm yöneticiler "Admin" rolündedir.
        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = model.RememberMe });

        await LogSessionAsync(principal, nameof(Login), "Panele giriş yaptı.");

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction(nameof(Start));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        if (User.Identity?.IsAuthenticated == true)
            await LogSessionAsync(User, nameof(Logout), "Panelden çıkış yaptı.");

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private Task LogSessionAsync(ClaimsPrincipal principal, string action, string description)
    {
        var entry = ActivityLogEntry.For(principal, HttpContext);
        entry.Category = "Oturum";
        entry.Controller = "Account";
        entry.Action = action;
        entry.Description = description;
        return _logs.WriteAsync(entry);
    }

    private static bool FixedEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}