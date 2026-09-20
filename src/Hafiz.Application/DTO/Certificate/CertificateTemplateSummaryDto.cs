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
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
