using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class CampaignsDashboardViewModel
{
    public List<Campaign> Campaigns { get; set; } = [];
    public CampaignPageSettings PageSettings { get; set; } = new();
}