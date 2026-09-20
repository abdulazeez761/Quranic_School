using System;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateTemplateSummaryDto
{
    public Guid Id { get; set; }
    public Guid InstituteId { get; set; }
    public string InstituteName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CertificateType Type { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; }
    public int CurrentVersion { get; set; }
    public int CertificatesIssuedCount { get; set; }

    // Theme colors of the current version, used to tint the template card in the admin list.
    public string PrimaryColor { get; set; } = "#C59B27";
    public string SecondaryColor { get; set; } = "#064E3B";
    public string AccentColor { get; set; } = "#DFBA69";
    public string BackgroundColor { get; set; } = "#FDFBF7";

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
