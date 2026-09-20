using System;
using Hafiz.Application.DTO.Certificate.TemplateConfiguration;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateTemplateDto
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
    public Guid CurrentVersionId { get; set; }
    public TemplateConfiguration.TemplateConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
