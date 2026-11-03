using AysuOgel.Business.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AysuOgel.Web.Helpers;

/// <summary>
/// Admin alanındaki her isteği, oturumdaki yöneticinin o sayfaya (controller'a) yetkisi olup
/// olmadığına göre denetler. Ana (sistem) yönetici tüm sayfalara erişir.
/// </summary>
public class AdminPageAccessFilter : IAsyncAuthorizationFilter
{
    /// <summary>Yetki denetiminden muaf controller'lar (giriş/çıkış, yetkisiz sayfası).</summary>
    private static readonly HashSet<string> Exempt = new(StringComparer.OrdinalIgnoreCase) { "Account" };

    private readonly IAdminPageService _pages;
    public AdminPageAccessFilter(IAdminPageService pages) => _pages = pages;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var route = context.RouteData.Values;
        if (!string.Equals(route["area"]?.ToString(), "Admin", StringComparison.OrdinalIgnoreCase)) return;

        var controller = route["controller"]?.ToString();
        if (string.IsNullOrEmpty(controller) || Exempt.Contains(controller)) return;

        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) return;

        // Oturum yoksa [Authorize] giriş sayfasına yönlendirir.
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true) return;

        if (await _pages.CanAccessAsync(user.GetAdminUserId(), controller)) return;

        context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "Admin" });
    }
}
