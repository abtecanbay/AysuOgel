using AysuOgel.Business.Abstract;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Controllers;

[Route("kampanyalar")]
public class CampaignsController : Controller
{
    private readonly ICampaignService _campaigns;

    public CampaignsController(ICampaignService campaigns) => _campaigns = campaigns;

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = new CampaignsPageViewModel
        {
            PageSettings = await _campaigns.GetPageSettingsAsync(),
            Campaigns = await _campaigns.GetCurrentAsync()
        };

        return View(model);
    }
}