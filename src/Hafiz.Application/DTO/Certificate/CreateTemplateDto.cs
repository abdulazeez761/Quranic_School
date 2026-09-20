using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CreateTemplateDto
{
    public Guid InstituteId { get; set; }

    [Required(ErrorMessage = "اسم القالب مطلوب.")]
    [StringLength(150, ErrorMessage = "اسم القالب لا يتجاوز 150 حرفاً.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public CertificateType Type { get; set; } = CertificateType.Quran;

    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Optional initial JSON. If null/empty, default config for the Type will be seeded.
    /// </summary>
    public string? InitialConfigurationJson { get; set; }
}

public class UpdateTemplateConfigDto
{
    [Required]
    public Guid TemplateId { get; set; }

    [Required(ErrorMessage = "اسم القالب مطلوب.")]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "إعدادات القالب مطلوبة.")]
    public string ConfigurationJson { get; set; } = "{}";
}
