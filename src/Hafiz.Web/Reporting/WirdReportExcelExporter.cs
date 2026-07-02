using System;
using System.IO;
using ClosedXML.Excel;
using Hafiz.DTOs.Reports;
using Hafiz.Models;

namespace Hafiz.Web.Reporting
{
    /// <summary>
    /// يبني ملف Excel لتقرير الأوراد (ورقة ملخّص + ورقة تفاصيل). مشترك بين مناطق
    /// المشرف ومشرف النظام حتى لا يتكرّر منطق التصدير.
    /// </summary>
    public static class WirdReportExcelExporter
    {
        public const string ContentType =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public static byte[] Build(WirdReportViewModel vm)
        {
            using var workbook = new XLWorkbook();
            BuildSummarySheet(workbook, vm);
            BuildDetailsSheet(workbook, vm);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void BuildSummarySheet(XLWorkbook workbook, WirdReportViewModel vm)
        {
            var sheet = workbook.Worksheets.Add("الملخّص");
            sheet.RightToLeft = true;

            var stats = vm.Stats;
            var rows = new (string Label, object Value)[]
            {
                ("إجمالي الأوراد", stats.TotalAssignments),
                ("المكتملة", stats.CompletedCount),
                ("المعلّقة", stats.PendingCount),
                ("القادمة", stats.UpcomingCount),
                ("نسبة الإنجاز %", stats.CompletionRate),
                ("إجمالي الصفحات المكافئة", stats.TotalEquivalentPages),
                ("الصفحات المكافئة المكتملة", stats.CompletedEquivalentPages),
            };

            sheet.Cell(1, 1).Value = "مؤشّرات عامة";
            sheet.Range(1, 1, 1, 2).Merge().Style.Font.SetBold();
            for (int i = 0; i < rows.Length; i++)
            {
                sheet.Cell(i + 2, 1).Value = rows[i].Label;
                sheet.Cell(i + 2, 2).Value = XLCellValue.FromObject(rows[i].Value);
            }

            int typeHeaderRow = rows.Length + 3;
            sheet.Cell(typeHeaderRow, 1).Value = "النوع";
            sheet.Cell(typeHeaderRow, 2).Value = "الإجمالي";
            sheet.Cell(typeHeaderRow, 3).Value = "المكتمل";
            sheet.Cell(typeHeaderRow, 4).Value = "الصفحات";
            sheet.Range(typeHeaderRow, 1, typeHeaderRow, 4).Style.Font.SetBold();

            int r = typeHeaderRow + 1;
            foreach (var t in vm.Stats.ByType)
            {
                sheet.Cell(r, 1).Value = TypeLabel(t.Type);
                sheet.Cell(r, 2).Value = t.Total;
                sheet.Cell(r, 3).Value = t.Completed;
                sheet.Cell(r, 4).Value = t.Pages;
                r++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static void BuildDetailsSheet(XLWorkbook workbook, WirdReportViewModel vm)
        {
            var sheet = workbook.Worksheets.Add("التفاصيل");
            sheet.RightToLeft = true;

            string[] headers =
            {
                "الطالب",
                "الحلقة",
                "النوع",
                "الوصف",
                "الصفحات المكافئة",
                "التاريخ",
                "الحالة",
                "التقييم",
                "ملاحظة",
            };
            for (int c = 0; c < headers.Length; c++)
                sheet.Cell(1, c + 1).Value = headers[c];
            sheet.Range(1, 1, 1, headers.Length).Style.Font.SetBold();

            int row = 2;
            foreach (var d in vm.Details)
            {
                sheet.Cell(row, 1).Value = d.StudentName;
                sheet.Cell(row, 2).Value = d.ClassName;
                sheet.Cell(row, 3).Value = TypeLabel(d.Type);
                sheet.Cell(row, 4).Value = d.Description;
                sheet.Cell(row, 5).Value = d.EquivalentPages;
                sheet.Cell(row, 6).Value = d.AssignedDate.ToString("yyyy-MM-dd");
                sheet.Cell(row, 7).Value = d.IsCompleted ? "مكتمل" : "معلّق";
                sheet.Cell(row, 8).Value = StatusLabel(d.Status);
                sheet.Cell(row, 9).Value = d.Note ?? string.Empty;
                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        private static string TypeLabel(AssignmentType type) =>
            type switch
            {
                AssignmentType.Wird => "ورد",
                AssignmentType.Memorization => "حفظ",
                AssignmentType.Revision => "مراجعة",
                AssignmentType.Tajwid => "تجويد",
                AssignmentType.Tafsir => "تفسير",
                _ => "أخرى",
            };

        private static string StatusLabel(AssignmentStatus status) =>
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
}
