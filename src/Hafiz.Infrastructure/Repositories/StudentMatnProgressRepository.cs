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
        return await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.MatnId == matnId);
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByStudentAsync(Guid studentId)
    {
        return await _context.StudentMatnProgresses
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => p.StudentId == studentId)
            .OrderBy(p => p.Matn.Order)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByMatnAsync(Guid matnId)
    {
        return await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => p.MatnId == matnId)
            .OrderBy(p => p.Student.StudentInfo.FirstName)
            .ThenBy(p => p.Student.StudentInfo.SecondName)
            .ToListAsync();
    }

    public async Task<IEnumerable<StudentMatnProgress>> GetByClassAsync(Guid classId)
    {
        var studentIds = await _context.Classes
            .Where(c => c.Id == classId)
            .SelectMany(c => c.Students.Select(s => s.UserId))
            .ToListAsync();

        return await _context.StudentMatnProgresses
            .Include(p => p.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(p => p.Matn)
                .ThenInclude(m => m.StudyProgram)
            .Include(p => p.LastUpdatedByTeacher)
                .ThenInclude(t => t!.TeacherInfo)
            .Where(p => studentIds.Contains(p.StudentId))
            .OrderBy(p => p.Student.StudentInfo.FirstName)
            .ThenBy(p => p.Student.StudentInfo.SecondName)
            .ThenBy(p => p.Matn.Order)
            .ToListAsync();
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
