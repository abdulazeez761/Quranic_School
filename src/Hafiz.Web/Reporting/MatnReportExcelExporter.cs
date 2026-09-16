using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Hafiz.Domain.Enums;
using Hafiz.DTOs.Matn;
using Hafiz.Models;

namespace Hafiz.Web.Reporting;

/// <summary>
/// يبني ملف Excel لتقارير إنجازات وتسميع المتون العلمية
/// </summary>
public static class MatnReportExcelExporter
{
    public const string ContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public static byte[] Build(IEnumerable<MatnAssignmentDto> assignments, string title = "تقرير تسميع المتون العلمية")
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("إنجازات المتون");
        sheet.RightToLeft = true;

        string[] headers =
        {
            "الطالب",
            "الحلقة",
            "نوع الإنجاز",
            "الباب / الفصل",
            "من",
            "إلى",
            "الكمية",
            "وحدة القياس",
            "التاريخ",
            "الحالة",
            "التقييم",
            "الملاحظات",
        };

        for (int c = 0; c < headers.Length; c++)
            sheet.Cell(1, c + 1).Value = headers[c];
        sheet.Range(1, 1, 1, headers.Length).Style.Font.SetBold().Fill.SetBackgroundColor(XLColor.FromHtml("#0284c7")).Font.SetFontColor(XLColor.White);

        int row = 2;
        foreach (var a in assignments)
        {
            sheet.Cell(row, 1).Value = a.StudentName;
            sheet.Cell(row, 2).Value = a.ClassName;
            sheet.Cell(row, 3).Value = a.PerformanceTypeName;
            sheet.Cell(row, 4).Value = a.ChapterName ?? string.Empty;
            sheet.Cell(row, 5).Value = a.FromNumber?.ToString() ?? "—";
            sheet.Cell(row, 6).Value = a.ToNumber?.ToString() ?? "—";
            sheet.Cell(row, 7).Value = a.Amount?.ToString() ?? "—";
            sheet.Cell(row, 8).Value = a.UnitName;
            sheet.Cell(row, 9).Value = a.AssignedDate.ToString("yyyy-MM-dd");
            sheet.Cell(row, 10).Value = a.IsCompleted ? "مكتمل" : "معلّق";
            sheet.Cell(row, 11).Value = StatusLabel(a.Status);
            sheet.Cell(row, 12).Value = a.Note ?? string.Empty;
            row++;
        }

        sheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string StatusLabel(AssignmentStatus? status) =>
        status switch
        {
            AssignmentStatus.excellent => "ممتاز",
            AssignmentStatus.veryGood => "جيد جداً",
            AssignmentStatus.good => "جيد",
            AssignmentStatus.fair => "مقبول",
            AssignmentStatus.poor => "ضعيف",
            _ => "—",
        };
}
