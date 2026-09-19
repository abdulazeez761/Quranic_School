using System;
using System.ComponentModel.DataAnnotations;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.DTOs.Matn;

public class MatnAssignmentDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public Guid? MatnId { get; set; }
    public string? MatnTitle { get; set; }

    public MatnPerformanceType PerformanceType { get; set; }
    public string PerformanceTypeName => PerformanceType switch
    {
        MatnPerformanceType.Memorization => "حفظ جديد",
        MatnPerformanceType.Revision => "مراجعة",
        MatnPerformanceType.Mudarasah => "مدارسة",
        _ => "تسميع"
    };

    public MatnUnit Unit { get; set; }
    public string UnitName => Unit switch
    {
        MatnUnit.Verses => "أبيات",
        MatnUnit.Lines => "سطور",
        MatnUnit.Pages => "صفحات",
        MatnUnit.Chapters => "أبواب",
        MatnUnit.Hadiths => "أحاديث",
        _ => "وحدات"
    };

    public decimal? Amount { get; set; }
    public string? ChapterName { get; set; }
    public int? FromNumber { get; set; }
    public int? ToNumber { get; set; }

    public AssignmentStatus Status { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsUpcoming { get; set; }
    public DateTime AssignedDate { get; set; }
    public string? Note { get; set; }
}

public class AssignMatnDto
{
    [Required]
    public Guid StudentId { get; set; }

    [Required]
    public Guid ClassId { get; set; }

    public Guid? MatnId { get; set; }

    [Required]
    public MatnPerformanceType PerformanceType { get; set; } = MatnPerformanceType.Memorization;

    [Required]
    public MatnUnit Unit { get; set; } = MatnUnit.Verses;

    [Range(0.01, 9999.99, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر.")]
    public decimal? Amount { get; set; }

    [StringLength(150)]
    public string? ChapterName { get; set; }

    public int? FromNumber { get; set; }
    public int? ToNumber { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.notSet;
    public bool IsCompleted { get; set; } = false;
    public bool IsUpcoming { get; set; } = false;
    public DateTime? AssignedDate { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

public class UpdateMatnStatusDto
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public AssignmentStatus Status { get; set; }

    public string? Note { get; set; }
}

public class EditMatnAssignmentDto
{
    [Required]
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid? MatnId { get; set; }

    [Required]
    public MatnPerformanceType PerformanceType { get; set; } = MatnPerformanceType.Memorization;

    [Required]
    public MatnUnit Unit { get; set; } = MatnUnit.Verses;

    [Range(0.01, 9999.99, ErrorMessage = "الكمية يجب أن تكون أكبر من صفر.")]
    public decimal? Amount { get; set; }

    [StringLength(150)]
    public string? ChapterName { get; set; }

    public int? FromNumber { get; set; }
    public int? ToNumber { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.notSet;
    public bool IsUpcoming { get; set; } = false;

    [StringLength(500)]
    public string? Note { get; set; }
}
