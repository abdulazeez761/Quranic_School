using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories;

public class StudentMatnProgressRepository : IStudentMatnProgressRepository
{
    private readonly ApplicationDbContext _context;

    public StudentMatnProgressRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StudentMatnProgress?> GetByIdAsync(Guid id)
    {
        return await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<StudentMatnProgress?> GetByStudentAndMatnAsync(Guid studentId, Guid matnId)
    {
        var progress = await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.MatnId == matnId);

        if (progress != null)
        {
            await SyncProgressesWithAssignmentsAsync(new List<StudentMatnProgress> { progress }, new List<Guid> { studentId });
        }

        return progress;
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByStudentAsync(Guid studentId)
    {
        var existingProgresses = await _context.StudentMatnProgresses
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => p.StudentId == studentId)
            .ToListAsync();

        // Find student's enrolled classes that have a StudyProgram
        var programIds = await _context.Classes
            .Where(c => !c.IsDeleted && c.StudyProgramId.HasValue && c.Students.Any(s => s.UserId == studentId))
            .Select(c => c.StudyProgramId!.Value)
            .Distinct()
            .ToListAsync();

        // Fallback: check if existing progresses link to a StudyProgram
        if (!programIds.Any())
        {
            var fallbackProgIds = existingProgresses
                .Where(p => p.Matn != null && p.Matn.StudyProgramId.HasValue)
                .Select(p => p.Matn!.StudyProgramId!.Value)
                .Distinct()
                .ToList();
            programIds.AddRange(fallbackProgIds);
        }

        if (programIds.Any())
        {
            var allProgramMatuns = await _context.Matns
                .Include(m => m.StudyProgram)
                .Where(m => !m.IsDeleted && m.IsActive && m.StudyProgramId.HasValue && programIds.Contains(m.StudyProgramId.Value))
                .OrderBy(m => m.Order)
                .ToListAsync();

            var existingMatnIds = existingProgresses.Select(p => p.MatnId).ToHashSet();

            foreach (var m in allProgramMatuns)
            {
                if (!existingMatnIds.Contains(m.Id))
                {
                    existingProgresses.Add(new StudentMatnProgress
                    {
                        Id = Guid.Empty,
                        StudentId = studentId,
                        MatnId = m.Id,
                        Matn = m,
                        StudyStatus = StudyStatus.NotStarted,
                        MemorizationStatus = MemorizationStatus.NotStarted,
                        ExamStatus = ExamStatus.NotTested
                    });
                }
            }
        }

        await SyncProgressesWithAssignmentsAsync(existingProgresses, new List<Guid> { studentId });

        return existingProgresses.OrderBy(p => p.Matn?.Order ?? 0);
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByMatnAsync(Guid matnId)
    {
        var progresses = await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => p.MatnId == matnId)
            .OrderBy(p => p.Student.StudentInfo.FirstName)
            .ThenBy(p => p.Student.StudentInfo.SecondName)
            .ToListAsync();

        var studentIds = progresses.Select(p => p.StudentId).Distinct().ToList();
        await SyncProgressesWithAssignmentsAsync(progresses, studentIds);

        return progresses;
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByClassAsync(Guid classId)
    {
        var cls = await _context.Classes
            .Include(c => c.Students)
                .ThenInclude(s => s.StudentInfo)
            .FirstOrDefaultAsync(c => c.Id == classId && !c.IsDeleted);

        if (cls == null)
            return Enumerable.Empty<StudentMatnProgress>();

        var studentList = cls.Students.ToList();
        var studentIds = studentList.Select(s => s.UserId).ToList();

        var existing = await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => studentIds.Contains(p.StudentId))
            .ToListAsync();

        if (cls.StudyProgramId.HasValue)
        {
            var programMatuns = await _context.Matns
                .Include(m => m.StudyProgram)
                .Where(m => !m.IsDeleted && m.IsActive && m.StudyProgramId == cls.StudyProgramId.Value)
                .OrderBy(m => m.Order)
                .ToListAsync();

            foreach (var st in studentList)
            {
                var stExistingMatnIds = existing.Where(p => p.StudentId == st.UserId).Select(p => p.MatnId).ToHashSet();
                foreach (var matn in programMatuns)
                {
                    if (!stExistingMatnIds.Contains(matn.Id))
                    {
                        existing.Add(new StudentMatnProgress
                        {
                            Id = Guid.Empty,
                            StudentId = st.UserId,
                            Student = st,
                            MatnId = matn.Id,
                            Matn = matn,
                            StudyStatus = StudyStatus.NotStarted,
                            MemorizationStatus = MemorizationStatus.NotStarted,
                            ExamStatus = ExamStatus.NotTested
                        });
                    }
                }
            }
        }

        await SyncProgressesWithAssignmentsAsync(existing, studentIds);

        return existing
            .OrderBy(p => p.Student?.StudentInfo?.FirstName)
            .ThenBy(p => p.Student?.StudentInfo?.SecondName)
            .ThenBy(p => p.Matn?.Order ?? 0);
    }

    private async Task SyncProgressesWithAssignmentsAsync(List<StudentMatnProgress> progresses, List<Guid> studentIds)
    {
        if (!progresses.Any() || !studentIds.Any())
            return;

        var cleanStudentIds = studentIds.Distinct().ToList();

        // Fetch all assignments for these students
        var assignments = await _context.MatnAssignments
            .Where(ma => cleanStudentIds.Contains(ma.StudentId))
            .Select(ma => new { ma.StudentId, ma.MatnId, ma.PerformanceType, ma.ChapterName, ma.AssignedDate })
            .ToListAsync();

        bool hasChanges = false;
        var toAddList = new List<StudentMatnProgress>();

        foreach (var p in progresses)
        {
            var matnTitle = p.Matn?.Title;
            var studentAssignments = assignments
                .Where(a => a.StudentId == p.StudentId &&
                           (a.MatnId == p.MatnId ||
                           (!a.MatnId.HasValue && !string.IsNullOrEmpty(matnTitle) && a.ChapterName != null && a.ChapterName.StartsWith(matnTitle))))
                .ToList();

            bool hasMudarasah = studentAssignments.Any(a => a.PerformanceType == MatnPerformanceType.Mudarasah);
            bool hasHifz = studentAssignments.Any(a => a.PerformanceType == MatnPerformanceType.Memorization ||
                                                       a.PerformanceType == MatnPerformanceType.Revision);

            // 1. Study status sync
            if (p.StudyStatus != StudyStatus.Completed)
            {
                if (hasMudarasah)
                {
                    if (p.StudyStatus == StudyStatus.NotStarted)
                    {
                        p.StudyStatus = StudyStatus.InProgress;
                        var earliestMudarasah = studentAssignments
                            .Where(a => a.PerformanceType == MatnPerformanceType.Mudarasah)
                            .OrderBy(a => a.AssignedDate)
                            .Select(a => (DateTime?)a.AssignedDate)
                            .FirstOrDefault();
                        p.StudyStartedAt ??= earliestMudarasah ?? DateTime.UtcNow;
                        p.LastUpdatedAt = DateTime.UtcNow;
                        if (p.Id != Guid.Empty) hasChanges = true;
                    }
                }
                else
                {
                    if (p.StudyStatus == StudyStatus.InProgress)
                    {
                        p.StudyStatus = StudyStatus.NotStarted;
                        p.LastUpdatedAt = DateTime.UtcNow;
                        if (p.Id != Guid.Empty) hasChanges = true;
                    }
                }
            }

            // 2. Memorization status sync
            if (p.MemorizationStatus != MemorizationStatus.Memorized)
            {
                if (hasHifz)
                {
                    if (p.MemorizationStatus == MemorizationStatus.NotStarted)
                    {
                        p.MemorizationStatus = MemorizationStatus.InProgress;
                        p.LastUpdatedAt = DateTime.UtcNow;
                        if (p.Id != Guid.Empty) hasChanges = true;
                    }
                }
                else
                {
                    if (p.MemorizationStatus == MemorizationStatus.InProgress)
                    {
                        p.MemorizationStatus = MemorizationStatus.NotStarted;
                        p.LastUpdatedAt = DateTime.UtcNow;
                        if (p.Id != Guid.Empty) hasChanges = true;
                    }
                }
            }

            // If it was a virtual row (Id == Empty) and now has assignments, persist it
            if (p.Id == Guid.Empty && (hasMudarasah || hasHifz))
            {
                var newEntity = new StudentMatnProgress
                {
                    Id = Guid.NewGuid(),
                    StudentId = p.StudentId,
                    MatnId = p.MatnId,
                    StudyStatus = p.StudyStatus,
                    StudyStartedAt = p.StudyStartedAt,
                    MemorizationStatus = p.MemorizationStatus,
                    ExamStatus = p.ExamStatus,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdatedAt = DateTime.UtcNow
                };
                toAddList.Add(newEntity);
                p.Id = newEntity.Id;
            }
        }

        if (toAddList.Any())
        {
            await _context.StudentMatnProgresses.AddRangeAsync(toAddList);
            hasChanges = true;
        }

        if (hasChanges)
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Non-blocking
            }
        }
    }

    public async Task<StudentMatnProgress> AddAsync(StudentMatnProgress progress)
    {
        await _context.StudentMatnProgresses.AddAsync(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<bool> UpdateAsync(StudentMatnProgress progress)
    {
        _context.StudentMatnProgresses.Update(progress);
        var affected = await _context.SaveChangesAsync();
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var progress = await _context.StudentMatnProgresses.FirstOrDefaultAsync(p => p.Id == id);
        if (progress == null)
            return false;

        _context.StudentMatnProgresses.Remove(progress);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetCountByStatusAsync(
        Guid instituteId, 
        StudyStatus? studyStatus = null, 
        MemorizationStatus? memorizationStatus = null, 
        ExamStatus? examStatus = null)
    {
        var query = _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Where(p => p.Student.StudentInfo.InstituteId == instituteId);

        if (studyStatus.HasValue)
        {
            query = query.Where(p => p.StudyStatus == studyStatus.Value);
        }

        if (memorizationStatus.HasValue)
        {
            query = query.Where(p => p.MemorizationStatus == memorizationStatus.Value);
        }

        if (examStatus.HasValue)
        {
            query = query.Where(p => p.ExamStatus == examStatus.Value);
        }

        return await query.CountAsync();
    }
}
