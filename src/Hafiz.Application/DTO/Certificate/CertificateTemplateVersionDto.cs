using System;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateTemplateVersionDto
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public int VersionNumber { get; set; }
    public string ConfigurationJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public string CreatedAtFormatted { get; set; } = string.Empty;
    public Guid? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public int CertificatesIssuedCount { get; set; }
    public bool IsCurrent { get; set; }

    // Summary metadata extracted from configuration for quick display
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? AccentColor { get; set; }
    public string? BackgroundColor { get; set; }
    public string? Orientation { get; set; }
    public string? PageSize { get; set; }
    public int SectionCount { get; set; }
}
