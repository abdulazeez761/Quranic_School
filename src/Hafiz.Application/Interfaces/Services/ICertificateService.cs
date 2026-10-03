using System;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;

namespace Hafiz.Application.Interfaces.Services;

public interface ICertificateService
{
    Task<CertificateModel?> GetMatnCertificateAsync(Guid studentMatnProgressId);
    Task<CertificateModel?> GetQuranCertificateAsync(Guid studentId, int? fromJuz = null, int? toJuz = null);
    Task<CertificateModel?> GetGeneralCertificateAsync(Guid studentId, string? title = null, string? reason = null);
}
