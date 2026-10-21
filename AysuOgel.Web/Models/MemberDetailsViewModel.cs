using AysuOgel.Entities.Entities;

namespace AysuOgel.Web.Models;

public class MemberDetailsViewModel
{
    public Member Member { get; set; } = null!;

    /// <summary>Yeni paket eklerken seçilebilecek satıştaki paketler.</summary>
    public List<Package> AvailablePackages { get; set; } = [];

    public MemberPackageFormViewModel NewPackage { get; set; } = new();
}
