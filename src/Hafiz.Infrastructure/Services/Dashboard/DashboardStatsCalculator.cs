using Hafiz.Models;
using Hafiz.Models.enums;

namespace Hafiz.Infrastructure.Services.Dashboard
{
    /// <summary>
    /// يُحوِّل مُعطيات ورد واحد إلى مساهمته في الصفحات/الأجزاء/الآيات.
    /// الأولوية للقيمة الصريحة (Amount + AmountUnit) — وهي المُدخل الأساسي عند إسناد الورد —
    /// ثم يرجع للنطاق (من/إلى) كحل احتياطي للسجلات القديمة التي لا تحمل Amount.
    /// النتائج هنا يجب أن تطابق <c>WirdPageCalculator.ToPagesExpression</c> حتى تبقى إحصائيات
    /// الداشبورد متوافقة مع إحصائيات التقارير في باقي النظام.
    /// </summary>
    internal static class DashboardStatsCalculator
    {
        internal static void Accumulate(
            WirdUnitsProjection w,
            ref double pages,
            ref double juz,
            ref int ayahs
        )
        {
            if (w.Amount.HasValue && w.AmountUnit.HasValue)
            {
                switch (w.AmountUnit.Value)
                {
                    case WirdUnit.Pages:
                        pages += (double)w.Amount.Value;
                        return;
                    case WirdUnit.Juz:
                        juz += (double)w.Amount.Value;
                        return;
                    case WirdUnit.Ayahs:
                        // ما يعادل الآيات بالصفحات يُضاف إلى صفحات الحفظ/المراجعة
                        // بنفس طريقة WirdPageCalculator المستخدم في التقارير.
                        pages += (double)(w.EquivalentPages ?? 0m);
                        ayahs += (int)
                            System.Math.Round(w.Amount.Value, System.MidpointRounding.AwayFromZero);
                        return;
                }
            }

            // احتياطي: اشتقاق الكمية من النطاق (شامل للطرفين)
            if (w.FromPage.HasValue && w.ToPage.HasValue)
            {
                pages += System.Math.Max(0, w.ToPage.Value - w.FromPage.Value + 1);
                return;
            }

            if (w.FromJuz.HasValue && w.ToJuz.HasValue)
            {
                juz += System.Math.Max(0, w.ToJuz.Value - w.FromJuz.Value + 1);
                return;
            }

            if (
                !string.IsNullOrWhiteSpace(w.FromAyah)
                && w.ToAyah.HasValue
                && int.TryParse(w.FromAyah, out int fromAyahNum)
            )
            {
                ayahs += System.Math.Max(0, w.ToAyah.Value - fromAyahNum + 1);
            }
        }
    }

    /// <summary>إسقاط مبسَّط للحقول اللازمة لحساب وحدات الورد.</summary>
    internal sealed class WirdUnitsProjection
    {
        public AssignmentType Type { get; init; }
        public decimal? Amount { get; init; }
        public WirdUnit? AmountUnit { get; init; }
        public decimal? EquivalentPages { get; init; }
        public int? FromPage { get; init; }
        public int? ToPage { get; init; }
        public int? FromJuz { get; init; }
        public int? ToJuz { get; init; }
        public string? FromAyah { get; init; }
        public int? ToAyah { get; init; }
    }
}
