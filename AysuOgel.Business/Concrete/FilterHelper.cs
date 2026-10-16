namespace AysuOgel.Business.Concrete;

internal static class FilterHelper
{
    public static string? Clean(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();

    /// <summary>Telefon aramasında yalnızca rakam ve + kalır.</summary>
    public static string? PhoneDigits(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var digits = new string(text.Where(c => char.IsDigit(c) || c == '+').ToArray());
        return digits.Length == 0 ? null : digits;
    }

    /// <summary>[from, toExclusive) aralığı üretir; from > to ise yer değiştirir.</summary>
    public static (DateTime? From, DateTime? ToExclusive) DateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            (from, to) = (to, from);

        return (
            from?.ToDateTime(TimeOnly.MinValue),
            to?.AddDays(1).ToDateTime(TimeOnly.MinValue));
    }
}