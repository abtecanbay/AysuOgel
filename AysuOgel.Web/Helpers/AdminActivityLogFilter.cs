using System.Security.Claims;
using AysuOgel.Business.Abstract;
using AysuOgel.Entities.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AysuOgel.Web.Helpers;

/// <summary>
/// Admin alanında başarıyla tamamlanan her değiştirici isteği (POST) işlem kaydına yazar.
/// Böylece yeni bir sayfa/aksiyon eklendiğinde de ayrıca kod yazmadan loglanır.
/// <para>
/// Açıklama, controller'ın TempData'ya yazdığı başarı/hata mesajından alınır ("Kampanya güncellendi." gibi);
/// mesaj yoksa aksiyon adından üretilir. İşlem sonunda yönlendirme yapılmadıysa (ör. form doğrulama hatasıyla
/// aynı sayfa tekrar gösterildiyse) hiçbir şey değişmemiştir ve kayıt yazılmaz.
/// </para>
/// </summary>
public class AdminActivityLogFilter : IAsyncActionFilter
{
    /// <summary>Giriş/çıkış AccountController içinde ayrıca loglanır.</summary>
    private static readonly HashSet<string> Skip = new(StringComparer.OrdinalIgnoreCase) { "Account", "ActivityLog" };

    /// <summary>Formdan kaydın adını almak için bakılan alanlar (sırayla). Şifre vb. alanlar asla okunmaz.</summary>
    private static readonly string[] TargetFields = ["FullName", "Name", "Title", "Username", "Subject"];

    private static readonly Dictionary<string, string> ActionLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Create"] = "Yeni kayıt eklendi.",
        ["Edit"] = "Kayıt güncellendi.",
        ["Delete"] = "Kayıt silindi.",
        ["Settings"] = "Sayfa ayarları güncellendi.",
        ["PageSettings"] = "Sayfa ayarları güncellendi.",
        ["SetStatus"] = "Durum değiştirildi.",
        ["Reply"] = "Mesaja yanıt verildi.",
        ["CreateAdmin"] = "Yönetici eklendi.",
        ["EditAdmin"] = "Yönetici güncellendi.",
        ["DeleteAdmin"] = "Yönetici silindi.",
        ["AddPackage"] = "Üyeye paket eklendi.",
        ["EditPackage"] = "Üye paketi güncellendi.",
        ["DeletePackage"] = "Üye paketi silindi.",
        ["AddSession"] = "Seans eklendi.",
        ["EditSession"] = "Seans güncellendi.",
        ["DeleteSession"] = "Seans silindi.",
        ["SetSessionStatus"] = "Seans durumu değiştirildi."
    };

    private readonly IActivityLogService _logs;
    private readonly IAdminPageService _pages;

    public AdminActivityLogFilter(IActivityLogService logs, IAdminPageService pages)
    {
        _logs = logs;
        _pages = pages;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        var http = context.HttpContext;
        if (HttpMethods.IsGet(http.Request.Method) || HttpMethods.IsHead(http.Request.Method)) return;

        var route = context.RouteData.Values;
        if (!string.Equals(route["area"]?.ToString(), "Admin", StringComparison.OrdinalIgnoreCase)) return;

        var controllerName = route["controller"]?.ToString() ?? string.Empty;
        var actionName = route["action"]?.ToString() ?? string.Empty;
        if (Skip.Contains(controllerName)) return;
        if (http.User.Identity?.IsAuthenticated != true) return;

        // Hata fırladıysa veya işlem yönlendirmeyle bitmediyse (doğrulama hatası, 404, 400...) kayıt yok.
        if (executed.Exception is not null && !executed.ExceptionHandled) return;
        if (executed.Result is not (RedirectToActionResult or RedirectResult or LocalRedirectResult or RedirectToRouteResult)) return;

        var (description, success) = ReadOutcome(context.Controller as Controller, actionName);

        var page = await _pages.FindByControllerAsync(controllerName);
        var entry = ActivityLogEntry.For(http.User, http);
        entry.Category = page?.Title ?? controllerName;
        entry.Controller = controllerName;
        entry.Action = actionName;
        entry.EntityId = route["id"]?.ToString();
        entry.Target = await ReadTargetAsync(http.Request);
        entry.Description = description;
        entry.IsSuccess = success;

        await _logs.WriteAsync(entry);
    }

    /// <summary>TempData'daki "...Success" / "...Error" mesajını okur (Peek: mesaj sayfada yine gösterilir).</summary>
    private static (string Description, bool Success) ReadOutcome(Controller? controller, string action)
    {
        var tempData = controller?.TempData;
        if (tempData is not null)
        {
            foreach (var key in tempData.Keys.ToList())
            {
                if (key.EndsWith("Error", StringComparison.OrdinalIgnoreCase) && tempData.Peek(key) is string error)
                    return (error, false);
            }

            foreach (var key in tempData.Keys.ToList())
            {
                if (key.EndsWith("Success", StringComparison.OrdinalIgnoreCase) && tempData.Peek(key) is string ok)
                    return (ok, true);
            }

            // Mesajlar sayfası: ReplyMessage + ReplyLevel (success / warning / danger).
            if (tempData.Peek("ReplyMessage") is string reply)
                return (reply, !string.Equals(tempData.Peek("ReplyLevel") as string, "danger", StringComparison.OrdinalIgnoreCase));
        }

        return (ActionLabels.TryGetValue(action, out var label) ? label : $"{action} işlemi yapıldı.", true);
    }

    private static async Task<string?> ReadTargetAsync(HttpRequest request)
    {
        if (!request.HasFormContentType) return null;

        try
        {
            var form = await request.ReadFormAsync();
            foreach (var field in TargetFields)
            {
                var value = form[field].ToString();
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            var fullName = $"{form["FirstName"]} {form["LastName"]}".Trim();
            return fullName.Length > 0 ? fullName : null;
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }
}

/// <summary>Oturumdaki yöneticiden işlem kaydı oluşturur.</summary>
public static class ActivityLogEntry
{
    public static AdminActivityLog For(ClaimsPrincipal user, HttpContext http)
    {
        var adminUserId = user.GetAdminUserId();
        var username = user.Identity?.Name ?? user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

        return new AdminActivityLog
        {
            AdminUserId = adminUserId,
            IsSystemAdmin = adminUserId is null,
            Username = username,
            FullName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? username,
            IpAddress = http.Connection.RemoteIpAddress?.ToString()
        };
    }
}
