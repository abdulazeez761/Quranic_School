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
    public bool CornerDecorations { get; set; } = true;
    public bool WatermarkEnabled { get; set; } = true;
    public string WatermarkIcon { get; set; } = "bx-book-open";
    public double WatermarkOpacity { get; set; } = 0.04;
}
