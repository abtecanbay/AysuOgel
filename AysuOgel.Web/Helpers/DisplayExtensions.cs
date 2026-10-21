using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

namespace AysuOgel.Web.Helpers;

public static class DisplayExtensions
{
    public static string DisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        return member?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? value.ToString();
    }

    private static readonly TimeZoneInfo Turkey = FindTurkey();

    private static TimeZoneInfo FindTurkey()
    {
        foreach (var id in new[] { "Europe/Istanbul", "Turkey Standard Time" })
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Local;
    }

    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    /// <summary>Sunucu kültüründen bağımsız "1.500 ₺" / "1.500,50 ₺" biçimi.</summary>
    public static string ToLira(this decimal amount) =>
        amount.ToString(amount % 1 == 0 ? "N0" : "N2", TurkishCulture) + " ₺";

    public static DateTime ToTurkeyTime(this DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Turkey);
}