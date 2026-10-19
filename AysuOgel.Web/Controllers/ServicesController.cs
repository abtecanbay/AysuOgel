using AysuOgel.Business.Abstract;
using AysuOgel.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Controllers;

public class ServicesController : Controller
{
    private readonly IServiceService _services;
    private readonly IServicesPageService _page;

    public ServicesController(IServiceService services, IServicesPageService page)
    {
        _services = services;
        _page = page;
    }

    [Route("hizmetler")]
    public async Task<IActionResult> Index() =>
        View(new ServicesPageViewModel
        {
            Content = await _page.GetAsync(),
            Services = await _services.GetActiveAsync()
        });

    [HttpGet("hizmetler/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var service = await _services.GetByIdAsync(id);
        if (service is null || !service.IsActive) return NotFound();
        return View(service);
    }
}