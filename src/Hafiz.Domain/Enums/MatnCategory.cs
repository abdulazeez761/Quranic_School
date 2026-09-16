namespace Hafiz.Domain.Enums;

public enum MatnCategory
{
    General = 0,             // عام / متفرقات
    Tajweed = 1,             // تجويد وقراءات
    Aqeedah = 2,             // عقيدة وتوحيد
    Hadith = 3,              // حديث ومصطلح
    Fiqh = 4,                // فقه وأصوله
    NahwAndLanguage = 5,     // نحو ولغة
    Seerah = 6,              // سيرة وتاريخ
    Adab = 7                 // آداب وتزكية
}

public static class MatnCategoryExtensions
{
    public static string ToArabic(this MatnCategory category) => category switch
    {
        MatnCategory.Tajweed => "تجويد وقراءات",
        MatnCategory.Aqeedah => "عقيدة وتوحيد",
        MatnCategory.Hadith => "حديث ومصطلح",
        MatnCategory.Fiqh => "فقه وأصوله",
        MatnCategory.NahwAndLanguage => "نحو ولغة",
        MatnCategory.Seerah => "سيرة وتاريخ",
        MatnCategory.Adab => "آداب وتزكية",
        _ => "عام / أخرى"
    };
}
