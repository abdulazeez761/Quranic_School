using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;

namespace Hafiz.Repositories.Interfaces;

public interface ICertificateTemplateRepository
{
    Task<CertificateTemplate?> GetByIdAsync(Guid id, bool includeVersions = false);
    Task<CertificateTemplate?> GetDefaultTemplateAsync(Guid instituteId, CertificateType type);
    Task<IEnumerable<CertificateTemplate>> GetByInstituteAsync(Guid instituteId, CertificateType? type = null);
    Task<IEnumerable<CertificateTemplate>> GetAllAsync(CertificateType? type = null);
    Task<CertificateTemplate> AddAsync(CertificateTemplate template);
    Task<bool> UpdateAsync(CertificateTemplate template);
    Task<bool> DeleteAsync(Guid id);
    Task<CertificateTemplateVersion> AddVersionAsync(CertificateTemplateVersion version);
    Task<CertificateTemplateVersion?> GetVersionAsync(Guid versionId);
    Task<CertificateTemplateVersion?> GetLatestVersionAsync(Guid templateId);
    Task<int> GetIssuedCertificatesCountAsync(Guid templateId);
    Task ClearDefaultFlagAsync(Guid instituteId, CertificateType type, Guid? exceptTemplateId = null);
}
