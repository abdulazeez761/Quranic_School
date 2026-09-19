using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.Domain.Entities;

public class MatnAssignment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "معرف الطالب مطلوب.")]
    public Guid StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [Required(ErrorMessage = "معرف الحلقة مطلوب.")]
    public Guid ClassId { get; set; }

    [ForeignKey(nameof(ClassId))]
    public Class Class { get; set; } = null!;

    // المتن المستهدف بالتسميع
    public Guid? MatnId { get; set; }

    [ForeignKey(nameof(MatnId))]
    public Matn? Matn { get; set; }

    // نموذج الأداء: (حفظ = 1، مراجعة = 2، مدارسة = 3)
    [Required]
    public MatnPerformanceType PerformanceType { get; set; } = MatnPerformanceType.Memorization;

    // وحدة القياس: (أبيات، سطور، صفحات، أبواب، أحاديث)
    [Required]
    public MatnUnit Unit { get; set; } = MatnUnit.Verses;

    // الكمية المنجزة
    [Column(TypeName = "decimal(6,2)")]
    [Range(0.01, 9999.99, ErrorMessage = "الكمية يجب أن تكون أكبر من 0.")]
    public decimal? Amount { get; set; }

    // تفاصيل موضع التسميع (اختيارية للدقة)
    [StringLength(150)]
    public string? ChapterName { get; set; }

    public int? FromNumber { get; set; }
    public int? ToNumber { get; set; }

    // التقييم وحالة التسميع
    [Required]
    public AssignmentStatus Status { get; set; } = AssignmentStatus.notSet;

    public bool IsCompleted { get; set; } = false;
    public bool IsUpcoming { get; set; } = false;

    [Required]
    public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

    [StringLength(500, ErrorMessage = "الملاحظات لا تتجاوز 500 حرف.")]
    public string? Note { get; set; }
}
