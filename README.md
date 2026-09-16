<div align="center">

# Hifz — منصة إدارة حلقات التحفيظ

**نظام متكامل لإدارة دور ومراكز تحفيظ القرآن الكريم**

[![.NET](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-9.0-blue?logo=nuget)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Latest-red?logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-orange)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

</div>

---

## نبذة عن المشروع

**منصة حافظ (Hifz)** هي نظام ويب سحابي متكامل ومفتوح المصدر لإدارة دور ومراكز ومعاهد تحفيظ القرآن الكريم.
صُمّمت المنصة لتكون بنية متعددة المراكز (**Multi-Tenant / Multi-Institute**)، تربط بين:
- **مشرف النظام العام (SuperAdmin)**
- **إدارة المركز (Admin)**
- **المعلّمين (Teachers)**
- **الطلاب (Students)**
- **أولياء الأمور (Parents)**

تتيح المنصة متابعة الحفظ والمراجعة اليومية، وإجراء الفصول الافتراضية المباشرة بالفيديو، وتوليد التقارير والإحصائيات وتصديرها لإكسل، وتسجيل الحضور وساعات العمل، مع نظام أمان متقدم وأرشفة كاملة للبيانات.

---

## أبرز المميزات

| الميزة | التفاصيل |
| ------ | -------- |
| **تعدد المراكز والمعاهد (Multi-Tenancy)** | دعم إدارة مراكز ومعاهد متعددة بحسابات مستقلة وعزل كامل لبيانات كل مركز |
| **إدارة الأدوار والصلاحيات** | 5 أدوار مخصصة: مشرف عام (SuperAdmin)، مدير مركز (Admin)، معلّم، طالب، وليّ أمر |
| **الفصول الافتراضية المباشرة (Live Meetings)** | فصول قرآنية مرئية تفاعلية مدمجة عبر **Jitsi Meet API** مع تحكم كامل للمعلم |
| **الخطط الروتينية والإسناد الجماعي** | خطط مخصصة لكل طالب (حفظ جديد، مراجعة قريبة، مراجعة بعيدة، تجويد) مع إسناد جماعي بضغطة زر |
| **محرك الأوراد والقرآن** | تتبع الحفظ بالصفحات والأجزاء والآيات (114 سورة)، حساب المكافئ، تقييم التسميع، ومعيار لقب "حافظ" |
| **التقارير وتصدير Excel** | تقارير يومية مفصلة للحلقات، تقارير أوراد متقدمة وفلاتر شاملة، وتصدير فوري إلى ملفات Excel (`.xlsx`) |
| **لوحة إحصائيات حية (Dashboard)** | تحليل دقيق لكميات الإنجاز، توزيع الجنسين، مقارنة الحضور المتوقع بالفعلي لحظياً، وشريط أنشطة حي |
| **تسجيل الحضور وساعات العمل** | تسجيل حضور وغياب الطلاب والمعلمين بحالات متعددة (حاضر، متأخر، غائب، معذور) وتوثيق ساعات العمل |
| **الحذف الناعم والأرشفة (Soft Delete)** | أرشفة آمنة للشعب والطلاب والمعلمين والمراكز مع إمكانية الاستعادة الفورية (Restore) |
| **أمان متقدم ومكافحة التخمين** | حماية ضد هجمات Brute-force عبر **Rate Limiting**، رؤوس أمان مشددة، ومصادقة جلسات لحظية |
| **تطبيق ويب تقدمي (PWA)** | دعم التثبيت على الهواتف والحواسب، Service Worker، وصفحة عمل دون اتصال (Offline Fallback) |
| **ملاحظات أولياء الأمور** | تواصل ثنائي الاتجاه بين المعلّم وولي الأمر مع توثيق حالة القراءة وتاريخها (`IsRead`, `ReadAt`) |
| **إدارة المناطق الزمنية** | كشف وضبط التوقيت المحلي لكل مستخدم تلقائياً عبر الكوكيز لعرض المواعيد بدقة |
| **دعم اللغات (Localization)** | دعم كامل للغتين العربية والإنجليزية مع تبديل الاتجاه (RTL / LTR) |
| **النسخ الاحتياطي السحابي** | نسخ احتياطي لقاعدة البيانات يدوي ومجدول يومياً ورفعه تلقائياً إلى Google Drive |

---

### تفاصيل الميزات الرئيسية

#### 1. إدارة المراكز وتعدد المعاهد (Multi-Tenancy)
* يدعم النظام تشغيل عدة معاهد ومراكز تحفيظ على نفس النسخة.
* يتم عزل بيانات الطلاب والمعلمين والحلقات والتقارير بحسب المركز التابع له المستخدم (`InstituteId`).
* يستطيع المشرف العام (SuperAdmin) إنشاء مراكز جديدة وتعيين حسابات الإدارة لها في خطوة واحدة، أو إيقاف المركز مؤقتاً؛ مع طرد لحظي للمستخدمين التابعين للمركز المعطّل من جلساتهم.

#### 2. الفصول والاجتماعات الافتراضية عبر الفيديو (Virtual Classrooms)
* دمج تقنية **Jitsi Meet** مباشرة داخل المنصة دون الحاجة لتطبيقات خارجية.
* ينشئ النظام غرفة مخصصة وآمنة لكل حلقة دراسية (`HafizClass_{classId}`).
* يمتلك المعلّم أدوات تحكم تفاعلية:
  * بدء الجلسة وإغلاقها لجميع الطلاب.
  * كتم صوت الجميع (Mute All).
  * مشاركة الشاشة (Screen Sharing).
  * المحادثة النصية (Chat) وعرض الشبكة (Tile View).
  * استقبال تنبيهات رفع الأيدي للطلاب (Raise Hand).
* يستطيع الطلاب وأولياء الأمور الانضمام للجلسات المباشرة النشطة من صفحاتهم بضغطة زر.

#### 3. الخطط الروتينية الفردية والإسناد الجماعي للأوراد
* **خطة روتينية مخصصة (`StudentRoutinePlan`):** تحديد معدلات الإنجاز اليومية لكل طالب في 4 مسارات:
  1. الحفظ الجديد (Memorization).
  2. المراجعة الصغرى/القريبة (Recent Revision).
  3. المراجعة الكبرى/البعيدة (Old Revision).
  4. التلاوة وأحكام التجويد (Recitation & Tajwid).
* **الإسناد الجماعي (`AssignWirdsBatch`):** إمكانية إسناد وحفظ مسارات الورد المتعددة للطالب دفعة واحدة.
* **درج السياق السريع:** لوحة منبثقة تفاعلية للمعلّم تسترجع سياق أحدث أوراد الطالب وخطة اليوم لسرعة التقييم.

#### 4. محرك الأوراد والقرآن الكريم
* قاعدة بيانات مدمجة لجميع سور القرآن الكريم الـ 114 بالأسماء العربية والإنجليزية.
* دعم مختلف وحدات القياس (صفحات، أجزاء، آيات)، مع حاسبة ذكية تحوّل الآيات والأجزاء إلى ما يكافئها بالصفحات (`WirdPageCalculator`).
* تقييم جودة الحفظ والتسميع (ضعيف، مقبول، جيد، جيد جداً، ممتاز).
* تحديد تلقائي للوصول إلى لقب **"حافظ"** (`IsHafiz`) عند إتمام 600 صفحة (30 جزءاً).
* دعم تصنيف الأوراد المستقبلية المجدولة (`IsUpcoming`).

#### 5. منظومة التقارير وتصدير Excel
* **التقارير اليومية للحلقات:** عرض شامل لحضور وغياب الطلاب وتفاصيل الأوراد المنجزة مرتبة حسب كل حلقة ليوم محدد.
* **تقارير الأوراد المتقدمة:** تصفية متقدمة حسب المركز، الشعبة، الطالب، نوع الورد، التاريخ، والحالة، مع إحصائيات تجميعية وترتيب للطلاب الأكثر إنجازاً.
* **تصدير Excel:** تصدير فوري لكامل سجلات التقارير المطابقة للتصفية إلى ملفات إكسل منسقة (`.xlsx`).

#### 6. الحذف الناعم والأرشفة (Soft Delete & Recovery)
* يطبّق النظام واجهة `ISoftDeletable` عبر الـ `Global Query Filters` في EF Core.
* نقل الكيانات المحذوفة إلى قسم "الأرشيف" مع الاحتفاظ بكامل السجلات التاريخية.
* إمكانية استعادة وتفعيل (Restore) الشعب والطلاب والمعلمين والمراكز في أي وقت.

#### 7. الأمان ومكافحة هجمات التخمين (Security & Rate Limiting)
* حماية خاصة لصفحات المصادقة عبر سياسة `FixedWindowRateLimiter` (بحد أقصى 5 محاولات في الدقيقة لكل IP لمنع الـ Brute Force).
* محدد طلبات عام `TokenBucketRateLimiter` مع استثناء الملفات الثابتة.
* ترويسات حماية متقدمة: `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `X-XSS-Protection`, و `Referrer-Policy`.
* تحقق تلقائي من صلاحية الجلسات وطرد المستخدمين المحذوفين أو المعطلين فوراً.

#### 8. تطبيق الويب التقدمي (PWA) وإدارة التوقيت
* ملف بيان `manifest.json` وتوافق كامل مع شاشات الهواتف والأجهزة اللوحية.
* Service Worker مخصص لإدارة التخزين المؤقت وتحسين سرعة التصفح.
* صفحة مستقلة للعمل دون اتصال بالإنترنت (`offline.html`).
* اكتشاف المنطقة الزمنية للمستخدم وتخزينها لضبط أوقات الحلقات وجداول الحضور بدقة محلية.

---

## التقنيات المستخدمة

| المجال | التقنية |
| ------ | ------- |
| **Backend Framework** | ASP.NET Core 8.0 MVC |
| **Data Access & ORM** | Entity Framework Core 9.0 |
| **Database Engine** | Microsoft SQL Server |
| **Live Virtual Classes** | Jitsi Meet External API (WebRTC) |
| **Excel Reporting** | ClosedXML / Document Format Exporters |
| **PWA & Offline** | Service Workers + Web Manifest + Cache API |
| **Security & Rate Limiting** | ASP.NET Core Token Bucket & Fixed Window Limiters |
| **Frontend** | Razor Views + Bootstrap 5 + Boxicons / FontAwesome + Select2 |
| **Architecture Pattern** | Clean Architecture (Domain, Application, Infrastructure, Web) |

---

## المعمارية — Clean Architecture

يتّبع المشروع مبادئ **Clean Architecture** لضمان الفصل بين الطبقات وقابلية الاختبار والصيانة:

```
┌──────────────────────────────────────────────┐
│              Hafiz.Web (Presentation)        │  ← واجهة المستخدم والمتحكمات والتقارير
│         Controllers · Views · Areas          │
└───────────────┬──────────────────────────────┘
                │ يعتمد على
┌───────────────▼──────────────────────────────┐
│           Hafiz.Application                  │  ← منطق التطبيق والخدمات و DTOs
│    Interfaces · Services · DTOs · Helpers    │
└───────────────┬──────────────────────────────┘
                │ يعتمد على
┌───────────────▼──────────────────────────────┐
│            Hafiz.Domain                      │  ← جوهر النظام والكيانات والتعدادات
│            Entities · Enums                  │
└──────────────────────────────────────────────┘
                ▲
                │ يطبّق
┌───────────────┴──────────────────────────────┐
│          Hafiz.Infrastructure                │  ← البنية التحتية وقاعدة البيانات والخدمات
│   Repositories · Data · Migrations · Backup  │
└──────────────────────────────────────────────┘
```

---

## هيكل المشروع

```
Hifz.sln
├── src/
│   ├── Hafiz.Domain/                  ← الطبقة الجوهرية (Core)
│   │   ├── Entities/                  # Institute, User, Student, Teacher, Parent, Class
│   │   │                              # StudentRoutinePlan, WirdAssignment, ParentNote
│   │   │                              # StudentAttendance, TeacherAttendance, Video
│   │   └── Enums/                     # UserRole, Surah (114), AssignmentType, AssignmentStatus
│   │                                  # WirdUnit, TajwidLevel, AttendanceStatus, ClassDays
│   │
│   ├── Hafiz.Application/             ← طبقة التطبيق (Application)
│   │   ├── Interfaces/                # واجهات الخدمات والمستودعات
│   │   ├── Services/                  # منطق الأعمال (Auth, Institute, Wird, StudentPlan, ...)
│   │   ├── DTO/                       # كائنات نقل البيانات مصنفة حسب الميزات
│   │   ├── Common/Helper/             # حاسبة الأوراد (WirdPageCalculator), Formatting, Paging
│   │   └── Resources/                 # ملفات الترجمة (.resx)
│   │
│   ├── Hafiz.Infrastructure/          ← طبقة البنية التحتية (Infrastructure)
│   │   ├── Data/                      # ApplicationDbContext (Global Filters & Soft Delete)
│   │   ├── Repositories/              # تطبيق واجهات المستودعات
│   │   ├── Services/                  # خدمات خارجية (MeetingService, DashboardService, Backup)
│   │   └── Migrations/                # ترحيلات EF Core
│   │
│   └── Hafiz.Web/                     ← طبقة العرض (Presentation)
│       ├── Areas/
│       │   ├── SuperAdmin/            # لوحة مشرف النظام العام (المراكز، التقارير، النسخ الاحتياطي)
│       │   ├── Admin/                 # لوحة إدارة المركز (الحلقات، الطلاب، المعلمون، التقارير)
│       │   ├── Teacher/               # بوابة المعلّم (الطلاب، الأوراد، الفصول المباشرة، الحضور)
│       │   ├── Student/               # بوابة الطالب (لوحة الإنجاز، الأوراد، الحضور، الفصول المباشرة)
│       │   └── Parent/                # بوابة ولي الأمر (متابعة الأبناء، الملاحظات، الفصول المباشرة)
│       ├── Controllers/               # متحكمات عامة (Auth, Home)
│       ├── Reporting/                 # مصدّر تقارير الإكسل (WirdReportExcelExporter)
│       ├── Helpers/                   # محول المناطق الزمنية (TimeZoneHelper)
│       ├── BackgroundServices/        # مهام النسخ الاحتياطي المجدولة (DailyBackupService)
│       └── wwwroot/                   # ملفات ثابتة، Service Worker (sw.js)، offline.html، الأيقونات
│
└── tools/
    └── DriveAuth/                     # أداة ترخيص Google Drive OAuth لمرة واحدة
```

---

## التشغيل محلياً

### المتطلبات

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) أو أحدث
- [SQL Server](https://www.microsoft.com/en-us/sql-server) (محلي أو Express)
- Git

### الخطوات

```bash
# 1. استنساخ المشروع
git clone https://github.com/abdulazeez761/Hifz.git
cd Hifz

# 2. تحديث Connection String في appsettings.json
# عدّل ConnectionStrings:DefaultConnection حسب بيئتك

# 3. تشغيل المشروع (التهجير وقاعدة البيانات تُطبّق تلقائياً عند الإقلاع)
cd src/Hafiz.Web
dotnet run
```

بعد التشغيل، افتح المتصفح على: `https://localhost:5001` (أو الرابط المعروض في الطرفية).

> **ملاحظة:** يطبّق النظام ترحيلات قاعدة البيانات تلقائياً عند أول تشغيل عبر `context.Database.Migrate()` دون الحاجة لتنفيذ أمر التهجير يدوياً.

### حساب تسجيل الدخول الافتراضي (SuperAdmin Seed)

يقوم النظام تلقائياً عند التشغيل الأول بإنشاء حساب المشرف العام المبدئي التالي لتسجيل الدخول:

| الحقل | القيمة الافتراضية |
| ----- | ----------------- |
| **اسم المستخدم** | `superadmin` |
| **كلمة المرور** | `SuperAdmin123!` |
| **البريد الإلكتروني** | `superadmin@hafiz.com` |
| **الدور** | `SuperAdmin` (مشرف عام النظام) |

> ⚠️ **تنبيه أمني:** يُنصح بشدة بتغيير كلمة المرور فور تسجيل الدخول في بيئة الإنتاج.

---

## النسخ الاحتياطي لقاعدة البيانات (Google Drive)

تنشئ الميزة نسخة احتياطية كاملة (`.bak`) لقاعدة البيانات عبر أمر `BACKUP DATABASE`، ثم ترفعها تلقائيًا إلى مجلد على **Google Drive** وتحذف النسخة المحلية. متاحة كـ:

- **زر يدوي:** لوحة SuperAdmin → «النسخ الاحتياطي».
- **مهمة مجدولة يوميًا:** عبر `DailyBackupService` (الوقت الافتراضي 02:00).

### كيف تعمل آليًا

```
زر/مهمة مجدولة → BACKUP DATABASE TO DISK (.bak) → رفع إلى Google Drive → حذف النسخة المحلية
```

| المكوّن | الموقع |
| ------- | ------ |
| واجهة الخدمة | `Hafiz.Application/Interfaces/Services/IBackupService.cs` |
| تنفيذ النسخ + الرفع | `Hafiz.Infrastructure/Services/BackupService.cs` |
| رفع Google Drive | `Hafiz.Infrastructure/Services/GoogleDriveUploader.cs` |
| المهمة المجدولة | `Hafiz.Web/BackgroundServices/DailyBackupService.cs` |
| زر الواجهة | `Hafiz.Web/Areas/SuperAdmin/.../Backup` |
| أداة موافقة OAuth لمرة واحدة | `tools/DriveAuth/` |

### 1. إعداد Google Cloud (مرة واحدة)

1. أنشئ مشروعًا في [Google Cloud Console](https://console.cloud.google.com) وفعّل **Google Drive API**.
2. **OAuth consent screen** → النوع **External** → أضف حسابك في **Test users**.
3. **Credentials → Create Credentials → OAuth client ID** → النوع **Desktop app** → نزّل ملف JSON.
4. احفظه باسم `credentials.json`.

### 2. إعدادات التطبيق

| المفتاح | الوصف |
| ------- | ----- |
| `Backup:LocalFolder` | مجلد إنشاء ملف `.bak` المؤقت (لا بد أن يكتب فيه مستخدم خدمة SQL Server) |
| `Backup:DailyTime` | وقت النسخ المجدول بصيغة `HH:mm` (افتراضي `02:00`) |
| `GoogleDrive:CredentialsFilePath` | مسار `credentials.json` |
| `GoogleDrive:TokenFolder` | مجلد حفظ الـ refresh token (يحتاج صلاحية كتابة) |
| `GoogleDrive:TargetFolderId` | معرّف مجلد الوجهة في Drive (من رابط المجلد) |

> ⚠️ ملفّا `credentials.json` و الـ token **أسرار** — لا تضِفهما إلى git. ضعهما خارج المستودع.

### 3. الموافقة لمرة واحدة (OAuth)

الموافقة التفاعلية تحتاج متصفحًا ولا تصلح داخل طلب ويب أو على سيرفر بلا واجهة. شغّل أداة الموافقة **على جهاز فيه متصفح** مرة واحدة لتوليد الـ token:

```bash
dotnet run --project tools/DriveAuth -- "<مسار credentials.json>" "<مجلد الـ token>"
# الافتراضي على ويندوز: C:\HafizSecrets\credentials.json و C:\HafizSecrets\drive-token
```

أكمِل الموافقة في المتصفح. ينتج ملف token داخل مجلد الـ token. بعدها يعمل الزر اليدوي والمهمة المجدولة **بدون متصفح**.

---

## النشر على سيرفر (Linux + SQL Server أصلي على نفس الجهاز)

> السيناريو: التطبيق منشور في `/var/www/Quranic_School` ويعمل كخدمة systemd خلف nginx، و SQL Server مثبت مباشرةً على Linux. يكتب `mssql` ملف الـ `.bak`، ويقرؤه/يحذفه مستخدم خدمة الويب — فيحتاج الطرفان وصولًا لنفس المجلد.

### 1. إعدادات الإنتاج بمسارات Linux

أنشئ `src/Hafiz.Web/appsettings.Production.json` (يُحمَّل تلقائيًا عند `ASPNETCORE_ENVIRONMENT=Production`):

```json
{
  "Backup": {
    "LocalFolder": "/var/www/hafiz/backup",
    "DailyTime": "02:00"
  },
  "GoogleDrive": {
    "CredentialsFilePath": "/etc/hafiz/credentials.json",
    "TokenFolder": "/etc/hafiz/drive-token",
    "TargetFolderId": "ضع-معرّف-المجلد-هنا"
  }
}
```

سلسلة اتصال الإنتاج تُضبط عبر متغيّر بيئة في وحدة systemd (لتجنّب وضع كلمة المرور في الريبو):

```
Environment=ConnectionStrings__DefaultConnection=Server=localhost;Database=QuranSchoolDB;User Id=...;Password=...;TrustServerCertificate=True;
```

### 2. صلاحيات مجلد النسخ (الخطوة الحرجة)

`mssql` يكتب الملف ومستخدم الويب (مثل `www-data`) يقرؤه ويحذفه — لذا نستخدم ملكية مجموعة مشتركة:

```bash
sudo mkdir -p /var/www/hafiz/backup

# المجموعة mssql تملك المجلد + كتابة للطرفين + setgid لتوريث المجموعة للملفات الجديدة
sudo chown www-data:mssql /var/www/hafiz/backup
sudo chmod 2770 /var/www/hafiz/backup

# ضمّ مستخدم خدمة الويب لمجموعة mssql
sudo usermod -aG mssql www-data

# ضروري: إعادة تشغيل الخدمة لتفعيل عضوية المجموعة الجديدة
sudo systemctl restart <اسم-الخدمة>
```

> إذا ظهر `Operating system error 5 (Access is denied)` فهذا سببه صلاحيات هذا المجلد.
> وتأكد أن المجلدات الأب قابلة للعبور: `ls -ld /var /var/www /var/www/hafiz` (يجب وجود `x` للآخرين).

### 3. رفع ملفات الأسرار إلى السيرفر

```bash
sudo mkdir -p /etc/hafiz/drive-token
# انسخ من جهازك المحلي:
#   credentials.json           → /etc/hafiz/credentials.json
#   محتوى مجلد الـ token        → /etc/hafiz/drive-token/
sudo chown -R www-data:www-data /etc/hafiz
sudo chmod 600 /etc/hafiz/credentials.json
sudo chmod 700 /etc/hafiz/drive-token     # المكتبة تُحدّث الـ token هنا → يحتاج كتابة
```

> الـ token مُولَّد محليًا (الخطوة «الموافقة لمرة واحدة») وهو JSON عادي **ينتقل من Windows إلى Linux دون إعادة موافقة ولا متصفح على السيرفر**.

### 4. الملفات التي تُرفع

- **التطبيق المُعاد نشره** (الكود الجديد مُترجَم داخل DLLs — لا يكفي تعديل الإعدادات فقط).
- **ملفّا الأسرار** (`credentials.json` + مجلد `drive-token`) — منفصلان عن git.
- **لا تُرفع** `tools/DriveAuth` (للموافقة المحلية فقط).

### 5. التوقيت والجدولة

- على systemd تبقى العملية حيّة فتعمل المهمة المجدولة بثبات — تأكد من **نسخة واحدة فقط** من الخدمة.
- اضبط المنطقة الزمنية وإلا تُنفَّذ الساعة `02:00` بتوقيت UTC:
  `sudo timedatectl set-timezone Asia/Amman` (أو `Environment=TZ=Asia/Amman` في الوحدة).

### 6. التحقق

1. سجّل دخول SuperAdmin → «النسخ الاحتياطي» → اضغط الزر → رسالة نجاح بدون متصفح.
2. تحقق من ظهور `QuranSchoolDB_yyyyMMdd_HHmmss.bak` في مجلد Google Drive.
3. عند الفشل راجع السجلات: `journalctl -u <اسم-الخدمة> -e`.

---

## المساهمة

المساهمات مرحّب بها! للمساهمة:

1. اعمل **Fork** للمستودع
2. أنشئ فرعاً جديداً: `git checkout -b feature/your-feature`
3. نفّذ التعديلات واعمل Commit: `git commit -m "feat: your feature"`
4. ارفع الفرع: `git push origin feature/your-feature`
5. افتح **Pull Request**

---

## التواصل

- تواصل معي عبر [LinkedIn](https://linkedin.com/in/abdulziz-alhariri)

---

<div align="center">

إذا أعجبك المشروع، لا تنسَ تعمل **Star** للمستودع ⭐

</div>
