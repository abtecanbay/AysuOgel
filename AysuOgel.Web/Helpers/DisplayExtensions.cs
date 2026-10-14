using System.ComponentModel.DataAnnotations;
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

    public static DateTime ToTurkeyTime(this DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Turkey);
}