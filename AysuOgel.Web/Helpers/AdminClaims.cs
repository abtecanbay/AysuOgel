namespace AysuOgel.Web.Helpers;

/// <summary>Yönetici oturumuna eklenen özel claim adları.</summary>
public static class AdminClaims
{
    /// <summary>Veritabanındaki AdminUser.Id. Ana (sistem) yöneticide bu claim bulunmaz.</summary>
    public const string AdminUserId = "admin_user_id";

    public static int? GetAdminUserId(this System.Security.Claims.ClaimsPrincipal user) =>
        int.TryParse(user.FindFirst(AdminUserId)?.Value, out var id) ? id : null;
}
