using AysuOgel.Business.Abstract;
using AysuOgel.Business.Models;
using AysuOgel.Web.Helpers;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Areas.Admin.Controllers;

/// <summary>
/// İşlem geçmişi. Ana (sistem) yönetici tüm yöneticilerin yaptığı işlemleri görür ve yöneticiye göre
/// filtreleyebilir; panelden eklenen yöneticiler yalnızca kendi geçmişlerini görür.
/// Sayfa yetkisi gerektirmez (AdminPageAccessFilter'da muaf); kısıtlama burada, sorgu düzeyinde yapılır.
/// </summary>
[Area("Admin")]
[Authorize]
public class ActivityLogController : Controller
{
    private readonly IActivityLogService _logs;
    private readonly IAdminUserService _admins;
    private readonly IConfiguration _config;

    public ActivityLogController(IActivityLogService logs, IAdminUserService admins, IConfiguration config)
    {
        _logs = logs;
        _admins = admins;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> Index(ActivityLogFilter filter)
    {
        var adminUserId = User.GetAdminUserId();
        var isSystemAdmin = adminUserId is null;

        // Diğer yöneticiler başka birinin kayıtlarını URL ile de isteyemez.
        if (!isSystemAdmin) filter.Admin = null;

        return View(new ActivityLogViewModel
        {
            Filter = filter,
            IsSystemAdmin = isSystemAdmin,
            Logs = await _logs.GetPagedAsync(filter, adminUserId),
            Categories = await _logs.GetCategoriesAsync(adminUserId),
            Admins = isSystemAdmin ? await _admins.GetAllAsync() : [],
            SystemAdminUsername = isSystemAdmin ? _config["Admin:Username"] : null
        });
    }
}
