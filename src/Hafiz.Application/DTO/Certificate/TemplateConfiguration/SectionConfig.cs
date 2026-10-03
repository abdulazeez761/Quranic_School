using System.Collections.Generic;
using System.Text.Json;

namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public class SectionConfig
{
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public int Order { get; set; } = 1;
    public Dictionary<string, object> Config { get; set; } = new();

    public T GetConfig<T>() where T : new()
    {
        if (Config == null || Config.Count == 0) return new T();
        var json = JsonSerializer.Serialize(Config);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new T();
    }
}

public class HeaderSectionConfig
{
    public bool ShowInstituteLogo { get; set; } = true;
    public bool ShowInstituteName { get; set; } = true;
    public bool ShowBasmalah { get; set; } = true;
    public string BasmalahText { get; set; } = "بِسْمِ اللَّـهِ الرَّحْمَـٰنِ الرَّحِيمِ";
    public bool ShowCertificateNumber { get; set; } = true;
    public bool ShowDate { get; set; } = false;
    public bool ShowSubInstitute { get; set; } = true;
    public string SubInstituteText { get; set; } = "شؤون حلقات القرآن الكريم";
}

public class TitleSectionConfig
{
    public string Text { get; set; } = "{CertificateTitle}";
    public string? Subtitle { get; set; }
    public bool ShowDecorators { get; set; } = true;
    public bool ShowUnderline { get; set; } = true;
}

public class VerseSectionConfig
{
    public string Text { get; set; } = "﴿ إِنَّ هَـٰذَا الْقُرْآنَ يَهْدِي لِلَّتِي هِيَ أَقْوَمُ ﴾";
    public string? Reference { get; set; }
}

public class StatementSectionConfig
{
    public string Text { get; set; } = "تشهد إدارة المركز بأن {GenderedStudent} قد {GenderedCompleted} بحمد الله وتوفيقه حفظ وإتقان المقرر من كتاب الله العزيز.";
    public string? StudentNamePrefix { get; set; }
    public bool ShowStudentName { get; set; } = true;
    public bool ShowSubjectName { get; set; } = true;
    public bool ShowAuthor { get; set; } = true;
    public bool ShowScopeDetails { get; set; } = true;
}

public class QuranScopeSectionConfig
{
    public bool ShowJuzCount { get; set; } = true;
    public bool ShowFromJuz { get; set; } = true;
    public bool ShowToJuz { get; set; } = true;
    public bool ShowCompletionPercentage { get; set; } = true;
    public bool ShowDetailedJuzList { get; set; } = false;
    public bool ShowSurahRange { get; set; } = false;
    public bool ShowRiwayah { get; set; } = true;
    public string RiwayahText { get; set; } = "برواية حفص عن عاصم من طريق الشاطبية";
}

public class EvaluationSectionConfig
{
    public bool ShowScore { get; set; } = true;
    public bool ShowRating { get; set; } = true;
    public bool ShowExamResult { get; set; } = true;
}

public class SignatureColumn
{
    public string Title { get; set; } = string.Empty;
    public string NameField { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string Type { get; set; } = "signature"; // "signature" or "seal"
    public bool ShowSeal { get; set; } = false;
    public bool ShowDate { get; set; } = false;
}

public class SignaturesSectionConfig
{
    public List<SignatureColumn> Columns { get; set; } = new();
}

public class QrVerificationSectionConfig
{
    public int Size { get; set; } = 80;
    public bool ShowLabel { get; set; } = true;
    public string LabelText { get; set; } = "رمز التحقق الإلكتروني";
}
