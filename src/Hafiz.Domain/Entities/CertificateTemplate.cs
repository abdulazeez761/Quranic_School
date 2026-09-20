using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Enums;

namespace Hafiz.Domain.Entities;

public class CertificateTemplate : ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute Institute { get; set; } = null!;

    [Required(ErrorMessage = "اسم القالب مطلوب.")]
    [StringLength(150, ErrorMessage = "اسم القالب لا يتجاوز 150 حرفاً.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public CertificateType Type { get; set; }

    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int CurrentVersion { get; set; } = 1;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<CertificateTemplateVersion> Versions { get; set; } = new List<CertificateTemplateVersion>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    // ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
