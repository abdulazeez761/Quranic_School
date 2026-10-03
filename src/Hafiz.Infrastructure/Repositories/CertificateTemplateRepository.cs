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

public class CertificateTemplateRepository : ICertificateTemplateRepository
{
    private readonly ApplicationDbContext _context;

    public CertificateTemplateRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CertificateTemplate?> GetByIdAsync(Guid id, bool includeVersions = false)
    {
        var query = _context.CertificateTemplates
            .Include(t => t.Institute)
            .AsQueryable();

        if (includeVersions)
        {
            query = query.Include(t => t.Versions.OrderByDescending(v => v.VersionNumber));
        }

        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<CertificateTemplate?> GetDefaultTemplateAsync(Guid instituteId, CertificateType type)
    {
        return await _context.CertificateTemplates
            .Include(t => t.Institute)
            .Include(t => t.Versions.OrderByDescending(v => v.VersionNumber))
            .Where(t => t.InstituteId == instituteId && t.Type == type && t.IsActive)
            .OrderByDescending(t => t.IsDefault)
            .ThenByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CertificateTemplate>> GetByInstituteAsync(Guid instituteId, CertificateType? type = null)
    {
        var query = _context.CertificateTemplates
            .Include(t => t.Institute)
            .Include(t => t.Versions.OrderByDescending(v => v.VersionNumber))
            .Where(t => t.InstituteId == instituteId);

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        return await query.OrderByDescending(t => t.IsDefault)
            .ThenBy(t => t.Type)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<CertificateTemplate>> GetAllAsync(CertificateType? type = null)
    {
        var query = _context.CertificateTemplates
            .Include(t => t.Institute)
            .Include(t => t.Versions.OrderByDescending(v => v.VersionNumber))
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        return await query.OrderBy(t => t.Institute.Name)
            .ThenBy(t => t.Type)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<CertificateTemplate> AddAsync(CertificateTemplate template)
    {
        await _context.CertificateTemplates.AddAsync(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> UpdateAsync(CertificateTemplate template)
    {
        // Mark only the template itself: Update() would walk the graph and rewrite the
        // included Institute row (and every version) from possibly stale values.
        _context.Entry(template).State = EntityState.Modified;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var template = await _context.CertificateTemplates.FindAsync(id);
        if (template == null) return false;

        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<CertificateTemplateVersion> AddVersionAsync(CertificateTemplateVersion version)
    {
        await _context.CertificateTemplateVersions.AddAsync(version);
        await _context.SaveChangesAsync();
        return version;
    }

    public async Task<CertificateTemplateVersion?> GetVersionAsync(Guid versionId)
    {
        return await _context.CertificateTemplateVersions
            .Include(v => v.Template)
            .FirstOrDefaultAsync(v => v.Id == versionId);
    }

    public async Task<CertificateTemplateVersion?> GetLatestVersionAsync(Guid templateId)
    {
        return await _context.CertificateTemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<CertificateTemplateVersion>> GetVersionsByTemplateIdAsync(Guid templateId)
    {
        return await _context.CertificateTemplateVersions
            .Where(v => v.TemplateId == templateId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync();
    }

    public async Task<int> GetIssuedCertificatesCountAsync(Guid templateId)
    {
        return await _context.Certificates.CountAsync(c => c.TemplateId == templateId && !c.IsDeleted);
    }

    public async Task<Dictionary<Guid, int>> GetIssuedCertificatesCountPerVersionAsync(Guid templateId)
    {
        return await _context.Certificates
            .Where(c => c.TemplateId == templateId && !c.IsDeleted)
            .GroupBy(c => c.TemplateVersionId)
            .Select(g => new { VersionId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.VersionId, x => x.Count);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<Guid, string>();

        var users = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.SecondName })
            .ToListAsync();

        return users.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.SecondName}".Trim());
    }

    public async Task ClearDefaultFlagAsync(Guid instituteId, CertificateType type, Guid? exceptTemplateId = null)
    {
        // Soft-deleted templates are hidden by the global query filter, but the filtered unique
        // index on (InstituteId, Type, IsDefault) still covers them. Deleting a template leaves
        // IsDefault = 1 behind, so without IgnoreQueryFilters that stale row blocks every later
        // set-default with a duplicate key violation.
        var defaults = await _context.CertificateTemplates
            .IgnoreQueryFilters()
            .Where(t => t.InstituteId == instituteId && t.Type == type && t.IsDefault && (!exceptTemplateId.HasValue || t.Id != exceptTemplateId.Value))
            .ToListAsync();

        foreach (var def in defaults)
        {
            def.IsDefault = false;
        }

        if (defaults.Count > 0)
        {
            await _context.SaveChangesAsync();
        }
    }
}
