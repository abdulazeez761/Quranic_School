namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public class ThemeConfig
{
    public string Preset { get; set; } = "classic_gold";
    public string PrimaryColor { get; set; } = "#C59B27";
    public string SecondaryColor { get; set; } = "#064E3B";
    public string AccentColor { get; set; } = "#DFBA69";
    public string TextColor { get; set; } = "#1C1917";
    public string BackgroundColor { get; set; } = "#FDFBF7";
    public string BorderColor { get; set; } = "#C59B27";
    public string BorderStyle { get; set; } = "double";
    public int BorderWidth { get; set; } = 3;

    /// <summary>
    /// Ornament style for the inner frame corners. "auto" derives from the legacy
    /// <see cref="CornerDecorations"/> flag so templates saved before this option existed render unchanged.
    /// One of: auto, none, simple, arabesque, girih, floral, medallion.
    /// </summary>
    public string CornerStyle { get; set; } = "auto";

    /// <summary>Edge length of a corner ornament in pixels.</summary>
    public int CornerSize { get; set; } = 56;

    /// <summary>"lines" is the thin border rule; "ornate" adds the wide ornamental band around it.</summary>
    public string FrameStyle { get; set; } = "lines";

    /// <summary>Repeating background motif. One of: none, paper, grid, arabesque, geometric, damask, stars.</summary>
    public string BackgroundPattern { get; set; } = "none";

    /// <summary>Which theme colour tints the background pattern: primary, secondary or accent.</summary>
    public string BackgroundPatternColor { get; set; } = "primary";

    public double BackgroundPatternOpacity { get; set; } = 0.06;

    /// <summary>Legacy on/off corner flag, superseded by <see cref="CornerStyle"/> but still honoured when that is "auto".</summary>
    public bool CornerDecorations { get; set; } = true;
    public bool WatermarkEnabled { get; set; } = true;
    public string WatermarkIcon { get; set; } = "bx-book-open";
    public double WatermarkOpacity { get; set; } = 0.04;
}
