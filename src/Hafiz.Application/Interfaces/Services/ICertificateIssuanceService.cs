using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.Interfaces.Services;

public interface ICertificateIssuanceService
{
    Task<CertificateModel?> IssueMatnCertificateAsync(
        Guid studentMatnProgressId,
        Guid? templateId = null,
        Guid? issuedBy = null,
        string? baseUrl = null,
        Guid? instituteId = null
    );
    Task<CertificateModel?> IssueQuranCertificateAsync(
        Guid studentId,
        int? fromJuz = null,
        int? toJuz = null,
        Guid? templateId = null,
        Guid? issuedBy = null,
        string? baseUrl = null,
        Guid? instituteId = null,
        bool forceNew = false
    );
    Task<CertificateModel?> IssueGeneralCertificateAsync(
        Guid studentId,
        string? title = null,
        string? reason = null,
        Guid? templateId = null,
        Guid? issuedBy = null,
        string? baseUrl = null,
        bool forceNew = false,
        Guid? instituteId = null
    );
    Task<CertificateModel?> GetExistingMatnCertificateAsync(Guid studentMatnProgressId, string? baseUrl = null);
    Task<CertificateModel?> GetExistingQuranCertificateAsync(Guid studentId, int? fromJuz = null, int? toJuz = null, string? baseUrl = null);
    Task<CertificateModel?> GetExistingGeneralCertificateAsync(Guid studentId, string? title = null, string? baseUrl = null);
    Task<CertificateModel?> GetCertificateModelAsync(Guid certificateId, string? baseUrl = null);
    Task<CertificateModel?> GetCertificateModelByVerificationTokenAsync(
        string token,
        string? baseUrl = null
    );
    Task<CertificateVerificationDto> VerifyCertificateAsync(string token, string? baseUrl = null);
    Task<bool> RevokeCertificateAsync(
        Guid certificateId,
        string reason,
        Guid revokedBy,
        Guid? instituteId = null
    );
    Task<bool> DeleteCertificateAsync(
        Guid certificateId,
        Guid deletedBy,
        Guid? instituteId = null
    );
    Task<IEnumerable<CertificateListItemDto>> GetHistoryAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 50,
        string? sort = null
    );
    Task<int> GetHistoryCountAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null
    );

    /// <summary>Counts for the management tiles, scoped by the same filters as the list.</summary>
    Task<CertificateStats> GetStatsAsync(
        Guid? instituteId = null,
        DateTime? issuedSinceUtc = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null
    );

    /// <summary>
    /// Audit trail for one certificate with performer names resolved, newest first.
    /// Returns null when the certificate does not belong to <paramref name="instituteId"/>.
    /// </summary>
    Task<IReadOnlyList<CertificateAuditLogDto>?> GetAuditTrailAsync(
        Guid certificateId,
        Guid? instituteId = null
    );
}
