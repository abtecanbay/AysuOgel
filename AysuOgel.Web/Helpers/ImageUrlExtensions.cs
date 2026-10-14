using Microsoft.AspNetCore.Mvc;

namespace AysuOgel.Web.Helpers;

public static class ImageUrlExtensions
{
    /// <summary>
    /// "https://cdn.../a.jpg" → olduğu gibi (CDN).
    /// "a.jpg", "/a.jpg", "uploads/a.jpg" → /uploads/a.jpg (sunucudaki dosya).
    /// </summary>
    public static string ImageUrl(this IUrlHelper url, string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return string.Empty;
        path = path.Trim();

        if (Uri.TryCreate(path, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp))
            return path;

        var file = path.TrimStart('~', '/');
        if (!file.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            file = "uploads/" + file;

        return url.Content("~/" + file);
    }
}