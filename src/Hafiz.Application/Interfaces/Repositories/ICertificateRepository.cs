using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface ICertificateRepository
{
    Task<Certificate?> GetByIdAsync(Guid id, bool includeDetails = true);
    Task<Certificate?> GetByVerificationTokenAsync(string token);
    Task<Certificate?> GetByNumberAsync(Guid instituteId, string certificateNumber);
    Task<Certificate?> GetBySourceEntityAsync(Guid sourceEntityId);
    Task<Certificate?> GetLatestForStudentAsync(Guid studentId, CertificateType type);
    Task<IEnumerable<Certificate>> GetHistoryAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 50);
    Task<int> GetCountAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null);
    Task<Certificate> AddAsync(Certificate certificate);
    Task<bool> UpdateAsync(Certificate certificate);
    Task<int> GetNextSequenceAsync(Guid instituteId, CertificateType type);
    Task AddAuditLogAsync(CertificateAuditLog auditLog);
    Task<IEnumerable<CertificateAuditLog>> GetAuditLogsAsync(Guid certificateId);
}
