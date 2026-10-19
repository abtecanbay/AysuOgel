using AysuOgel.Business.Models;
using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class AvailabilityListViewModel : PagedFilter, IPagedResult
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<AvailabilityWindow> Items { get; set; } = [];
    public int TotalCount { get; set; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int To => TotalCount == 0 ? 0 : From + Items.Count - 1;
}