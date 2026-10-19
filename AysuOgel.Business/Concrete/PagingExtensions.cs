using AysuOgel.Business.Models;
using Microsoft.EntityFrameworkCore;

namespace AysuOgel.Business.Concrete;

internal static class PagingExtensions
{
    /// <summary>Sıralama sorguda önceden yapılmış olmalı.</summary>
    public static async Task<PagedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        if (page > totalPages) page = totalPages;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<T>(items, page, pageSize, total);
    }
}