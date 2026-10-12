using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AysuOgel.Entities.Entities;

public class Service
{
    public int Id { get; set; }

    [Display(Name = "Hizmet Adı")]
    [Required(ErrorMessage = "Hizmet adı zorunludur.")]
    [MaxLength(150, ErrorMessage = "En fazla 150 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Kısa Açıklama")]
    [MaxLength(300, ErrorMessage = "En fazla 300 karakter olabilir.")]
    public string? ShortDescription { get; set; }

    [Display(Name = "Açıklama")]
    [MaxLength(3000, ErrorMessage = "En fazla 3000 karakter olabilir.")]
    public string? Description { get; set; }

    [Display(Name = "Görsel Adresi")]
    [MaxLength(1000, ErrorMessage = "En fazla 1000 karakter olabilir.")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Sıra")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
