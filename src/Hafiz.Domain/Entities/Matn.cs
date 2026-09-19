using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Enums;

namespace Hafiz.Domain.Entities;

public class Matn : ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required(ErrorMessage = "اسم المتن مطلوب.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "اسم المتن يجب ألا يتجاوز 150 حرفاً ولا يقل عن حرفين.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Author { get; set; }

    [Required]
    public MatnCategory Category { get; set; } = MatnCategory.General;

    public int? TotalVerses { get; set; }
    public int? TotalChapters { get; set; }

    public MatnUnit DefaultUnit { get; set; } = MatnUnit.Verses;

    // البرنامج العلمي التابع له هذا المتن (اختياري)
    public Guid? StudyProgramId { get; set; }

    [ForeignKey(nameof(StudyProgramId))]
    public StudyProgram? StudyProgram { get; set; }

    // ترتيب المتن داخل البرنامج العلمي (يبدأ من 1)
    public int Order { get; set; } = 1;

    // علامة النجاح في الاختبار (الحد الأدنى للدرجة بالنسبة المئوية)
    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "علامة النجاح يجب أن تكون بين 0 و 100.")]
    public decimal PassingGrade { get; set; } = 60m;

    // حالة تفعيل المتن للواجبات والتسميع
    public bool IsActive { get; set; } = true;

    // إذا كان المعهد null، فهو متن عام لكل المعاهد (Global System Library)
    public Guid? InstituteId { get; set; }

    [ForeignKey(nameof(InstituteId))]
    public Institute? Institute { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Implementation of ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
