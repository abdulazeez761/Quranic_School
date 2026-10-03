using System.Collections.Generic;
using Hafiz.Domain.Enums;

namespace Hafiz.Application.DTO.Certificate.TemplateConfiguration;

public static class DefaultTemplateConfigurations
{
    public static TemplateConfiguration GetDefault(CertificateType type)
    {
        return type switch
        {
            CertificateType.Quran => CreateDefaultQuranConfig(),
            CertificateType.Matn => CreateDefaultMatnConfig(),
            CertificateType.Sanad => CreateDefaultSanadConfig(),
            CertificateType.General => CreateDefaultGeneralConfig(),
            _ => CreateDefaultGeneralConfig()
        };
    }

    public static TemplateConfiguration CreateDefaultQuranConfig()
    {
        return new TemplateConfiguration
        {
            Layout = new LayoutConfig
            {
                PageSize = "A4",
                Orientation = "landscape",
                Margins = new MarginsConfig { Top = 10, Right = 12, Bottom = 10, Left = 12 }
            },
            Theme = new ThemeConfig
            {
                Preset = "classic_gold",
                PrimaryColor = "#C59B27",
                SecondaryColor = "#064E3B",
                AccentColor = "#DFBA69",
                TextColor = "#1C1917",
                BackgroundColor = "#FDFBF7",
                BorderColor = "#C59B27",
                BorderStyle = "double",
                BorderWidth = 3,
                CornerDecorations = true,
                WatermarkEnabled = true,
                WatermarkIcon = "bx-book-open",
                WatermarkOpacity = 0.04
            },
            Typography = new TypographyConfig
            {
                TitleFont = "Reem Kufi",
                BodyFont = "Cairo",
                ArabicFont = "Amiri",
                TitleSize = "2.1rem",
                BodySize = "1.28rem",
                StudentNameSize = "2.6rem",
                LineHeight = 1.8
            },
            Sections = new List<SectionConfig>
            {
                new SectionConfig
                {
                    Type = "header",
                    Enabled = true,
                    Order = 1,
                    Config = new Dictionary<string, object>
                    {
                        { "showInstituteLogo", true },
                        { "showInstituteName", true },
                        { "showBasmalah", true },
                        { "showCertificateNumber", true },
                        { "showSubInstitute", true },
                        { "subInstituteText", "شؤون حلقات القرآن الكريم" }
                    }
                },
                new SectionConfig
                {
                    Type = "title",
                    Enabled = true,
                    Order = 2,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "شهادة إتمام وتفوق قرآني" },
                        { "showDecorators", true },
                        { "showUnderline", true }
                    }
                },
                new SectionConfig
                {
                    Type = "verse",
                    Enabled = true,
                    Order = 3,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "﴿ إِنَّ هَـٰذَا الْقُرْآنَ يَهْدِي لِلَّتِي هِيَ أَقْوَمُ وَيُبَشِّرُ الْمُؤْمِنِينَ الَّذِينَ يَعْمَلُونَ الصَّالِحَاتِ أَنَّ لَهُمْ أَجْرًا كَبِيرًا ﴾" }
                    }
                },
                new SectionConfig
                {
                    Type = "statement",
                    Enabled = true,
                    Order = 4,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "تـشـهـد إدارة المركز والـحـلـقـات بـأن {GenderedStudent}:" },
                        { "showStudentName", true },
                        { "showSubjectName", true },
                        { "showAuthor", false },
                        { "showScopeDetails", true }
                    }
                },
                new SectionConfig
                {
                    Type = "quranScope",
                    Enabled = true,
                    Order = 5,
                    Config = new Dictionary<string, object>
                    {
                        { "showJuzCount", true },
                        { "showFromJuz", true },
                        { "showToJuz", true },
                        { "showCompletionPercentage", true },
                        { "showDetailedJuzList", false },
                        { "showSurahRange", false },
                        { "showRiwayah", true },
                        { "riwayahText", "برواية حفص عن عاصم من طريق الشاطبية" }
                    }
                },
                new SectionConfig
                {
                    Type = "evaluation",
                    Enabled = true,
                    Order = 6,
                    Config = new Dictionary<string, object>
                    {
                        { "showScore", true },
                        { "showRating", true },
                        { "showExamResult", true }
                    }
                },
                new SectionConfig
                {
                    Type = "signatures",
                    Enabled = true,
                    Order = 7,
                    Config = new Dictionary<string, object>
                    {
                        {
                            "columns", new List<SignatureColumn>
                            {
                                new() { Title = "معلم الحلقة", NameField = "{TeacherName}", Subtitle = "التوقيع والاعتماد", Type = "signature" },
                                new() { Title = "الختم والتاريخ", NameField = "", Subtitle = "", Type = "seal", ShowSeal = true, ShowDate = true },
                                new() { Title = "إدارة المركز", NameField = "{DirectorName}", Subtitle = "الاعتماد الرسمي", Type = "signature" }
                            }
                        }
                    }
                },
                new SectionConfig
                {
                    Type = "qrVerification",
                    Enabled = true,
                    Order = 8,
                    Config = new Dictionary<string, object>
                    {
                        { "size", 80 },
                        { "showLabel", true },
                        { "labelText", "رمز التحقق الإلكتروني المعتمد" }
                    }
                }
            },
            Tokens = new TokenConfig
            {
                ClosingText = "سائلين المولى عز وجل أن يجعله من أهل القرآن الذين هم أهل الله وخاصته، وأن ينفع به الإسلام والمسلمين."
            }
        };
    }

    public static TemplateConfiguration CreateDefaultMatnConfig()
    {
        return new TemplateConfiguration
        {
            Layout = new LayoutConfig
            {
                PageSize = "A4",
                Orientation = "landscape",
                Margins = new MarginsConfig { Top = 10, Right = 12, Bottom = 10, Left = 12 }
            },
            Theme = new ThemeConfig
            {
                Preset = "royal_emerald",
                PrimaryColor = "#064E3B",
                SecondaryColor = "#C59B27",
                AccentColor = "#047857",
                TextColor = "#1C1917",
                BackgroundColor = "#FDFBF7",
                BorderColor = "#064E3B",
                BorderStyle = "double",
                BorderWidth = 3,
                CornerDecorations = true,
                WatermarkEnabled = true,
                WatermarkIcon = "bx-book-bookmark",
                WatermarkOpacity = 0.04
            },
            Typography = new TypographyConfig
            {
                TitleFont = "Reem Kufi",
                BodyFont = "Cairo",
                ArabicFont = "Amiri",
                TitleSize = "2.1rem",
                BodySize = "1.28rem",
                StudentNameSize = "2.6rem",
                LineHeight = 1.8
            },
            Sections = new List<SectionConfig>
            {
                new SectionConfig
                {
                    Type = "header",
                    Enabled = true,
                    Order = 1,
                    Config = new Dictionary<string, object>
                    {
                        { "showInstituteLogo", true },
                        { "showInstituteName", true },
                        { "showBasmalah", true },
                        { "showCertificateNumber", true },
                        { "showSubInstitute", true },
                        { "subInstituteText", "قسم المتون العلمية والتأصيل الشرعي" }
                    }
                },
                new SectionConfig
                {
                    Type = "title",
                    Enabled = true,
                    Order = 2,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "شهادة إتقان وضبط متن علمي" },
                        { "showDecorators", true },
                        { "showUnderline", true }
                    }
                },
                new SectionConfig
                {
                    Type = "verse",
                    Enabled = true,
                    Order = 3,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "﴿ يَرْفَعِ اللَّهُ الَّذِينَ آمَنُوا مِنكُمْ وَالَّذِينَ أُوتُوا الْعِلْمَ دَرَجَاتٍ ﴾" }
                    }
                },
                new SectionConfig
                {
                    Type = "statement",
                    Enabled = true,
                    Order = 4,
                    Config = new Dictionary<string, object>
                    {
                        { "text", "تـشـهـد إدارة المركز بأن {GenderedStudent}:" },
                        { "showStudentName", true },
                        { "showSubjectName", true },
                        { "showAuthor", true },
                        { "showScopeDetails", true }
                    }
                },
                new SectionConfig
                {
                    Type = "evaluation",
                    Enabled = true,
                    Order = 5,
                    Config = new Dictionary<string, object>
                    {
                        { "showScore", true },
                        { "showRating", true },
                        { "showExamResult", true }
                    }
                },
                new SectionConfig
                {
                    Type = "signatures",
                    Enabled = true,
                    Order = 6,
                    Config = new Dictionary<string, object>
                    {
                        {
                            "columns", new List<SignatureColumn>
                            {
                                new() { Title = "المجاز / المعلم", NameField = "{TeacherName}", Subtitle = "المشرف على التسميع", Type = "signature" },
                                new() { Title = "الختم والتاريخ", NameField = "", Subtitle = "", Type = "seal", ShowSeal = true, ShowDate = true },
                                new() { Title = "مدير المعهد", NameField = "{DirectorName}", Subtitle = "الاعتماد الرسمي", Type = "signature" }
                            }
                        }
                    }
                },
                new SectionConfig
                {
                    Type = "qrVerification",
                    Enabled = true,
                    Order = 7,
                    Config = new Dictionary<string, object>
                    {
                        { "size", 80 },
                        { "showLabel", true },
                        { "labelText", "رمز التحقق الإلكتروني المعتمد" }
                    }
                }
            },
            Tokens = new TokenConfig
            {
                ClosingText = "بارك الله في همته ونفع به ووفقه لمواصلة طلب العلم الشريف والعمل به."
            }
        };
    }

    public static TemplateConfiguration CreateDefaultSanadConfig()
    {
        var config = CreateDefaultQuranConfig();
        config.Theme.Preset = "navy_sapphire";
        config.Theme.PrimaryColor = "#1E3A8A";
        config.Theme.SecondaryColor = "#B45309";
        config.Theme.AccentColor = "#60A5FA";
        config.Theme.BorderColor = "#1E3A8A";
        config.Theme.FrameStyle = "ornate";
        config.Theme.CornerStyle = "girih";
        config.Theme.CornerSize = 64;

        var header = config.Sections.FirstOrDefault(s => s.Type == "header");
        if (header != null) header.Config["subInstituteText"] = "قسم الإسناد والإجازات القرآنية";

        var title = config.Sections.FirstOrDefault(s => s.Type == "title");
        if (title != null) title.Config["text"] = "إجازة بالسند المتصل في القرآن الكريم";

        var verse = config.Sections.FirstOrDefault(s => s.Type == "verse");
        if (verse != null) verse.Config["text"] = "﴿ ثُمَّ أَوْرَثْنَا الْكِتَابَ الَّذِينَ اصْطَفَيْنَا مِنْ عِبَادِنَا ﴾";

        var stmt = config.Sections.FirstOrDefault(s => s.Type == "statement");
        if (stmt != null)
        {
            stmt.Config["text"] = "تشهد إدارة المركز والمسند المجيز بأن {GenderedStudent}:";
            stmt.Config["showAuthor"] = true;
        }

        config.Tokens.ClosingText = "وأوصيه بتقوى الله تعالى في السر والعلن، وملازمة كتاب الله، وألا ينساني ومشايخي من صالح دعائه.";
        return config;
    }

    public static TemplateConfiguration CreateDefaultGeneralConfig()
    {
        var config = CreateDefaultQuranConfig();
        config.Theme.Preset = "classic_gold";
        config.Theme.PrimaryColor = "#854D0E";
        config.Theme.SecondaryColor = "#1E3A8A";
        config.Theme.AccentColor = "#CA8A04";
        config.Theme.BorderColor = "#854D0E";

        var header = config.Sections.FirstOrDefault(s => s.Type == "header");
        if (header != null) header.Config["subInstituteText"] = "إدارة البرامج والأنشطة الطلابية";

        var title = config.Sections.FirstOrDefault(s => s.Type == "title");
        if (title != null) title.Config["text"] = "شهادة شكر وتقدير";

        var verse = config.Sections.FirstOrDefault(s => s.Type == "verse");
        if (verse != null) verse.Config["text"] = "﴿ هَلْ جَزَاءُ الْإِحْسَانِ إِلَّا الْإِحْسَانُ ﴾";

        var stmt = config.Sections.FirstOrDefault(s => s.Type == "statement");
        if (stmt != null)
        {
            stmt.Config["text"] = "تتقدم إدارة المركز بوافر الشكر والتقدير إلى {GenderedStudent}:";
            stmt.Config["showScopeDetails"] = false;
        }

        var scope = config.Sections.FirstOrDefault(s => s.Type == "quranScope");
        if (scope != null) scope.Enabled = false;

        config.Tokens.ClosingText = "تقديراً لجهوده المتميزة وتفوقه المستمر، متمنين له دوام التوفيق والنجاح.";
        return config;
    }
}
