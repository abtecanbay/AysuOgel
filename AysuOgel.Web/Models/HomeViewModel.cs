using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class HomeViewModel
{
    public HomePageContent Content { get; set; } = new();
    public List<Service> Services { get; set; } = new();
}