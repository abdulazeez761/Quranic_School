using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Hafiz.Application.DTO.Certificate;
using Hafiz.Domain.Enums;

namespace Hafiz.Web.Reporting;

/// <summary>
/// يبني ملف Excel لسجل الشهادات الصادرة، مطابقاً للفلاتر المعروضة على الشاشة.
/// </summary>
public static class CertificateReportExcelExporter
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Build(IEnumerable<CertificateListItemDto> certificates, string? filterSummary = null)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("سجل الشهادات");
        sheet.RightToLeft = true;

        string[] headers =
        {
            "رقم الشهادة",
            "الطالب",
            "النوع",
            "الموضوع",
            "القالب",
            "تاريخ الإصدار",
            "الحالة",
            "تاريخ الإلغاء",
            "سبب الإلغاء",
            "رابط التحقق",
        };

        for (int c = 0; c < headers.Length; c++)
            sheet.Cell(1, c + 1).Value = headers[c];
        sheet.Range(1, 1, 1, headers.Length).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#064E3B")).Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var item in certificates)
        {
            sheet.Cell(row, 1).Value = item.CertificateNumber;
            sheet.Cell(row, 2).Value = item.StudentName;
            sheet.Cell(row, 3).Value = item.TypeDisplayName;
            sheet.Cell(row, 4).Value = item.SubjectTitle;
            sheet.Cell(row, 5).Value = item.TemplateName;
            sheet.Cell(row, 6).Value = item.IssuedAtUserTime.ToString("yyyy/MM/dd");
            sheet.Cell(row, 7).Value = item.StatusDisplayName;
            sheet.Cell(row, 8).Value = item.RevokedAtUserTime?.ToString("yyyy/MM/dd") ?? string.Empty;
            sheet.Cell(row, 9).Value = item.RevokeReason ?? string.Empty;
            sheet.Cell(row, 10).Value = string.IsNullOrWhiteSpace(item.VerificationToken)
                ? string.Empty
                : $"/Certificates/Verify/{item.VerificationToken}";
            row++;
        }

        if (row > 2)
        {
            sheet.Range(2, 1, row - 1, headers.Length).Style.Border.SetBottomBorder(XLBorderStyleValues.Hair);
            // Colour the status column so a long register can be scanned at a glance.
            for (int r = 2; r < row; r++)
            {
                var cell = sheet.Cell(r, 7);
                var fill = cell.GetString() switch
                {
                    "معتمدة وسارية" => "#D1FAE5",
                    "ملغاة" => "#FEE2E2",
                    _ => "#FEF3C7",
                };
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml(fill));
            }
        }

        sheet.Columns().AdjustToContents();
        sheet.SheetView.FreezeRows(1);

        if (!string.IsNullOrWhiteSpace(filterSummary))
        {
            // Written into a cell rather than a header so the table stays a clean data range.
            var note = sheet.Cell(row + 1, 1);
            note.Value = filterSummary;
            note.Style.Font.SetItalic().Font.SetFontColor(XLColor.Gray);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
