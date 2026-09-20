using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.Interfaces.Services;

public interface ICertificateIssuanceService
{
    Task<CertificateModel?> IssueMatnCertificateAsync(Guid studentMatnProgressId, Guid? templateId = null, Guid? issuedBy = null, string? baseUrl = null);
    Task<CertificateModel?> IssueQuranCertificateAsync(Guid studentId, int? fromJuz = null, int? toJuz = null, Guid? templateId = null, Guid? issuedBy = null, string? baseUrl = null);
    Task<CertificateModel?> GetCertificateModelAsync(Guid certificateId, string? baseUrl = null);
    Task<CertificateModel?> GetCertificateModelByVerificationTokenAsync(string token, string? baseUrl = null);
    Task<CertificateVerificationDto> VerifyCertificateAsync(string token, string? baseUrl = null);
    Task<bool> RevokeCertificateAsync(Guid certificateId, string reason, Guid revokedBy, Guid? instituteId = null);
    Task<IEnumerable<CertificateListItemDto>> GetHistoryAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 50);
    Task<int> GetHistoryCountAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null);
}
