using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.Domain.Entities;

public class StudyProgram : ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "اسم البرنامج التعليمي مطلوب.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "اسم البرنامج يجب ألا يتجاوز 150 حرفاً ولا يقل عن 3 أحرف.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public ProgramType Type { get; set; } = ProgramType.Quran; // الافتراضي دائماً قرآن

    // المتون التابعة لهذا البرنامج (في حال كان نوع البرنامج متون علمية)
    public ICollection<Matn> Matuns { get; set; } = new List<Matn>();

    [Required]
    public Guid InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute Institute { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // الحلقات التشغيلية التابعة لهذا البرنامج
    public ICollection<Class> Classes { get; set; } = new List<Class>();

    // Implementation of ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
