using AysuOgel.Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Controllers;

public class AboutController : Controller
{
    private readonly IAboutPageService _about;
    public AboutController(IAboutPageService about) => _about = about;

    [Route("hakkimizda")]
    public async Task<IActionResult> Index() =>
        View(await _about.GetAsync());
}