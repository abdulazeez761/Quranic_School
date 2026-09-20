using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.Matn;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services;

public class MatnAssignmentService : IMatnAssignmentService
{
    private readonly IMatnAssignmentRepository _assignmentRepository;
    private readonly IClassRepository _classRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IMatnRepository _matnRepository;
    private readonly IStudentMatnProgressRepository _progressRepository;

    public MatnAssignmentService(
        IMatnAssignmentRepository assignmentRepository,
        IClassRepository classRepository,
        IStudentRepository studentRepository,
        IMatnRepository matnRepository,
        IStudentMatnProgressRepository progressRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classRepository = classRepository;
        _studentRepository = studentRepository;
        _matnRepository = matnRepository;
        _progressRepository = progressRepository;
    }

    public async Task<MatnAssignmentDto?> GetByIdAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        return assignment == null ? null : MapToDto(assignment);
    }

    public async Task<IEnumerable<MatnAssignmentDto>> GetByStudentAndClassAsync(Guid studentId, Guid classId)
    {
        var assignments = await _assignmentRepository.GetByStudentAndClassAsync(studentId, classId);
        return assignments.Select(MapToDto);
    }

    public async Task<IEnumerable<MatnAssignmentDto>> GetByClassAndDateAsync(Guid classId, DateTime date)
    {
        var assignments = await _assignmentRepository.GetByClassAndDateAsync(classId, date);
        return assignments.Select(MapToDto);
    }

    public async Task<IEnumerable<MatnAssignmentDto>> GetByClassIdAsync(Guid classId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var assignments = await _assignmentRepository.GetByClassIdAsync(classId, fromDate, toDate);
        return assignments.Select(MapToDto);
    }

    public async Task<IEnumerable<MatnAssignmentDto>> GetByStudentIdAsync(Guid studentId)
    {
        var assignments = await _assignmentRepository.GetByStudentIdAsync(studentId);
        return assignments.Select(MapToDto);
    }

    public async Task<(bool Success, string Message, Guid? Id)> AssignAsync(AssignMatnDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "البيانات غير صالحة.", null);

        var cls = await _classRepository.GetById(dto.ClassId);
        if (cls == null)
            return (false, "الشعبة/الحلقة غير موجودة.", null);

        // التحقق من أن المعلم يدرّس هذه الحلقة
        if (!cls.Teachers.Any(t => t.UserId == teacherId))
            return (false, "غير مصرح لك بتسجيل التسميع في هذه الحلقة.", null);

        // التحقق من أن الطالب مسجل في هذه الحلقة
        if (!cls.Students.Any(s => s.UserId == dto.StudentId))
            return (false, "الطالب غير مسجل في هذه الحلقة.", null);

        // التحقق من صحة المتن إن تم تحديده أو استنتاجه
        if (!dto.MatnId.HasValue && cls.StudyProgramId.HasValue)
        {
            var programMatuns = (await _matnRepository.GetByProgramIdAsync(cls.StudyProgramId.Value)).ToList();
            if (programMatuns.Count == 1)
            {
                dto.MatnId = programMatuns[0].Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ChapterName))
            {
                var matched = programMatuns.FirstOrDefault(m => dto.ChapterName.StartsWith(m.Title, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    dto.MatnId = matched.Id;
                }
            }
        }

        if (dto.MatnId.HasValue)
        {
            var matn = await _matnRepository.GetByIdAsync(dto.MatnId.Value);
            if (matn == null)
                return (false, "المتن المحدد غير موجود.", null);

            if (!matn.IsActive)
                return (false, "المتن غير مفعّل للواجبات والتسميع.", null);

            // التحقق من العزل للمعهد
            if (matn.InstituteId != null && matn.InstituteId != cls.InstituteId)
                return (false, "المتن لا يتبع هذا المعهد.", null);

            // التحقق من تبعية المتن للبرنامج العلمي الخاص بالحلقة
            if (cls.StudyProgramId.HasValue && matn.StudyProgramId.HasValue && matn.StudyProgramId != cls.StudyProgramId)
                return (false, "المتن المحدد لا ينتمي إلى البرنامج العلمي المعتمد لهذه الحلقة.", null);
        }

        var assignment = new MatnAssignment
        {
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
            MatnId = dto.MatnId,
            PerformanceType = dto.PerformanceType,
            Unit = dto.Unit,
            Amount = dto.Amount,
            ChapterName = dto.ChapterName?.Trim(),
            FromNumber = dto.FromNumber,
            ToNumber = dto.ToNumber,
            Status = dto.Status,
            IsCompleted = dto.Status != AssignmentStatus.notSet,
            IsUpcoming = dto.IsUpcoming,
            AssignedDate = dto.AssignedDate ?? DateTime.UtcNow,
            Note = dto.Note?.Trim()
        };

        var created = await _assignmentRepository.AddAsync(assignment);

        if (assignment.MatnId.HasValue)
        {
            await ReevaluateProgressAfterAssignmentChangeAsync(assignment.StudentId, assignment.MatnId.Value);
        }

        return (true, "تم رصد التسميع بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> UpdateAsync(EditMatnAssignmentDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "البيانات غير صالحة.");

        var assignment = await _assignmentRepository.GetByIdAsync(dto.Id);
        if (assignment == null)
            return (false, "سجل الورد غير موجود.");

        var cls = await _classRepository.GetById(assignment.ClassId);
        if (cls != null && !cls.Teachers.Any(t => t.UserId == teacherId))
            return (false, "غير مصرح لك بتعديل الورد في هذه الحلقة.");

        if (dto.MatnId.HasValue)
        {
            var matn = await _matnRepository.GetByIdAsync(dto.MatnId.Value);
            if (matn == null)
                return (false, "المتن المحدد غير موجود.");

            if (!matn.IsActive)
                return (false, "المتن غير مفعّل للواجبات والتسميع.");

            if (cls != null)
            {
                if (matn.InstituteId != null && matn.InstituteId != cls.InstituteId)
                    return (false, "المتن لا يتبع هذا المعهد.");

                if (cls.StudyProgramId.HasValue && matn.StudyProgramId.HasValue && matn.StudyProgramId != cls.StudyProgramId)
                    return (false, "المتن المحدد لا ينتمي إلى البرنامج العلمي المعتمد لهذه الحلقة.");
            }
        }

        var oldMatnId = assignment.MatnId;

        assignment.MatnId = dto.MatnId;
        assignment.PerformanceType = dto.PerformanceType;
        assignment.Unit = dto.Unit;
        assignment.Amount = dto.Amount;
        assignment.ChapterName = dto.ChapterName?.Trim();
        assignment.FromNumber = dto.FromNumber;
        assignment.ToNumber = dto.ToNumber;
        if (dto.IsUpcoming)
        {
            assignment.IsUpcoming = true;
            assignment.Status = AssignmentStatus.notSet;
            assignment.IsCompleted = false;
        }
        else
        {
            assignment.IsUpcoming = false;
            assignment.Status = dto.Status;
            assignment.IsCompleted = dto.Status != AssignmentStatus.notSet;
        }
        assignment.Note = dto.Note?.Trim();

        var updated = await _assignmentRepository.UpdateAsync(assignment);
        if (updated)
        {
            if (assignment.MatnId.HasValue)
            {
                await ReevaluateProgressAfterAssignmentChangeAsync(assignment.StudentId, assignment.MatnId.Value);
            }
            if (oldMatnId.HasValue && oldMatnId.Value != assignment.MatnId)
            {
                await ReevaluateProgressAfterAssignmentChangeAsync(assignment.StudentId, oldMatnId.Value);
            }
        }
        return updated ? (true, "تم تحديث ورد المتن بنجاح.") : (false, "فشل حفظ التعديلات.");
    }

    private async Task ReevaluateProgressAfterAssignmentChangeAsync(Guid studentId, Guid matnId)
    {
        try
        {
            var studentAssignments = (await _assignmentRepository.GetByStudentIdAsync(studentId))
                .Where(a => a.MatnId == matnId)
                .ToList();

            bool hasMudarasah = studentAssignments.Any(a => a.PerformanceType == MatnPerformanceType.Mudarasah);
            bool hasHifz = studentAssignments.Any(a => a.PerformanceType == MatnPerformanceType.Memorization || 
                                                       a.PerformanceType == MatnPerformanceType.Revision);

            var existing = await _progressRepository.GetByStudentAndMatnAsync(studentId, matnId);
            if (existing == null)
            {
                if (hasMudarasah || hasHifz)
                {
                    var progress = new StudentMatnProgress
                    {
                        StudentId = studentId,
                        MatnId = matnId,
                        StudyStatus = hasMudarasah ? StudyStatus.InProgress : StudyStatus.NotStarted,
                        StudyStartedAt = hasMudarasah ? DateTime.UtcNow : null,
                        MemorizationStatus = hasHifz ? MemorizationStatus.InProgress : MemorizationStatus.NotStarted,
                        ExamStatus = ExamStatus.NotTested,
                        LastUpdatedAt = DateTime.UtcNow
                    };
                    await _progressRepository.AddAsync(progress);
                }
                return;
            }

            bool modified = false;

            if (existing.StudyStatus != StudyStatus.Completed)
            {
                var newStudyStatus = hasMudarasah ? StudyStatus.InProgress : StudyStatus.NotStarted;
                if (existing.StudyStatus != newStudyStatus)
                {
                    existing.StudyStatus = newStudyStatus;
                    if (newStudyStatus == StudyStatus.InProgress)
                        existing.StudyStartedAt ??= DateTime.UtcNow;
                    modified = true;
                }
            }

            if (existing.MemorizationStatus != MemorizationStatus.Memorized)
            {
                var newMemStatus = hasHifz ? MemorizationStatus.InProgress : MemorizationStatus.NotStarted;
                if (existing.MemorizationStatus != newMemStatus)
                {
                    existing.MemorizationStatus = newMemStatus;
                    modified = true;
                }
            }

            if (modified)
            {
                existing.LastUpdatedAt = DateTime.UtcNow;
                await _progressRepository.UpdateAsync(existing);
            }
        }
        catch
        {
            // Non-blocking background sync
        }
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateMatnStatusDto dto, Guid teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(dto.Id);
        if (assignment == null)
            return (false, "السجل غير موجود.");

        assignment.Status = dto.Status;
        if (dto.Status != AssignmentStatus.notSet)
        {
            assignment.IsCompleted = true;
            assignment.IsUpcoming = false;
        }
        else
        {
            assignment.IsCompleted = false;
            assignment.IsUpcoming = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Note))
        {
            assignment.Note = dto.Note.Trim();
        }

        var updated = await _assignmentRepository.UpdateAsync(assignment);
        return updated ? (true, "تم تحديث التقييم بنجاح.") : (false, "فشل التحديث.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id);
        if (assignment == null)
            return (false, "السجل غير موجود.");

        var studentId = assignment.StudentId;
        var matnId = assignment.MatnId;
        var deleted = await _assignmentRepository.DeleteAsync(id);
        if (deleted && matnId.HasValue)
        {
            await ReevaluateProgressAfterAssignmentChangeAsync(studentId, matnId.Value);
        }
        return deleted ? (true, "تم حذف التكليف بنجاح.") : (false, "تعذر الحذف.");
    }

    private static MatnAssignmentDto MapToDto(MatnAssignment ma) => new()
    {
        Id = ma.Id,
        StudentId = ma.StudentId,
        StudentName = ma.Student?.StudentInfo != null ? $"{ma.Student.StudentInfo.FirstName} {ma.Student.StudentInfo.SecondName}" : string.Empty,
        ClassId = ma.ClassId,
        ClassName = ma.Class?.Name ?? string.Empty,
        MatnId = ma.MatnId,
        MatnTitle = ma.Matn?.Title,
        PerformanceType = ma.PerformanceType,
        Unit = ma.Unit,
        Amount = ma.Amount,
        ChapterName = ma.ChapterName,
        FromNumber = ma.FromNumber,
        ToNumber = ma.ToNumber,
        Status = ma.Status,
        IsCompleted = ma.IsCompleted,
        IsUpcoming = ma.IsUpcoming,
        AssignedDate = ma.AssignedDate,
        Note = ma.Note
    };
}
