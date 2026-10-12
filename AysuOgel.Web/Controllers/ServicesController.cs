using AysuOgel.Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Controllers;

public class ServicesController : Controller
{
    private readonly IServiceService _services;
    public ServicesController(IServiceService services) => _services = services;

    [Route("hizmetler")]
    public async Task<IActionResult> Index() =>
        View(await _services.GetActiveAsync());
}