using System;
using System.Collections.Generic;
using Hafiz.Application.DTO.Certificate;

namespace Hafiz.Application.Services;

/// <summary>Resolves user-editable template tokens without exposing Razor or HTML execution.</summary>
public static class CertificateTokenResolver
{
    public static string Resolve(string? value, CertificateModel certificate)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["{StudentName}"] = certificate.StudentName,
            ["{GenderedStudent}"] = certificate.GenderedStudent,
            ["{GenderedCompleted}"] = certificate.GenderedCompleted,
            ["{InstituteName}"] = certificate.InstituteName,
            ["{TeacherName}"] = certificate.TeacherName,
            ["{DirectorName}"] = certificate.DirectorName,
            ["{ClassName}"] = certificate.ClassName ?? string.Empty,
            ["{CertificateNumber}"] = certificate.CertificateNumber,
            ["{CertificateTitle}"] = certificate.CertificateTitle,
            ["{IssueDate}"] = certificate.IssueDateFormatted,
            ["{HijriDate}"] = certificate.HijriDateFormatted ?? string.Empty,
            ["{GregorianDate}"] = certificate.GregorianDateFormatted ?? string.Empty,
            ["{SubjectName}"] = certificate.SubjectName,
            ["{AuthorName}"] = certificate.SubtitleOrAuthor ?? string.Empty,
            ["{Score}"] = certificate.Score?.ToString("0.##") ?? string.Empty,
            ["{Rating}"] = certificate.Rating ?? string.Empty
        };
        foreach (var item in certificate.ExtraData) tokens["{" + item.Key + "}"] = item.Value;
        foreach (var item in tokens) value = value.Replace(item.Key, item.Value, StringComparison.OrdinalIgnoreCase);
        return value;
    }
}
