using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class ServicesPageViewModel
{
    public ServicesPageContent Content { get; set; } = new();
    public List<Service> Services { get; set; } = new();
    public List<SuccessStory> Successes { get; set; } = new();
}

public record BeforeAfterFrame(string? Url, string Label, string Alt, bool IsAfter);