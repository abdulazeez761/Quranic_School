using System;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate;

/// <summary>One row of a certificate's audit trail, ready for the details modal.</summary>
public class CertificateAuditLogDto
{
    public Guid Id { get; set; }
    public CertificateAuditAction Action { get; set; }
    public string? Details { get; set; }

    /// <summary>Already converted from UTC into the viewer's time zone.</summary>
    public DateTime PerformedAtUserTime { get; set; }

    public string PerformedAtDisplay => PerformedAtUserTime.ToString("yyyy/MM/dd HH:mm");

    /// <summary>Display name of the acting user, or null when the action was automatic.</summary>
    public string? PerformedByName { get; set; }

    public string PerformedByDisplay => string.IsNullOrWhiteSpace(PerformedByName) ? "النظام" : PerformedByName!;

    public string ActionDisplayName => Action switch
    {
        CertificateAuditAction.Created => "إنشاء",
        CertificateAuditAction.Updated => "تعديل",
        CertificateAuditAction.Issued => "إصدار",
        CertificateAuditAction.Revoked => "إلغاء اعتماد",
        CertificateAuditAction.Regenerated => "إعادة إصدار",
        CertificateAuditAction.Viewed => "عرض",
        CertificateAuditAction.Verified => "تحقق",
        CertificateAuditAction.Deleted => "حذف الشهادة",
        _ => "إجراء"
    };

    public string ActionIcon => Action switch
    {
        CertificateAuditAction.Issued => "bx-certification",
        CertificateAuditAction.Created => "bx-plus-circle",
        CertificateAuditAction.Revoked => "bx-block",
        CertificateAuditAction.Regenerated => "bx-refresh",
        CertificateAuditAction.Verified => "bx-check-shield",
        CertificateAuditAction.Viewed => "bx-show",
        CertificateAuditAction.Deleted => "bx-trash",
        _ => "bx-edit"
    };

    /// <summary>Drives the colour of the timeline dot.</summary>
    public string ActionTone => Action switch
    {
        CertificateAuditAction.Issued => "success",
        CertificateAuditAction.Revoked => "danger",
        CertificateAuditAction.Deleted => "danger",
        CertificateAuditAction.Verified => "info",
        _ => "neutral"
    };
}
