using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hafiz.Domain.Common;
using Hafiz.Domain.Enums;
using Hafiz.Models;

namespace Hafiz.Domain.Entities;

public class StudentMatnProgress : ISoftDeletable
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid StudentId { get; set; }

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    [Required]
    public Guid MatnId { get; set; }

    [ForeignKey(nameof(MatnId))]
    public Matn Matn { get; set; } = null!;

    public StudyStatus StudyStatus { get; set; } = StudyStatus.NotStarted;
    public MemorizationStatus MemorizationStatus { get; set; } = MemorizationStatus.NotStarted;
    public ExamStatus ExamStatus { get; set; } = ExamStatus.NotTested;

    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100, ErrorMessage = "الدرجة يجب أن تكون بين 0 و 100.")]
    public decimal? Score { get; set; }

    public DateTime? ExamDate { get; set; }

    [StringLength(1000)]
    public string? TeacherNotes { get; set; }

    public DateTime? StudyStartedAt { get; set; }
    public DateTime? StudyCompletedAt { get; set; }
    public DateTime? MemorizationCompletedAt { get; set; }

    public Guid? LastUpdatedByTeacherId { get; set; }

    [ForeignKey(nameof(LastUpdatedByTeacherId))]
    public Teacher? LastUpdatedByTeacher { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Implementation of ISoftDeletable
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}
