using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.Matn;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services;

public class StudentMatnProgressService : IStudentMatnProgressService
{
    private readonly IStudentMatnProgressRepository _progressRepository;
    private readonly IMatnRepository _matnRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;

    public StudentMatnProgressService(
        IStudentMatnProgressRepository progressRepository,
        IMatnRepository matnRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository)
    {
        _progressRepository = progressRepository;
        _matnRepository = matnRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<StudentMatnProgressDto?> GetByIdAsync(Guid id)
    {
        var progress = await _progressRepository.GetByIdAsync(id);
        return progress == null ? null : MapToDto(progress);
    }

    public async Task<StudentMatnProgressDto?> GetByStudentAndMatnAsync(Guid studentId, Guid matnId)
    {
        var progress = await _progressRepository.GetByStudentAndMatnAsync(studentId, matnId);
        return progress == null ? null : MapToDto(progress);
    }

    public async Task<IEnumerable<StudentMatnProgressDto>> GetByStudentAsync(Guid studentId)
    {
        var progresses = await _progressRepository.GetByStudentAsync(studentId);
        return progresses.Select(MapToDto);
    }

    public async Task<IEnumerable<StudentMatnProgressDto>> GetByClassAsync(Guid classId)
    {
        var progresses = await _progressRepository.GetByClassAsync(classId);
        return progresses.Select(MapToDto);
    }

    public async Task<(bool Success, string Message, Guid? Id)> CreateOrUpdateAsync(CreateStudentMatnProgressDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "بيانات التقدم غير صالحة.", null);

        var student = await _studentRepository.GetByIdAsync(dto.StudentId);
        if (student == null)
            return (false, "الطالب غير موجود.", null);

        var matn = await _matnRepository.GetByIdAsync(dto.MatnId);
        if (matn == null)
            return (false, "المتن غير موجود.", null);

        if (!matn.IsActive)
            return (false, "المتن غير مفعّل للتقييم والتسميع حالياً.", null);

        // التحقق من صلاحية المعلم إن تم تمرير معرفه
        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, dto.StudentId))
            return (false, "المعلم غير مصرح له بتعديل بيانات هذا الطالب.", null);

        var existing = await _progressRepository.GetByStudentAndMatnAsync(dto.StudentId, dto.MatnId);
        if (existing != null)
        {
            existing.StudyStatus = dto.StudyStatus;
            existing.MemorizationStatus = dto.MemorizationStatus;
            existing.ExamStatus = dto.ExamStatus;
            existing.Score = dto.Score;
            existing.ExamDate = dto.ExamDate;
            if (!string.IsNullOrWhiteSpace(dto.TeacherNotes))
                existing.TeacherNotes = dto.TeacherNotes.Trim();

            ApplyStatusTimestamps(existing);
            existing.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
            existing.LastUpdatedAt = DateTime.UtcNow;

            await _progressRepository.UpdateAsync(existing);
            return (true, "تم تحديث سجل إنجاز المتن بنجاح.", existing.Id);
        }

        var progress = new StudentMatnProgress
        {
            StudentId = dto.StudentId,
            MatnId = dto.MatnId,
            StudyStatus = dto.StudyStatus,
            MemorizationStatus = dto.MemorizationStatus,
            ExamStatus = dto.ExamStatus,
            Score = dto.Score,
            ExamDate = dto.ExamDate,
            TeacherNotes = dto.TeacherNotes?.Trim(),
            LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null,
            LastUpdatedAt = DateTime.UtcNow
        };

        ApplyStatusTimestamps(progress);

        var created = await _progressRepository.AddAsync(progress);
        return (true, "تم إنشاء سجل إنجاز المتن بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> CompleteStudyAsync(Guid id, Guid teacherId)
    {
        var progress = await _progressRepository.GetByIdAsync(id);
        if (progress == null)
            return (false, "سجل الإنجاز غير موجود.");

        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, progress.StudentId))
            return (false, "المعلم غير مصرح له بتعديل هذا السجل.");

        progress.StudyStatus = StudyStatus.Completed;
        progress.StudyCompletedAt ??= DateTime.UtcNow;
        progress.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
        progress.LastUpdatedAt = DateTime.UtcNow;

        var updated = await _progressRepository.UpdateAsync(progress);
        return updated ? (true, "تم تسجيل اكتمال مدارسة المتن بنجاح.") : (false, "فشل حفظ التعديل.");
    }

    public async Task<(bool Success, string Message)> CompleteMemorizationAsync(Guid id, Guid teacherId)
    {
        var progress = await _progressRepository.GetByIdAsync(id);
        if (progress == null)
            return (false, "سجل الإنجاز غير موجود.");

        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, progress.StudentId))
            return (false, "المعلم غير مصرح له بتعديل هذا السجل.");

        progress.MemorizationStatus = MemorizationStatus.Memorized;
        progress.MemorizationCompletedAt ??= DateTime.UtcNow;
        progress.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
        progress.LastUpdatedAt = DateTime.UtcNow;

        var updated = await _progressRepository.UpdateAsync(progress);
        return updated ? (true, "تم تسجيل اكتمال حفظ المتن بنجاح.") : (false, "فشل حفظ التعديل.");
    }

    public async Task<(bool Success, string Message)> CompleteStudyAndMemorizationAsync(CompleteBothStudyAndMemorizationDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "البيانات غير صالحة.");

        var progress = await _progressRepository.GetByIdAsync(dto.Id);
        if (progress == null)
            return (false, "سجل الإنجاز غير موجود.");

        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, progress.StudentId))
            return (false, "المعلم غير مصرح له بتعديل هذا السجل.");

        progress.StudyStatus = StudyStatus.Completed;
        progress.StudyCompletedAt ??= DateTime.UtcNow;
        progress.MemorizationStatus = MemorizationStatus.Memorized;
        progress.MemorizationCompletedAt ??= DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.TeacherNotes))
        {
            progress.TeacherNotes = string.IsNullOrWhiteSpace(progress.TeacherNotes)
                ? dto.TeacherNotes.Trim()
                : $"{progress.TeacherNotes}\n{dto.TeacherNotes.Trim()}";
        }

        progress.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
        progress.LastUpdatedAt = DateTime.UtcNow;

        var updated = await _progressRepository.UpdateAsync(progress);
        return updated ? (true, "تم إتمام دراسة وحفظ المتن بنجاح.") : (false, "فشل حفظ التعديل.");
    }

    public async Task<(bool Success, string Message)> RecordExamResultAsync(RecordExamResultDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "بيانات الاختبار غير صالحة.");

        var progress = await _progressRepository.GetByIdAsync(dto.Id);
        if (progress == null)
            return (false, "سجل الإنجاز غير موجود.");

        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, progress.StudentId))
            return (false, "المعلم غير مصرح له بتسجيل نتيجة الاختبار.");

        progress.ExamStatus = dto.ExamStatus;
        progress.Score = dto.Score;
        progress.ExamDate = dto.ExamDate ?? DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.TeacherNotes))
            progress.TeacherNotes = dto.TeacherNotes.Trim();

        progress.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
        progress.LastUpdatedAt = DateTime.UtcNow;

        var updated = await _progressRepository.UpdateAsync(progress);
        return updated ? (true, "تم تسجيل نتيجة الاختبار بنجاح.") : (false, "فشل حفظ نتيجة الاختبار.");
    }

    public async Task<(bool Success, string Message)> UpdateProgressAsync(UpdateStudentMatnProgressDto dto, Guid teacherId)
    {
        if (dto == null)
            return (false, "بيانات التحديث غير صالحة.");

        var progress = await _progressRepository.GetByIdAsync(dto.Id);
        if (progress == null)
            return (false, "سجل الإنجاز غير موجود.");

        if (teacherId != Guid.Empty && !await IsTeacherAuthorizedForStudentAsync(teacherId, progress.StudentId))
            return (false, "المعلم غير مصرح له بتعديل هذا السجل.");

        progress.StudyStatus = dto.StudyStatus;
        progress.MemorizationStatus = dto.MemorizationStatus;
        progress.ExamStatus = dto.ExamStatus;
        progress.Score = dto.Score;
        progress.ExamDate = dto.ExamDate;

        if (dto.TeacherNotes != null)
            progress.TeacherNotes = dto.TeacherNotes.Trim();

        ApplyStatusTimestamps(progress);
        progress.LastUpdatedByTeacherId = teacherId != Guid.Empty ? teacherId : null;
        progress.LastUpdatedAt = DateTime.UtcNow;

        var updated = await _progressRepository.UpdateAsync(progress);
        return updated ? (true, "تم تحديث السجل بنجاح.") : (false, "فشل حفظ التعديلات.");
    }

    private async Task<bool> IsTeacherAuthorizedForStudentAsync(Guid teacherId, Guid studentId)
    {
        var teacherClasses = await _teacherRepository.GetTeacherClasses(teacherId);
        if (teacherClasses == null || !teacherClasses.Any())
            return false;

        var teacherClassIds = teacherClasses.Select(c => c.Id).ToHashSet();
        var student = await _studentRepository.GetStudentWithClassesAsync(studentId);
        if (student == null)
            return false;

        if (student.ClassId.HasValue && teacherClassIds.Contains(student.ClassId.Value))
            return true;

        if (student.Classes != null && student.Classes.Any(c => teacherClassIds.Contains(c.Id)))
            return true;

        return false;
    }

    private static void ApplyStatusTimestamps(StudentMatnProgress p)
    {
        if (p.StudyStatus == StudyStatus.InProgress && !p.StudyStartedAt.HasValue)
            p.StudyStartedAt = DateTime.UtcNow;
        else if (p.StudyStatus == StudyStatus.Completed)
        {
            p.StudyStartedAt ??= DateTime.UtcNow;
            p.StudyCompletedAt ??= DateTime.UtcNow;
        }

        if (p.MemorizationStatus == MemorizationStatus.Memorized)
            p.MemorizationCompletedAt ??= DateTime.UtcNow;
    }

    private static StudentMatnProgressDto MapToDto(StudentMatnProgress p) => new()
    {
        Id = p.Id,
        StudentId = p.StudentId,
        StudentName = p.Student?.StudentInfo?.FullName ?? string.Empty,
        MatnId = p.MatnId,
        MatnTitle = p.Matn?.Title ?? string.Empty,
        MatnAuthor = p.Matn?.Author,
        MatnCategory = p.Matn?.Category ?? MatnCategory.General,
        MatnOrder = p.Matn?.Order ?? 1,
        StudyProgramId = p.Matn?.StudyProgramId,
        StudyProgramName = p.Matn?.StudyProgram?.Name,
        StudyStatus = p.StudyStatus,
        MemorizationStatus = p.MemorizationStatus,
        ExamStatus = p.ExamStatus,
        Score = p.Score,
        ExamDate = p.ExamDate,
        TeacherNotes = p.TeacherNotes,
        StudyStartedAt = p.StudyStartedAt,
        StudyCompletedAt = p.StudyCompletedAt,
        MemorizationCompletedAt = p.MemorizationCompletedAt,
        LastUpdatedByTeacherId = p.LastUpdatedByTeacherId,
        LastUpdatedByTeacherName = p.LastUpdatedByTeacher?.TeacherInfo?.FullName,
        LastUpdatedAt = p.LastUpdatedAt,
        CreatedAt = p.CreatedAt
    };
}
