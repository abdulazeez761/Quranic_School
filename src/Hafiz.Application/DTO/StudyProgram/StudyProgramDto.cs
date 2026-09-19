using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Hafiz.Domain.Enums;

namespace Hafiz.DTOs.StudyProgram;

public class MatnSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public MatnCategory Category { get; set; }
    public string CategoryName => Category.ToArabic();
    public int? TotalVerses { get; set; }
    public int? TotalChapters { get; set; }
    public MatnUnit DefaultUnit { get; set; }
    public int Order { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public Guid? InstituteId { get; set; }
    public Guid? StudyProgramId { get; set; }
}

public class StudyProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProgramType Type { get; set; }
    public string TypeName => Type switch
    {
        ProgramType.Quran => "تحفيظ القرآن الكريم",
        ProgramType.Matn => "المتون والعلوم الشرعية",
        ProgramType.Sanad => "السند الغيبي",
        ProgramType.Ijaza => "الإجازة القرآنية",
        _ => "أخرى"
    };

    public Guid InstituteId { get; set; }
    public bool IsActive { get; set; }
    public int ClassesCount { get; set; }
    public int MatunsCount => Matuns.Count;
    public List<MatnSummaryDto> Matuns { get; set; } = new();
    public DateTime CreatedAt { get; set; }

    // Convenient title display helper
    public string? MatnTitle => Matuns.Count > 0 
        ? (Matuns.Count == 1 ? Matuns[0].Title : $"{Matuns[0].Title} (+{Matuns.Count - 1} متون)") 
        : null;
}

public class CreateStudyProgramDto
{
    [Required(ErrorMessage = "اسم البرنامج التعليمي مطلوب.")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "اسم البرنامج يجب ألا يتجاوز 150 حرفاً ولا يقل عن 3 أحرف.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "نوع البرنامج مطلوب.")]
    public ProgramType Type { get; set; } = ProgramType.Quran;

    // متون مبدئية للإضافة (اختياري)
    public List<Guid>? InitialMatnIds { get; set; } = new();
}

public class UpdateStudyProgramDto : CreateStudyProgramDto
{
    [Required]
    public Guid Id { get; set; }

    public bool IsActive { get; set; } = true;
}
