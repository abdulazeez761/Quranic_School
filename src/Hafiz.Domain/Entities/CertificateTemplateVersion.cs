using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hafiz.Domain.Entities;

public class CertificateTemplateVersion
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TemplateId { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public CertificateTemplate Template { get; set; } = null!;

    [Required]
    public int VersionNumber { get; set; }

    /// <summary>
    /// JSON configuration of layout, theme, typography, sections, and tokens.
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string ConfigurationJson { get; set; } = "{}";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
}
