using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class CampaignsPageViewModel
{
    public CampaignPageSettings PageSettings { get; set; } = new();
    public List<Campaign> Campaigns { get; set; } = new();
}