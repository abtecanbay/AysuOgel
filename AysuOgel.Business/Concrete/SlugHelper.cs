using System.Text;

namespace AysuOgel.Business.Concrete;

public static class SlugHelper
{
    public const int MaxLength = 80;

    /// <summary>"Profesyonel Cilt Bakımı" → "profesyonel-cilt-bakimi"</summary>
    public static string ToSlug(string? text)
    {
        var sb = new StringBuilder();
        foreach (var ch in (text ?? string.Empty).Trim().ToLowerInvariant())
        {
            if (ch == '̇') continue;   // "İ".ToLowerInvariant() → "i" + birleşik nokta

            var c = ch switch
            {
                'ç' => 'c', 'ğ' => 'g', 'ı' => 'i', 'ö' => 'o', 'ş' => 's', 'ü' => 'u',
                'â' => 'a', 'î' => 'i', 'û' => 'u',
                _ => ch
            };

            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
                sb.Append(c);
            else if (sb.Length > 0 && sb[^1] != '-')
                sb.Append('-');
        }

        var slug = sb.ToString().Trim('-');
        if (slug.Length > MaxLength) slug = slug[..MaxLength].TrimEnd('-');
        return slug.Length == 0 ? "blok" : slug;
    }
}
