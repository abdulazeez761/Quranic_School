namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public class LayoutConfig
{
    public string PageSize { get; set; } = "A4"; // A4, Letter
    public string Orientation { get; set; } = "landscape"; // landscape, portrait
    public MarginsConfig Margins { get; set; } = new();
}

public class MarginsConfig
{
    public int Top { get; set; } = 10;
    public int Right { get; set; } = 12;
    public int Bottom { get; set; } = 10;
    public int Left { get; set; } = 12;
}
