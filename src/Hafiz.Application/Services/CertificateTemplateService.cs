using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Application.DTO.Certificate.TemplateConfiguration;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.Repositories.Interfaces;

namespace Hafiz.Application.Services;

public class CertificateTemplateService : ICertificateTemplateService
{
    private readonly ICertificateTemplateRepository _templates;

    public CertificateTemplateService(ICertificateTemplateRepository templates) => _templates = templates;

    public async Task<CertificateTemplateDto?> GetTemplateByIdAsync(Guid id)
    {
        var template = await _templates.GetByIdAsync(id, includeVersions: true);
        return template is null ? null : ToDto(template);
    }

    public async Task<IEnumerable<CertificateTemplateSummaryDto>> GetTemplatesByInstituteAsync(Guid instituteId, CertificateType? type = null)
    {
        var templates = await _templates.GetByInstituteAsync(instituteId, type);
        return await ToSummariesAsync(templates);
    }

    public async Task<IEnumerable<CertificateTemplateSummaryDto>> GetAllTemplatesAsync(CertificateType? type = null)
    {
        var templates = await _templates.GetAllAsync(type);
        return await ToSummariesAsync(templates);
    }

    public async Task<CertificateTemplateDto> GetEffectiveTemplateAsync(Guid instituteId, CertificateType type)
    {
        var template = await _templates.GetDefaultTemplateAsync(instituteId, type);
        if (template is null)
        {
            await SeedDefaultTemplatesForInstituteAsync(instituteId);
            template = await _templates.GetDefaultTemplateAsync(instituteId, type);
        }

        if (template is null)
            throw new InvalidOperationException("No active certificate template is available for this institute.");

        return ToDto(template);
    }

    public async Task<CertificateTemplateDto> CreateTemplateAsync(CreateTemplateDto dto, Guid? createdBy = null)
    {
        if (dto.InstituteId == Guid.Empty) throw new ArgumentException("Institute is required.", nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Template name is required.", nameof(dto));

        var configuration = ParseConfiguration(dto.InitialConfigurationJson, dto.Type);
        var template = new CertificateTemplate
        {
            InstituteId = dto.InstituteId,
            Name = dto.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            Type = dto.Type,
            IsDefault = dto.IsDefault,
            CurrentVersion = 1,
            CreatedBy = createdBy
        };

        if (template.IsDefault)
            await _templates.ClearDefaultFlagAsync(template.InstituteId, template.Type);

        await _templates.AddAsync(template);
        await _templates.AddVersionAsync(new CertificateTemplateVersion
        {
            TemplateId = template.Id,
            VersionNumber = 1,
            ConfigurationJson = configuration.ToJson(),
            CreatedBy = createdBy
        });

        return (await GetTemplateByIdAsync(template.Id))!;
    }

    public async Task<bool> UpdateTemplateConfigAsync(UpdateTemplateConfigDto dto, Guid? updatedBy = null)
    {
        var template = await _templates.GetByIdAsync(dto.TemplateId, includeVersions: true);
        if (template is null) return false;

        var configuration = ParseConfiguration(dto.ConfigurationJson, template.Type);
        if (dto.IsDefault)
            await _templates.ClearDefaultFlagAsync(template.InstituteId, template.Type, template.Id);

        template.Name = dto.Name.Trim();
        template.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        template.IsDefault = dto.IsDefault;
        template.IsActive = dto.IsActive;
        template.CurrentVersion++;
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = updatedBy;

        var updated = await _templates.UpdateAsync(template);
        if (!updated) return false;

        await _templates.AddVersionAsync(new CertificateTemplateVersion
        {
            TemplateId = template.Id,
            VersionNumber = template.CurrentVersion,
            ConfigurationJson = configuration.ToJson(),
            CreatedBy = updatedBy
        });
        return true;
    }

    public async Task<bool> SetDefaultTemplateAsync(Guid templateId, Guid instituteId)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || template.InstituteId != instituteId) return false;

        await _templates.ClearDefaultFlagAsync(instituteId, template.Type, template.Id);
        template.IsDefault = true;
        template.IsActive = true;
        template.UpdatedAt = DateTime.UtcNow;
        return await _templates.UpdateAsync(template);
    }

    public async Task<bool> DeleteTemplateAsync(Guid templateId, Guid? instituteId = null)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || (instituteId.HasValue && template.InstituteId != instituteId.Value)) return false;
        if (await _templates.GetIssuedCertificatesCountAsync(templateId) > 0) return false;
        return await _templates.DeleteAsync(templateId);
    }

    public async Task SeedDefaultTemplatesForInstituteAsync(Guid instituteId, Guid? createdBy = null)
    {
        foreach (var type in new[] { CertificateType.Quran, CertificateType.Matn })
        {
            if (await _templates.GetDefaultTemplateAsync(instituteId, type) is not null) continue;
            await CreateTemplateAsync(new CreateTemplateDto
            {
                InstituteId = instituteId,
                Type = type,
                IsDefault = true,
                Name = type == CertificateType.Quran ? "القالب الافتراضي لشهادة القرآن" : "القالب الافتراضي لشهادة المتن"
            }, createdBy);
        }
    }

    private static TemplateConfiguration ParseConfiguration(string? json, CertificateType type)
    {
        if (string.IsNullOrWhiteSpace(json)) return DefaultTemplateConfigurations.GetDefault(type);
        try
        {
            using var document = JsonDocument.Parse(json);
            var configuration = TemplateConfiguration.FromJson(document.RootElement.GetRawText());
            return configuration;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Template configuration must be valid JSON.", nameof(json), ex);
        }
    }

    private async Task<IEnumerable<CertificateTemplateSummaryDto>> ToSummariesAsync(IEnumerable<CertificateTemplate> templates)
    {
        var result = new List<CertificateTemplateSummaryDto>();
        foreach (var template in templates)
        {
            result.Add(new CertificateTemplateSummaryDto
            {
                Id = template.Id,
                InstituteId = template.InstituteId,
                InstituteName = template.Institute?.Name ?? string.Empty,
                Name = template.Name,
                Description = template.Description,
                Type = template.Type,
                IsDefault = template.IsDefault,
                IsActive = template.IsActive,
                CurrentVersion = template.CurrentVersion,
                CertificatesIssuedCount = await _templates.GetIssuedCertificatesCountAsync(template.Id),
                CreatedAt = template.CreatedAt,
                UpdatedAt = template.UpdatedAt
            });
        }
        return result;
    }

    private static CertificateTemplateDto ToDto(CertificateTemplate template)
    {
        var version = template.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        return new CertificateTemplateDto
        {
            Id = template.Id, InstituteId = template.InstituteId, InstituteName = template.Institute?.Name ?? string.Empty,
            Name = template.Name, Description = template.Description, Type = template.Type,
            IsDefault = template.IsDefault, IsActive = template.IsActive, CurrentVersion = template.CurrentVersion,
            CurrentVersionId = version?.Id ?? Guid.Empty,
            Configuration = TemplateConfiguration.FromJson(version?.ConfigurationJson),
            CreatedAt = template.CreatedAt, UpdatedAt = template.UpdatedAt
        };
    }
}
