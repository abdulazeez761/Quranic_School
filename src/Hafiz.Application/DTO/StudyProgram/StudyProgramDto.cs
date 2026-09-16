using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Domain.Enums;

namespace Hafiz.DTOs.StudyProgram;

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
    public Guid? MatnId { get; set; }
    public string? MatnTitle { get; set; }
    public bool IsActive { get; set; }
    public int ClassesCount { get; set; }
    public DateTime CreatedAt { get; set; }
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

    public Guid? MatnId { get; set; }
}

public class UpdateStudyProgramDto : CreateStudyProgramDto
{
    [Required]
    public Guid Id { get; set; }

    public bool IsActive { get; set; } = true;
}
