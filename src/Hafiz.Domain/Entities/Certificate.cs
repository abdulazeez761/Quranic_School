using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.Domain.Entities;

public class Certificate : ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute Institute { get; set; } = null!;

    [Required]
    public Guid StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string CertificateNumber { get; set; } = string.Empty;

    [Required]
    public CertificateType Type { get; set; }

    [Required]
    public Guid TemplateId { get; set; }

    [ForeignKey(nameof(TemplateId))]
    public CertificateTemplate Template { get; set; } = null!;

    [Required]
    public Guid TemplateVersionId { get; set; }

    [ForeignKey(nameof(TemplateVersionId))]
    public CertificateTemplateVersion TemplateVersion { get; set; } = null!;

    /// <summary>
    /// Frozen snapshot of all certificate data at issuance time.
    /// </summary>
    [Required]
    [Column(TypeName = "nvarchar(max)")]
    public string CertificateDataJson { get; set; } = "{}";

    /// <summary>
    /// Optional reference to the source achievement record (e.g., StudentMatnProgress.Id).
    /// </summary>
    public Guid? SourceEntityId { get; set; }

    [Required]
    [StringLength(64)]
    public string VerificationToken { get; set; } = string.Empty;

    [Required]
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public Guid? IssuedBy { get; set; }

    public DateTime? RevokedAt { get; set; }
    public Guid? RevokedBy { get; set; }

    [StringLength(500)]
    public string? RevokeReason { get; set; }

    // ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
