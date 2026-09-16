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

    public MatnAssignmentService(
        IMatnAssignmentRepository assignmentRepository,
        IClassRepository classRepository,
        IStudentRepository studentRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classRepository = classRepository;
        _studentRepository = studentRepository;
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

        var assignment = new MatnAssignment
        {
            StudentId = dto.StudentId,
            ClassId = dto.ClassId,
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
        return (true, "تم رصد التسميع بنجاح.", created.Id);
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(UpdateMatnStatusDto dto, Guid teacherId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(dto.Id);
        if (assignment == null)
            return (false, "السجل غير موجود.");

        assignment.Status = dto.Status;
        assignment.IsCompleted = dto.Status != AssignmentStatus.notSet;
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

        var deleted = await _assignmentRepository.DeleteAsync(id);
        return deleted ? (true, "تم حذف التكليف بنجاح.") : (false, "تعذر الحذف.");
    }

    private static MatnAssignmentDto MapToDto(MatnAssignment ma) => new()
    {
        Id = ma.Id,
        StudentId = ma.StudentId,
        StudentName = ma.Student?.StudentInfo != null ? $"{ma.Student.StudentInfo.FirstName} {ma.Student.StudentInfo.SecondName}" : string.Empty,
        ClassId = ma.ClassId,
        ClassName = ma.Class?.Name ?? string.Empty,
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
