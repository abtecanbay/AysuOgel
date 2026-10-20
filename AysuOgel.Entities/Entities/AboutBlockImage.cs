using System.ComponentModel.DataAnnotations;

namespace AysuOgel.Entities.Entities;

public class AboutBlockImage
{
    public int Id { get; set; }
    public int AboutBlockId { get; set; }

    [MaxLength(1000)]
    public string Url { get; set; } = string.Empty;

    [Display(Name = "Alternatif metin")]
    [MaxLength(200, ErrorMessage = "Alternatif metin en fazla 200 karakter olabilir.")]
    public string? Alt { get; set; }

    [Display(Name = "Açıklama")]
    [MaxLength(300, ErrorMessage = "Açıklama en fazla 300 karakter olabilir.")]
    public string? Caption { get; set; }

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    public AboutBlock? AboutBlock { get; set; }
}
