namespace AysuOgel.Business.Models;

public class ProgramStep
{
    public string Title { get; set; } = string.Empty;
    public string? Text { get; set; }
}

public class ComparisonRow
{
    public string Label { get; set; } = string.Empty;
    public string Left { get; set; } = string.Empty;
    public string Right { get; set; } = string.Empty;
}

public class ComparisonSection
{
    public string Title { get; set; } = string.Empty;
    public string? Intro { get; set; }
    public string LeftTitle { get; set; } = string.Empty;
    public string RightTitle { get; set; } = string.Empty;
    public List<ComparisonRow> Rows { get; set; } = new();
}

public class TransformationCard
{
    public string Name { get; set; } = string.Empty;       // "Üyemiz A.K."
    public string? Program { get; set; }                   // "Vaccu Active"
    public string? Duration { get; set; }                  // "8 hafta"
    public string? Note { get; set; }
    public string? BeforeImageUrl { get; set; }            // CDN adresi veya uploads dosyası
    public string? AfterImageUrl { get; set; }
}

public class ServicesPageContent
{
    public string Title { get; set; } = string.Empty;
    public string? Intro { get; set; }

    public string StepsTitle { get; set; } = string.Empty;
    public List<ProgramStep> Steps { get; set; } = new();

    public ComparisonSection Comparison { get; set; } = new();

    public string TransformationsTitle { get; set; } = string.Empty;
    public string? TransformationsNote { get; set; }
    public List<TransformationCard> Transformations { get; set; } = new();
}