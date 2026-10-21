using System.Security.Claims;
using AysuOgel.Business.Abstract;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AysuOgel.Web.Controllers;

/// <summary>Üyelerin e-posta ve şifreyle giriş yapıp paket / seans bilgilerini gördüğü sayfa.</summary>
public class MemberSessionsController : Controller
{
    private readonly IMemberService _members;
    public MemberSessionsController(IMemberService members) => _members = members;

    // /seans-sorgula
    [HttpGet("seans-sorgula")]
    public async Task<IActionResult> Login()
    {
        var auth = await HttpContext.AuthenticateAsync(MemberAuth.Scheme);
        if (auth.Succeeded) return RedirectToAction(nameof(Index));

        return View(new MemberLoginViewModel());
    }

    [HttpPost("seans-sorgula"), ValidateAntiForgeryToken, EnableRateLimiting("member-login")]
    public async Task<IActionResult> Login(MemberLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var member = await _members.VerifyCredentialsAsync(model.Email, model.Password);
        if (member is null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı.");
            model.Password = string.Empty;
            return View(model);
        }

        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{member.FirstName} {member.LastName}"),
                new Claim(MemberAuth.PasswordStampClaim, MemberAuth.PasswordStamp(member.PasswordHash))
            },
            MemberAuth.Scheme);

        await HttpContext.SignInAsync(MemberAuth.Scheme, new ClaimsPrincipal(identity));
        return RedirectToAction(nameof(Index));
    }

    // /seans-sorgula/seanslarim
    [HttpGet("seans-sorgula/seanslarim")]
    [Authorize(AuthenticationSchemes = MemberAuth.Scheme)]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Index()
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var member = int.TryParse(idValue, out var id) ? await _members.GetByIdAsync(id) : null;

        // Üye silindiyse ya da şifresi değiştiyse oturum geçersizdir.
        if (member is null
            || User.FindFirstValue(MemberAuth.PasswordStampClaim) != MemberAuth.PasswordStamp(member.PasswordHash))
        {
            await HttpContext.SignOutAsync(MemberAuth.Scheme);
            return RedirectToAction(nameof(Login));
        }

        member.Packages = member.Packages
            .OrderBy(p => p.Status)                 // devam edenler önce
            .ThenByDescending(p => p.StartDate)
            .ToList();
        return View(member);
    }

    // Form, üye kimliğiyle üretilen anti-forgery token taşır; doğrulama da aynı kimlikle yapılmalı.
    [HttpPost("seans-sorgula/cikis"), ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = MemberAuth.Scheme)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(MemberAuth.Scheme);
        return RedirectToAction(nameof(Login));
    }
}
