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

public class HeroSection
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? ButtonText { get; set; }
    public string? ButtonUrl { get; set; }
}

public class ContentBlock
{
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
    public BlockLayout Layout { get; set; } = BlockLayout.TextOnly;
    public List<ImageItem> Images { get; set; } = new();
}

public class HomePageContent
{
    public HeroSection Hero { get; set; } = new();
    public List<ContentBlock> Blocks { get; set; } = new();
}