namespace AysuOgel.Business.Models;

public interface IPagedResult
{
    int Page { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    int From { get; }
    int To { get; }
}

public class PagedResult<T> : IPagedResult
{
    public PagedResult(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }

    public List<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public int From => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int To => TotalCount == 0 ? 0 : From + Items.Count - 1;
}

public abstract class PagedFilter
{
    public static readonly int[] PageSizes = { 10, 20, 50, 100 };
    public const int DefaultPageSize = 20;

    private int _page = 1;
    private int _pageSize = DefaultPageSize;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>10 / 20 / 50 / 100; başka değer gelirse 20.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = PageSizes.Contains(value) ? value : DefaultPageSize;
    }
}