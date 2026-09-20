using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
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
        int pageSize = 50,
        string? sort = null);
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

    /// <summary>
    /// Register-wide counts for the certificates management tiles. The filters mirror
    /// <see cref="GetCountAsync"/> so the tiles describe the set currently on screen.
    /// </summary>
    Task<CertificateStats> GetStatsAsync(
        Guid? instituteId = null,
        DateTime? issuedSinceUtc = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null);

    /// <summary>Owning institute of one certificate, or null when it does not exist.</summary>
    Task<Guid?> GetInstituteIdAsync(Guid certificateId);

    /// <summary>Display names for the given user ids; ids with no user are simply absent.</summary>
    Task<IReadOnlyDictionary<Guid, string>> GetUserNamesAsync(IEnumerable<Guid> userIds);
}
