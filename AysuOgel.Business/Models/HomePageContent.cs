namespace AysuOgel.Business.Models;

public enum BlockLayout
{
    TextOnly,
    ImageLeft,
    ImageRight,
    Gallery
}

public class ImageItem
{
    public string Url { get; set; } = string.Empty;   // https://cdn... veya ornek.jpg
    public string Alt { get; set; } = string.Empty;
    public string? Caption { get; set; }
}

public class BannerSlide
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Badge { get; set; }          // "Kampanya", "Yeni" gibi küçük etiket
    public string ImageUrl { get; set; } = string.Empty;
    public string? ButtonText { get; set; }
    public string? LinkUrl { get; set; }        // "#post-hakkimizda" veya "/hizmetler"
}

public class ContentBlock
{
    public string Slug { get; set; } = string.Empty;   // sayfa içi bağlantı: #post-{Slug}
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
    public BlockLayout Layout { get; set; } = BlockLayout.TextOnly;
    public List<ImageItem> Images { get; set; } = new();
}

public class HomePageContent
{
    public List<BannerSlide> Banners { get; set; } = new();
    public List<ContentBlock> Blocks { get; set; } = new();
}