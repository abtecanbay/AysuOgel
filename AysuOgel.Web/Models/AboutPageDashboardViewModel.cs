using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class AboutPageDashboardViewModel
{
    public AboutPageSettings Settings { get; set; } = new();
    public List<AboutBlock> Blocks { get; set; } = [];
}
