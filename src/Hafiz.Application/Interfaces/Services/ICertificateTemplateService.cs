using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.Interfaces.Services;

public interface ICertificateTemplateService
{
    Task<CertificateTemplateDto?> GetTemplateByIdAsync(Guid id);
    Task<IEnumerable<CertificateTemplateSummaryDto>> GetTemplatesByInstituteAsync(Guid instituteId, CertificateType? type = null);
    Task<IEnumerable<CertificateTemplateSummaryDto>> GetAllTemplatesAsync(CertificateType? type = null);
    Task<CertificateTemplateDto> GetEffectiveTemplateAsync(Guid instituteId, CertificateType type);
    Task<CertificateTemplateDto> CreateTemplateAsync(CreateTemplateDto dto, Guid? createdBy = null);
    Task<bool> UpdateTemplateConfigAsync(UpdateTemplateConfigDto dto, Guid? updatedBy = null);
    Task<bool> SetDefaultTemplateAsync(Guid templateId, Guid instituteId);
    Task<bool> DeleteTemplateAsync(Guid templateId, Guid? instituteId = null);
    Task<IReadOnlyList<CertificateTemplateVersionDto>> GetTemplateVersionsAsync(Guid templateId, Guid? instituteId = null);
    Task<CertificateTemplateVersionDto?> GetTemplateVersionAsync(Guid templateId, Guid versionId, Guid? instituteId = null);
    Task<bool> RestoreVersionAsync(Guid templateId, Guid versionId, Guid? userId = null, Guid? instituteId = null);
    Task<CertificateTemplateDto?> DuplicateTemplateAsync(Guid templateId, string? newName = null, Guid? userId = null, Guid? instituteId = null);
    Task<bool> ToggleTemplateActiveAsync(Guid templateId, Guid? instituteId = null);
    Task SeedDefaultTemplatesForInstituteAsync(Guid instituteId, Guid? createdBy = null);
}
