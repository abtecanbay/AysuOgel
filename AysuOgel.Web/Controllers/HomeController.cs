using System.Diagnostics;
using AysuOgel.Business.Abstract;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHomePageService _home;
    private readonly IServiceService _services;

    public HomeController(IHomePageService home, IServiceService services)
    {
        _home = home;
        _services = services;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            Content = await _home.GetAsync(),
            Services = (await _services.GetActiveAsync()).Take(4).ToList()
        };
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}