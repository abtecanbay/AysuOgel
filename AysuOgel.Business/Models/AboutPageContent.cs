namespace AysuOgel.Business.Models;

public class AboutPageContent
{
    public string Title { get; set; } = string.Empty;
    public string? Intro { get; set; }
    public List<ContentBlock> Blocks { get; set; } = new();
}