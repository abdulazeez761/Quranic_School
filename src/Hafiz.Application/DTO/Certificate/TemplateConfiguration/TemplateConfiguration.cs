using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public class TemplateConfiguration
{
    public LayoutConfig Layout { get; set; } = new();
    public ThemeConfig Theme { get; set; } = new();
    public TypographyConfig Typography { get; set; } = new();
    public List<SectionConfig> Sections { get; set; } = new();
    public TokenConfig Tokens { get; set; } = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, JsonOptions);
    }

    public static TemplateConfiguration FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new TemplateConfiguration();

        try
        {
            return JsonSerializer.Deserialize<TemplateConfiguration>(json, JsonOptions) ?? new TemplateConfiguration();
        }
        catch
        {
            return new TemplateConfiguration();
        }
    }
}
