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

public class CertificateRepository : ICertificateRepository
{
    private readonly ApplicationDbContext _context;

    public CertificateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Certificate?> GetByIdAsync(Guid id, bool includeDetails = true)
    {
        var query = _context.Certificates.AsQueryable();

        if (includeDetails)
        {
            query = query
                .Include(c => c.Institute)
                .Include(c => c.Student)
                    .ThenInclude(s => s.StudentInfo)
                .Include(c => c.Template)
                .Include(c => c.TemplateVersion);
        }

        return await query.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Certificate?> GetByVerificationTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;

        return await _context.Certificates
            .Include(c => c.Institute)
            .Include(c => c.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(c => c.Template)
            .Include(c => c.TemplateVersion)
            .FirstOrDefaultAsync(c => c.VerificationToken == token);
    }

    public async Task<Certificate?> GetByNumberAsync(Guid instituteId, string certificateNumber)
    {
        return await _context.Certificates
            .Include(c => c.Institute)
            .Include(c => c.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(c => c.Template)
            .Include(c => c.TemplateVersion)
            .FirstOrDefaultAsync(c => c.InstituteId == instituteId && c.CertificateNumber == certificateNumber);
    }

    public async Task<Certificate?> GetBySourceEntityAsync(Guid sourceEntityId)
    {
        return await _context.Certificates
            .Include(c => c.Institute)
            .Include(c => c.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(c => c.Template)
            .Include(c => c.TemplateVersion)
            .FirstOrDefaultAsync(c => c.SourceEntityId == sourceEntityId && c.Status == CertificateStatus.Active);
    }

    public async Task<Certificate?> GetLatestForStudentAsync(Guid studentId, CertificateType type)
    {
        return await _context.Certificates
            .Include(c => c.Institute)
            .Include(c => c.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(c => c.Template)
            .Include(c => c.TemplateVersion)
            .Where(c => c.StudentId == studentId && c.Type == type && c.Status == CertificateStatus.Active)
            .OrderByDescending(c => c.IssuedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Certificate>> GetHistoryAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 50)
    {
        var query = BuildFilterQuery(instituteId, studentId, type, status, search);

        return await query
            .Include(c => c.Institute)
            .Include(c => c.Student)
                .ThenInclude(s => s.StudentInfo)
            .Include(c => c.Template)
            .OrderByDescending(c => c.IssuedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null)
    {
        var query = BuildFilterQuery(instituteId, studentId, type, status, search);
        return await query.CountAsync();
    }

    private IQueryable<Certificate> BuildFilterQuery(
        Guid? instituteId,
        Guid? studentId,
        CertificateType? type,
        CertificateStatus? status,
        string? search)
    {
        var query = _context.Certificates.AsQueryable();

        if (instituteId.HasValue)
        {
            query = query.Where(c => c.InstituteId == instituteId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(c => c.StudentId == studentId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.CertificateNumber.Contains(term) ||
                c.Student.StudentInfo.FirstName.Contains(term) ||
                c.Student.StudentInfo.SecondName.Contains(term) ||
                c.Student.StudentInfo.Username.Contains(term)
            );
        }

        return query;
    }

    public async Task<Certificate> AddAsync(Certificate certificate)
    {
        await _context.Certificates.AddAsync(certificate);
        await _context.SaveChangesAsync();
        return certificate;
    }

    public async Task<bool> UpdateAsync(Certificate certificate)
    {
        _context.Certificates.Update(certificate);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<int> GetNextSequenceAsync(Guid instituteId, CertificateType type)
    {
        var count = await _context.Certificates
            .IgnoreQueryFilters()
            .CountAsync(c => c.InstituteId == instituteId && c.Type == type);

        return count + 1;
    }

    public async Task AddAuditLogAsync(CertificateAuditLog auditLog)
    {
        await _context.CertificateAuditLogs.AddAsync(auditLog);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<CertificateAuditLog>> GetAuditLogsAsync(Guid certificateId)
    {
        return await _context.CertificateAuditLogs
            .Where(l => l.EntityId == certificateId)
            .OrderByDescending(l => l.PerformedAt)
            .ToListAsync();
    }
}
