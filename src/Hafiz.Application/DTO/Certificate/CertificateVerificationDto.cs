using System;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

public class CertificateVerificationDto
{
    public bool IsFound { get; set; } = true;
    public bool IsValid => IsFound && Status == CertificateStatus.Active;
    public CertificateStatus Status { get; set; } = CertificateStatus.Active;
    public string StatusMessage { get; set; } = string.Empty;

    public Guid CertificateId { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string InstituteName { get; set; } = string.Empty;
    public string? InstituteLogoUrl { get; set; }

    public CertificateType Type { get; set; }
    public string TypeDisplayName => Type switch
    {
        CertificateType.Quran => "شهادة قرآن كريم",
        CertificateType.Matn => "شهادة متن علمي",
        CertificateType.Sanad => "إجازة وإسناد",
        CertificateType.General => "شهادة تقديرية",
        _ => "شهادة معتمدة"
    };

    public string CertificateTitle { get; set; } = string.Empty;
    public string AchievementDescription { get; set; } = string.Empty;

    public decimal? Score { get; set; }
    public string? Rating { get; set; }

    public DateTime IssuedAtUserTime { get; set; }
    public string IssuedAtDisplay => IssuedAtUserTime.ToString("yyyy/MM/dd");
    public string? HijriDateDisplay { get; set; }

    public string? TeacherName { get; set; }
    public string? DirectorName { get; set; }

    public string VerificationToken { get; set; } = string.Empty;
    public string VerificationUrl { get; set; } = string.Empty;
    public string? CertificateViewUrl { get; set; }

    public DateTime? RevokedAtUserTime { get; set; }
    public string? RevokeReason { get; set; }
}
