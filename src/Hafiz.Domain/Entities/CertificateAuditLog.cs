using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.Domain.Entities;

public class CertificateAuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute Institute { get; set; } = null!;

    [Required]
    public Guid EntityId { get; set; }

    [Required]
    [StringLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public CertificateAuditAction Action { get; set; }

    public Guid? PerformedBy { get; set; }

    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    [StringLength(1000)]
    public string? Details { get; set; }
}
