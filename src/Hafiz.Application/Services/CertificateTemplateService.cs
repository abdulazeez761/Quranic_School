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

    public async Task<IReadOnlyList<CertificateTemplateVersionDto>> GetTemplateVersionsAsync(Guid templateId, Guid? instituteId = null)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || (instituteId.HasValue && template.InstituteId != instituteId.Value))
            return Array.Empty<CertificateTemplateVersionDto>();

        var versions = await _templates.GetVersionsByTemplateIdAsync(templateId);
        var certCounts = await _templates.GetIssuedCertificatesCountPerVersionAsync(templateId);
        var userIds = versions.Where(v => v.CreatedBy.HasValue).Select(v => v.CreatedBy!.Value);
        var userNames = await _templates.GetUserNamesAsync(userIds);

        return versions.Select(v => MapVersionToDto(v, template.CurrentVersion, certCounts, userNames)).ToList();
    }

    public async Task<CertificateTemplateVersionDto?> GetTemplateVersionAsync(Guid templateId, Guid versionId, Guid? instituteId = null)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || (instituteId.HasValue && template.InstituteId != instituteId.Value))
            return null;

        var version = await _templates.GetVersionAsync(versionId);
        if (version is null || version.TemplateId != templateId)
            return null;

        var certCounts = await _templates.GetIssuedCertificatesCountPerVersionAsync(templateId);
        var userNames = version.CreatedBy.HasValue
            ? await _templates.GetUserNamesAsync(new[] { version.CreatedBy.Value })
            : new Dictionary<Guid, string>();

        return MapVersionToDto(version, template.CurrentVersion, certCounts, userNames);
    }

    public async Task<bool> RestoreVersionAsync(Guid templateId, Guid versionId, Guid? userId = null, Guid? instituteId = null)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || (instituteId.HasValue && template.InstituteId != instituteId.Value))
            return false;

        var versionToRestore = await _templates.GetVersionAsync(versionId);
        if (versionToRestore is null || versionToRestore.TemplateId != template.Id)
            return false;

        var config = ParseConfiguration(versionToRestore.ConfigurationJson, template.Type);

        template.CurrentVersion++;
        template.UpdatedAt = DateTime.UtcNow;
        template.UpdatedBy = userId;

        var updated = await _templates.UpdateAsync(template);
        if (!updated) return false;

        await _templates.AddVersionAsync(new CertificateTemplateVersion
        {
            TemplateId = template.Id,
            VersionNumber = template.CurrentVersion,
            ConfigurationJson = config.ToJson(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        });

        return true;
    }

    public async Task<CertificateTemplateDto?> DuplicateTemplateAsync(Guid templateId, string? newName = null, Guid? userId = null, Guid? instituteId = null)
    {
        var source = await _templates.GetByIdAsync(templateId, includeVersions: true);
        if (source is null || (instituteId.HasValue && source.InstituteId != instituteId.Value))
            return null;

        var latestVersion = await _templates.GetLatestVersionAsync(templateId);
        var name = string.IsNullOrWhiteSpace(newName) ? $"{source.Name} (نسخة)" : newName.Trim();

        return await CreateTemplateAsync(new CreateTemplateDto
        {
            InstituteId = source.InstituteId,
            Name = name,
            Description = source.Description,
            Type = source.Type,
            IsDefault = false,
            InitialConfigurationJson = latestVersion?.ConfigurationJson
        }, userId);
    }

    public async Task<bool> ToggleTemplateActiveAsync(Guid templateId, Guid? instituteId = null)
    {
        var template = await _templates.GetByIdAsync(templateId);
        if (template is null || (instituteId.HasValue && template.InstituteId != instituteId.Value))
            return false;

        template.IsActive = !template.IsActive;
        template.UpdatedAt = DateTime.UtcNow;
        return await _templates.UpdateAsync(template);
    }

    private static CertificateTemplateVersionDto MapVersionToDto(
        CertificateTemplateVersion version,
        int currentVersionNumber,
        IReadOnlyDictionary<Guid, int> certCounts,
        IReadOnlyDictionary<Guid, string> userNames)
    {
        var parsed = TemplateConfiguration.FromJson(version.ConfigurationJson);
        var userDisplay = version.CreatedBy.HasValue && userNames.TryGetValue(version.CreatedBy.Value, out var name)
            ? name
            : null;

        return new CertificateTemplateVersionDto
        {
            Id = version.Id,
            TemplateId = version.TemplateId,
            VersionNumber = version.VersionNumber,
            ConfigurationJson = version.ConfigurationJson,
            CreatedAt = version.CreatedAt,
            CreatedAtFormatted = version.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
            CreatedBy = version.CreatedBy,
            CreatedByName = userDisplay,
            CertificatesIssuedCount = certCounts.GetValueOrDefault(version.Id, 0),
            IsCurrent = (version.VersionNumber == currentVersionNumber),
            PrimaryColor = parsed.Theme?.PrimaryColor ?? "#C59B27",
            SecondaryColor = parsed.Theme?.SecondaryColor ?? "#064E3B",
            AccentColor = parsed.Theme?.AccentColor ?? "#DFBA69",
            BackgroundColor = parsed.Theme?.BackgroundColor ?? "#FDFBF7",
            Orientation = parsed.Layout?.Orientation ?? "landscape",
            PageSize = parsed.Layout?.PageSize ?? "A4",
            SectionCount = parsed.Sections?.Count(s => s.Enabled) ?? 0
        };
    }

    public async Task SeedDefaultTemplatesForInstituteAsync(Guid instituteId, Guid? createdBy = null)
    {
        var items = new[]
        {
            (Type: CertificateType.Quran, Name: "القالب الافتراضي لشهادة القرآن"),
            (Type: CertificateType.Matn, Name: "القالب الافتراضي لشهادة المتن"),
            (Type: CertificateType.General, Name: "القالب الافتراضي لشهادة الشكر والتقدير")
        };

        foreach (var item in items)
        {
            if (await _templates.GetDefaultTemplateAsync(instituteId, item.Type) is not null) continue;
            await CreateTemplateAsync(new CreateTemplateDto
            {
                InstituteId = instituteId,
                Type = item.Type,
                IsDefault = true,
                Name = item.Name
            }, createdBy);
        }
    }

    private static TemplateConfiguration ParseConfiguration(string? json, CertificateType type)
    {
        var fallback = DefaultTemplateConfigurations.GetDefault(type);
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        try
        {
            using var document = JsonDocument.Parse(json);
            var configuration = TemplateConfiguration.FromJson(document.RootElement.GetRawText());
            // A configuration with no sections would print an empty certificate, so keep the built-in ones.
            if (configuration.Sections.Count == 0)
                configuration.Sections = fallback.Sections;
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
            var theme = TemplateConfiguration.FromJson(
                template.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault()?.ConfigurationJson).Theme;
            result.Add(new CertificateTemplateSummaryDto
            {
                PrimaryColor = theme.PrimaryColor,
                SecondaryColor = theme.SecondaryColor,
                AccentColor = theme.AccentColor,
                BackgroundColor = theme.BackgroundColor,
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
