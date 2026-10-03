using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Application.DTO.Certificate.TemplateConfiguration;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Common.Helper;
using Hafiz.Domain.Entities;
using Hafiz.Domain.Enums;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;

namespace Hafiz.Application.Services;

/// <summary>Creates immutable, institute-scoped certificate records from validated achievements.</summary>
public class CertificateIssuanceService : ICertificateIssuanceService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { PropertyNameCaseInsensitive = true };
    private readonly ICertificateRepository _certificates;
    private readonly ICertificateTemplateRepository _templates;
    private readonly IStudentMatnProgressRepository _progresses;
    private readonly IStudentRepository _students;
    private readonly ICertificateService _legacyCertificates;
    private readonly IQRCodeService _qrCodes;

    public CertificateIssuanceService(
        ICertificateRepository certificates,
        ICertificateTemplateRepository templates,
        IStudentMatnProgressRepository progresses,
        IStudentRepository students,
        ICertificateService legacyCertificates,
        IQRCodeService qrCodes
    )
    {
        _certificates = certificates;
        _templates = templates;
        _progresses = progresses;
        _students = students;
        _legacyCertificates = legacyCertificates;
        _qrCodes = qrCodes;
    }

    public async Task<CertificateModel?> IssueMatnCertificateAsync(
        Guid studentMatnProgressId,
        Guid? templateId = null,
        Guid? issuedBy = null,
        string? baseUrl = null
    )
    {
        var progress = await _progresses.GetByIdAsync(studentMatnProgressId);
        var instituteId = progress?.Student?.StudentInfo?.InstituteId;
        if (progress is null || !instituteId.HasValue)
            return null;

        var completed =
            progress.StudyStatus == StudyStatus.Completed
            || progress.MemorizationStatus == MemorizationStatus.Memorized
            || progress.ExamStatus == ExamStatus.Passed;
        if (!completed)
            return null;

        var existing = await _certificates.GetBySourceEntityAsync(progress.Id);
        if (existing is not null)
        {
            var model = ToModel(existing, baseUrl);
            model.IsExistingCertificate = true;
            return model;
        }

        var data = await _legacyCertificates.GetMatnCertificateAsync(progress.Id);
        return data is null
            ? null
            : await PersistAsync(
                data,
                instituteId.Value,
                CertificateType.Matn,
                progress.Id,
                templateId,
                issuedBy,
                baseUrl
            );
    }

    public async Task<CertificateModel?> GetExistingMatnCertificateAsync(Guid studentMatnProgressId, string? baseUrl = null)
    {
        var existing = await _certificates.GetBySourceEntityAsync(studentMatnProgressId);
        if (existing is null || existing.Status != CertificateStatus.Active)
            return null;

        var model = ToModel(existing, baseUrl);
        model.IsExistingCertificate = true;
        return model;
    }

    public async Task<CertificateModel?> GetExistingQuranCertificateAsync(Guid studentId, int? fromJuz = null, int? toJuz = null, string? baseUrl = null)
    {
        var existing = await _certificates.GetLatestForStudentAsync(studentId, CertificateType.Quran);
        if (existing is null || existing.Status != CertificateStatus.Active)
            return null;

        var model = ToModel(existing, baseUrl);
        if (fromJuz.HasValue && toJuz.HasValue)
        {
            var matches = (model.FromJuz == fromJuz && model.ToJuz == toJuz) ||
                          (fromJuz == 1 && toJuz == 30 && (!model.FromJuz.HasValue || model.FromJuz == 1) && (!model.ToJuz.HasValue || model.ToJuz == 30));
            if (!matches)
                return null;
        }

        model.IsExistingCertificate = true;
        return model;
    }

    public async Task<CertificateModel?> IssueQuranCertificateAsync(
        Guid studentId,
        int? fromJuz = null,
        int? toJuz = null,
        Guid? templateId = null,
        Guid? issuedBy = null,
        string? baseUrl = null
    )
    {
        var student = await _students.GetByIdAsync(studentId);
        var instituteId = student?.StudentInfo?.InstituteId;
        if (student is null || !instituteId.HasValue)
            return null;
        if (!IsValidQuranAchievement(student, fromJuz, toJuz))
            return null;

        var existing = await _certificates.GetLatestForStudentAsync(studentId, CertificateType.Quran);
        if (existing is not null && existing.Status == CertificateStatus.Active)
        {
            var existingModel = ToModel(existing, baseUrl);
            if (existingModel is not null)
            {
                var matchesRange = (!fromJuz.HasValue && !toJuz.HasValue) ||
                                   (existingModel.FromJuz == fromJuz && existingModel.ToJuz == toJuz) ||
                                   (fromJuz == 1 && toJuz == 30 && (!existingModel.FromJuz.HasValue || existingModel.FromJuz == 1) && (!existingModel.ToJuz.HasValue || existingModel.ToJuz == 30));

                if (matchesRange)
                {
                    existingModel.IsExistingCertificate = true;
                    return existingModel;
                }
            }
        }

        var data = await _legacyCertificates.GetQuranCertificateAsync(studentId, fromJuz, toJuz);
        return data is null
            ? null
            : await PersistAsync(
                data,
                instituteId.Value,
                CertificateType.Quran,
                null,
                templateId,
                issuedBy,
                baseUrl
            );
    }

    public async Task<CertificateModel?> GetCertificateModelAsync(
        Guid certificateId,
        string? baseUrl = null
    )
    {
        var certificate = await _certificates.GetByIdAsync(certificateId);
        if (certificate is null)
            return null;

        var model = ToModel(certificate, baseUrl);
        model.IsExistingCertificate = true;
        return model;
    }

    public async Task<CertificateModel?> GetCertificateModelByVerificationTokenAsync(
        string token,
        string? baseUrl = null
    )
    {
        var certificate = await _certificates.GetByVerificationTokenAsync(token);
        if (certificate is null)
            return null;

        var model = ToModel(certificate, baseUrl);
        model.IsExistingCertificate = true;
        return model;
    }

    public async Task<CertificateVerificationDto> VerifyCertificateAsync(
        string token,
        string? baseUrl = null
    )
    {
        var certificate = await _certificates.GetByVerificationTokenAsync(token);
        if (certificate is null)
        {
            return new CertificateVerificationDto
            {
                IsFound = false,
                StatusMessage = "لم يتم العثور على شهادة بهذا الرمز.",
            };
        }

        var model = ToModel(certificate, baseUrl);
        return new CertificateVerificationDto
        {
            IsFound = true,
            Status = certificate.Status,
            StatusMessage =
                certificate.Status == CertificateStatus.Active
                    ? "الشهادة صحيحة وسارية."
                    : "هذه الشهادة ملغاة أو غير سارية.",
            CertificateId = certificate.Id,
            CertificateNumber = certificate.CertificateNumber,
            StudentName = model.StudentName,
            InstituteName = model.InstituteName,
            InstituteLogoUrl = model.InstituteLogoUrl,
            Type = certificate.Type,
            CertificateTitle = model.CertificateTitle,
            AchievementDescription = model.AchievementDescription,
            Score = model.Score,
            Rating = model.Rating,
            IssuedAtUserTime = certificate.IssuedAt,
            HijriDateDisplay = model.HijriDateFormatted,
            TeacherName = model.TeacherName,
            DirectorName = model.DirectorName,
            VerificationToken = certificate.VerificationToken,
            VerificationUrl = model.VerificationUrl,
            CertificateViewUrl = BuildUrl(baseUrl, $"/Certificates/View/{certificate.Id}"),
            RevokedAtUserTime = certificate.RevokedAt,
            RevokeReason = certificate.RevokeReason,
        };
    }

    public async Task<bool> RevokeCertificateAsync(
        Guid certificateId,
        string reason,
        Guid revokedBy,
        Guid? instituteId = null
    )
    {
        var certificate = await _certificates.GetByIdAsync(certificateId, includeDetails: false);
        if (
            certificate is null
            || certificate.Status != CertificateStatus.Active
            || (instituteId.HasValue && certificate.InstituteId != instituteId.Value)
        )
            return false;

        certificate.Status = CertificateStatus.Revoked;
        certificate.RevokedAt = DateTime.UtcNow;
        certificate.RevokedBy = revokedBy;
        certificate.RevokeReason = string.IsNullOrWhiteSpace(reason)
            ? "تم إلغاء الشهادة."
            : reason.Trim();
        if (!await _certificates.UpdateAsync(certificate))
            return false;

        await _certificates.AddAuditLogAsync(
            new CertificateAuditLog
            {
                InstituteId = certificate.InstituteId,
                EntityId = certificate.Id,
                EntityType = nameof(Certificate),
                Action = CertificateAuditAction.Revoked,
                PerformedBy = revokedBy,
                Details = certificate.RevokeReason,
            }
        );
        return true;
    }

    public async Task<bool> DeleteCertificateAsync(
        Guid certificateId,
        Guid deletedBy,
        Guid? instituteId = null
    )
    {
        var certificate = await _certificates.GetByIdAsync(certificateId, includeDetails: false);
        if (
            certificate is null
            || (instituteId.HasValue && certificate.InstituteId != instituteId.Value)
        )
            return false;

        certificate.IsDeleted = true;
        certificate.DeletedAt = DateTime.UtcNow;
        certificate.DeletedBy = deletedBy;
        if (!await _certificates.UpdateAsync(certificate))
            return false;

        await _certificates.AddAuditLogAsync(
            new CertificateAuditLog
            {
                InstituteId = certificate.InstituteId,
                EntityId = certificate.Id,
                EntityType = nameof(Certificate),
                Action = CertificateAuditAction.Deleted,
                PerformedBy = deletedBy,
                Details = "تم حذف الشهادة وأرشفتها من لوحة إدارة الشهادات.",
            }
        );
        return true;
    }

    public async Task<IEnumerable<CertificateListItemDto>> GetHistoryAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null,
        int page = 1,
        int pageSize = 50,
        string? sort = null
    )
    {
        var certificates = await _certificates.GetHistoryAsync(
            instituteId,
            studentId,
            type,
            status,
            search,
            Math.Max(1, page),
            Math.Clamp(pageSize, 1, 100),
            sort
        );
        return certificates.Select(c =>
        {
            var model = ToModel(c, null);
            return new CertificateListItemDto
            {
                Id = c.Id,
                CertificateNumber = c.CertificateNumber,
                StudentId = c.StudentId,
                StudentName = model.StudentName,
                InstituteId = c.InstituteId,
                InstituteName = model.InstituteName,
                Type = c.Type,
                SubjectTitle = model.SubjectName,
                TemplateName = c.Template?.Name ?? model.TemplateName,
                IssuedAtUserTime = c.IssuedAt,
                Status = c.Status,
                VerificationToken = c.VerificationToken,
                RevokedAtUserTime = c.RevokedAt,
                RevokeReason = c.RevokeReason,
            };
        });
    }

    public Task<int> GetHistoryCountAsync(
        Guid? instituteId = null,
        Guid? studentId = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null
    ) => _certificates.GetCountAsync(instituteId, studentId, type, status, search);

    public Task<CertificateStats> GetStatsAsync(
        Guid? instituteId = null,
        DateTime? issuedSinceUtc = null,
        CertificateType? type = null,
        CertificateStatus? status = null,
        string? search = null
    ) => _certificates.GetStatsAsync(instituteId, issuedSinceUtc, type, status, search);

    public async Task<IReadOnlyList<CertificateAuditLogDto>?> GetAuditTrailAsync(
        Guid certificateId,
        Guid? instituteId = null
    )
    {
        var owner = await _certificates.GetInstituteIdAsync(certificateId);
        // A certificate from another institute must look absent, not forbidden.
        if (owner is null || (instituteId.HasValue && owner.Value != instituteId.Value))
        {
            return null;
        }

        var logs = (await _certificates.GetAuditLogsAsync(certificateId)).ToList();
        var performerIds = logs.Where(l => l.PerformedBy.HasValue)
            .Select(l => l.PerformedBy!.Value);
        var names = await _certificates.GetUserNamesAsync(performerIds);

        return logs.Select(l => new CertificateAuditLogDto
            {
                Id = l.Id,
                Action = l.Action,
                Details = l.Details,
                PerformedAtUserTime = l.PerformedAt,
                PerformedByName =
                    l.PerformedBy.HasValue && names.TryGetValue(l.PerformedBy.Value, out var name)
                        ? name
                        : null,
            })
            .ToList();
    }

    private async Task<CertificateModel> PersistAsync(
        CertificateModel data,
        Guid instituteId,
        CertificateType type,
        Guid? sourceEntityId,
        Guid? templateId,
        Guid? issuedBy,
        string? baseUrl
    )
    {
        var template = await ResolveTemplateAsync(instituteId, type, templateId);
        var version =
            await _templates.GetLatestVersionAsync(template.Id)
            ?? throw new InvalidOperationException("Template has no configuration version.");
        var issuedAt = DateTime.UtcNow;
        var token = GenerateToken();
        data.CertificateId = Guid.NewGuid();
        data.CertificateNumber = await GenerateCertificateNumberAsync(
            instituteId,
            type,
            template.Institute?.Name
        );
        data.InstituteId = instituteId;
        data.InstituteName = template.Institute?.Name ?? data.InstituteName;
        data.InstituteLogoUrl = template.Institute?.Logo ?? data.InstituteLogoUrl;
        data.TemplateName = template.Name;
        data.Type = type;
        data.IssueDate = issuedAt;
        data.IssueDateFormatted = FormatArabicDate(issuedAt);
        data.GregorianDateFormatted =
            issuedAt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture) + " م";
        data.HijriDateFormatted = FormatHijriDate(issuedAt);
        data.VerificationToken = token;
        data.VerificationUrl = BuildUrl(baseUrl, $"/Certificates/Verify/{token}");
        data.Status = CertificateStatus.Active;
        data.Configuration = TemplateConfiguration.FromJson(version.ConfigurationJson);
        data.QrCodeDataUrl = _qrCodes.GenerateDataUri(data.VerificationUrl);

        var entity = new Certificate
        {
            Id = data.CertificateId.Value,
            InstituteId = instituteId,
            StudentId = data.StudentId,
            CertificateNumber = data.CertificateNumber,
            Type = type,
            TemplateId = template.Id,
            TemplateVersionId = version.Id,
            CertificateDataJson = JsonSerializer.Serialize(data, JsonOptions),
            SourceEntityId = sourceEntityId,
            VerificationToken = token,
            IssuedAt = issuedAt,
            IssuedBy = issuedBy,
        };
        await _certificates.AddAsync(entity);
        await _certificates.AddAuditLogAsync(
            new CertificateAuditLog
            {
                InstituteId = instituteId,
                EntityId = entity.Id,
                EntityType = nameof(Certificate),
                Action = CertificateAuditAction.Issued,
                PerformedBy = issuedBy,
                Details = $"Issued {entity.CertificateNumber}",
            }
        );
        return data;
    }

    private async Task<CertificateTemplate> ResolveTemplateAsync(
        Guid instituteId,
        CertificateType type,
        Guid? templateId
    )
    {
        var template = templateId.HasValue
            ? await _templates.GetByIdAsync(templateId.Value, includeVersions: true)
            : await _templates.GetDefaultTemplateAsync(instituteId, type);
        if (
            template is null
            || template.InstituteId != instituteId
            || template.Type != type
            || !template.IsActive
        )
            throw new InvalidOperationException(
                "An active template for this institute and certificate type is required."
            );
        return template;
    }

    private async Task<string> GenerateCertificateNumberAsync(
        Guid instituteId,
        CertificateType type,
        string? instituteName
    )
    {
        var prefix = type switch
        {
            CertificateType.Quran => "QRN",
            CertificateType.Matn => "MTN",
            CertificateType.Sanad => "SND",
            CertificateType.General => "GEN",
            _ => "CUS",
        };
        var shortName = new string(
            (instituteName ?? "INS").Where(char.IsLetterOrDigit).Take(3).ToArray()
        ).ToUpperInvariant();
        shortName = shortName.Length == 0 ? "INS" : shortName;
        return $"{prefix}-{shortName}-{await _certificates.GetNextSequenceAsync(instituteId, type):D5}";
    }

    private static bool IsValidQuranAchievement(
        Hafiz.Models.Student student,
        int? fromJuz,
        int? toJuz
    )
    {
        if (!fromJuz.HasValue && !toJuz.HasValue)
            return student.MemorizedJuz > 0 || student.MemorizedPages > 0;
        if (!fromJuz.HasValue || !toJuz.HasValue || fromJuz < 1 || toJuz > 30 || fromJuz > toJuz)
            return false;
        if (fromJuz == 1 && toJuz == 30)
            return WirdPageCalculator.IsHafiz(student);
        var completedJuz = student.MemorizedJuz + (student.MemorizedPages / 20m);
        return toJuz.Value <= Math.Floor(completedJuz);
    }

    private CertificateModel ToModel(Certificate certificate, string? baseUrl)
    {
        CertificateModel? data;
        try
        {
            data = JsonSerializer.Deserialize<CertificateModel>(
                certificate.CertificateDataJson,
                JsonOptions
            );
        }
        catch (JsonException)
        {
            data = null;
        }
        data ??= new CertificateModel
        {
            StudentId = certificate.StudentId,
            Type = certificate.Type,
        };
        data.CertificateId = certificate.Id;
        data.CertificateNumber = certificate.CertificateNumber;
        data.InstituteId = certificate.InstituteId;
        data.InstituteName = certificate.Institute?.Name ?? data.InstituteName;
        data.InstituteLogoUrl = certificate.Institute?.Logo ?? data.InstituteLogoUrl;
        data.TemplateName = certificate.Template?.Name ?? data.TemplateName;
        data.VerificationToken = certificate.VerificationToken;
        data.VerificationUrl = BuildUrl(
            baseUrl,
            $"/Certificates/Verify/{certificate.VerificationToken}"
        );
        data.Status = certificate.Status;
        data.QrCodeDataUrl = _qrCodes.GenerateDataUri(data.VerificationUrl);
        return data;
    }

    private static string GenerateToken() =>
        Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string BuildUrl(string? baseUrl, string path) =>
        string.IsNullOrWhiteSpace(baseUrl) ? path : baseUrl.TrimEnd('/') + path;

    private static string FormatArabicDate(DateTime date) =>
        $"{FormatHijriDate(date)} الموافق {date:yyyy/MM/dd} م";

    private static string FormatHijriDate(DateTime date)
    {
        try
        {
            var calendar = new UmAlQuraCalendar();
            return $"{calendar.GetYear(date):0000}/{calendar.GetMonth(date):00}/{calendar.GetDayOfMonth(date):00} هـ";
        }
        catch (ArgumentOutOfRangeException)
        {
            return string.Empty;
        }
    }
}
