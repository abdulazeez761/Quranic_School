using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Domain.Enums;

namespace Hafiz.DTOs.Matn;

public class StudentMatnProgressDto
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;

    public Guid MatnId { get; set; }
    public string MatnTitle { get; set; } = string.Empty;
    public string? MatnAuthor { get; set; }
    public MatnCategory MatnCategory { get; set; }
    public string MatnCategoryName => MatnCategory.ToArabic();
    public int MatnOrder { get; set; }

    public Guid? StudyProgramId { get; set; }
    public string? StudyProgramName { get; set; }

    public StudyStatus StudyStatus { get; set; } = StudyStatus.NotStarted;
    public string StudyStatusName => StudyStatus.ToArabic();

    public MemorizationStatus MemorizationStatus { get; set; } = MemorizationStatus.NotStarted;
    public string MemorizationStatusName => MemorizationStatus.ToArabic();

    public ExamStatus ExamStatus { get; set; } = ExamStatus.NotTested;
    public string ExamStatusName => ExamStatus.ToArabic();

    public decimal? Score { get; set; }
    public DateTime? ExamDate { get; set; }
    public string? TeacherNotes { get; set; }

    public DateTime? StudyStartedAt { get; set; }
    public DateTime? StudyCompletedAt { get; set; }
    public DateTime? MemorizationCompletedAt { get; set; }

    public Guid? LastUpdatedByTeacherId { get; set; }
    public string? LastUpdatedByTeacherName { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class CreateStudentMatnProgressDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid MatnId { get; set; }

    public StudyStatus StudyStatus { get; set; } = StudyStatus.NotStarted;
    public MemorizationStatus MemorizationStatus { get; set; } = MemorizationStatus.NotStarted;
    public ExamStatus ExamStatus { get; set; } = ExamStatus.NotTested;

    [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين 0 و 100.")]
    public decimal? Score { get; set; }

    public DateTime? ExamDate { get; set; }

    [StringLength(1000)]
    public string? TeacherNotes { get; set; }
}

public class UpdateStudentMatnProgressDto
{
    [Required]
    public Guid Id { get; set; }

    public StudyStatus StudyStatus { get; set; }
    public MemorizationStatus MemorizationStatus { get; set; }
    public ExamStatus ExamStatus { get; set; }

    [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين 0 و 100.")]
    public decimal? Score { get; set; }

    public DateTime? ExamDate { get; set; }

    [StringLength(1000)]
    public string? TeacherNotes { get; set; }
}

public class UpdateStudyStatusDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public StudyStatus StudyStatus { get; set; }
}

public class UpdateMemorizationStatusDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public MemorizationStatus MemorizationStatus { get; set; }
}

public class RecordExamResultDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public ExamStatus ExamStatus { get; set; }

    [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين 0 و 100.")]
    public decimal? Score { get; set; }

    public DateTime? ExamDate { get; set; }

    [StringLength(1000)]
    public string? TeacherNotes { get; set; }
}

public class CompleteBothStudyAndMemorizationDto
{
    [Required]
    public Guid Id { get; set; }

    [StringLength(1000)]
    public string? TeacherNotes { get; set; }
}
