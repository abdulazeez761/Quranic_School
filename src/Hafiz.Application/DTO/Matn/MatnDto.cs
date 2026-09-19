using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Domain.Enums;

namespace Hafiz.DTOs.Matn;

public class MatnDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public MatnCategory Category { get; set; } = MatnCategory.General;
    public string CategoryName => Category.ToArabic();
    public int? TotalVerses { get; set; }
    public int? TotalChapters { get; set; }
    public MatnUnit DefaultUnit { get; set; }
    public string DefaultUnitName => DefaultUnit switch
    {
        MatnUnit.Verses => "أبيات",
        MatnUnit.Lines => "سطور",
        MatnUnit.Pages => "صفحات",
        MatnUnit.Chapters => "أبواب",
        MatnUnit.Hadiths => "أحاديث",
        _ => "غير محدد"
    };
    public string DefaultUnitSingular => DefaultUnit switch
    {
        MatnUnit.Verses => "بيت",
        MatnUnit.Lines => "سطر",
        MatnUnit.Pages => "صفحة",
        MatnUnit.Chapters => "باب",
        MatnUnit.Hadiths => "حديث",
        _ => "وحدة"
    };
    public bool IsGlobal => InstituteId == null;
    public Guid? InstituteId { get; set; }

    public Guid? StudyProgramId { get; set; }
    public string? StudyProgramName { get; set; }
    public int Order { get; set; } = 1;
    public decimal PassingGrade { get; set; } = 60m;
    public bool IsActive { get; set; } = true;
}

public class CreateMatnDto
{
    [Required(ErrorMessage = "اسم المتن مطلوب.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "اسم المتن يجب ألا يتجاوز 150 حرفاً.")]
    public string Title { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Author { get; set; }

    [Required(ErrorMessage = "الفن / التخصص مطلوب.")]
    public MatnCategory Category { get; set; } = MatnCategory.General;

    [Range(1, 10000, ErrorMessage = "عدد الأبيات يجب أن يكون رقماً موجباً.")]
    public int? TotalVerses { get; set; }

    [Range(1, 500, ErrorMessage = "عدد الأبواب يجب أن يكون رقماً موجباً.")]
    public int? TotalChapters { get; set; }

    public MatnUnit DefaultUnit { get; set; } = MatnUnit.Verses;

    public Guid? StudyProgramId { get; set; }
    public int Order { get; set; } = 1;

    [Range(0, 100, ErrorMessage = "علامة النجاح يجب أن تكون بين 0 و 100.")]
    public decimal PassingGrade { get; set; } = 60m;

    public bool IsActive { get; set; } = true;
}

public class UpdateMatnDto : CreateMatnDto
{
    [Required]
    public Guid Id { get; set; }
}
