using System;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateListItemDto
{
    public Guid Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid InstituteId { get; set; }
    public string InstituteName { get; set; } = string.Empty;
    public CertificateType Type { get; set; }
    public string TypeDisplayName => Type switch
    {
        CertificateType.Quran => "قرآن كريم",
        CertificateType.Matn => "متن علمي",
        CertificateType.Sanad => "إجازة بالسند",
        CertificateType.General => "شهادة عامة",
        CertificateType.Custom => "قالب مخصص",
        _ => "شهادة"
    };
    public string SubjectTitle { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public DateTime IssuedAtUserTime { get; set; }
    public string IssuedAtDisplay => IssuedAtUserTime.ToString("yyyy/MM/dd");
    public CertificateStatus Status { get; set; }
    public string StatusDisplayName => Status switch
    {
        CertificateStatus.Active => "معتمدة وسارية",
        CertificateStatus.Revoked => "ملغاة",
        CertificateStatus.Expired => "منتهية",
        _ => "غير معروف"
    };
    public string VerificationToken { get; set; } = string.Empty;
    public DateTime? RevokedAtUserTime { get; set; }
    public string? RevokeReason { get; set; }
}
