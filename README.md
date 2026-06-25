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

**منصة حافظ (Hifz)** هي نظام ويب مفتوح المصدر مصمّم لإدارة دور ومراكز تحفيظ القرآن الكريم.
تربط المنصة بين **الإدارة**، **المعلّمين**، **الطلاب**، و**أولياء الأمور** في مكان واحد، بحيث تسهّل متابعة الحفظ والمراجعة والحضور والملاحظات بشكل يومي.

---

## المميزات

| الميزة                | الوصف                                      |
| --------------------- | ------------------------------------------ |
| إدارة المستخدمين      | أدوار متعددة: مدير، معلّم، طالب، وليّ أمر  |
| إدارة الصفوف والحلقات | إنشاء حلقات وربط الطلاب بالمعلّمين         |
| الحضور والغياب        | تسجيل حضور يومي للطلاب والمعلّمين          |
| الورد اليومي          | توزيع ومتابعة ورد الحفظ والمراجعة لكل طالب |
| ملاحظات المعلّم       | نظام ملاحظات بين المعلّم وولي الأمر        |
| دعم اللغات            | دعم للعربية والإنجليزية (Localization)     |
| تصميم متجاوب          | يعمل على جميع الأجهزة (PWA-ready)          |
| النسخ الاحتياطي       | نسخ احتياطي لقاعدة البيانات يدوي/مجدول يوميًا ورفعه تلقائيًا إلى Google Drive |

---

## التقنيات المستخدمة

| المجال       | التقنية                                |
| ------------ | -------------------------------------- |
| Backend      | ASP.NET Core 8 MVC                     |
| ORM          | Entity Framework Core 9                |
| Database     | SQL Server                             |
| Frontend     | Razor Views + Bootstrap 5 + JavaScript |
| Architecture | Clean Architecture                     |

---

## المعمارية — Clean Architecture

يتّبع المشروع مبادئ **Clean Architecture** لضمان الفصل بين الطبقات وقابلية الاختبار والصيانة.

```
┌──────────────────────────────────────────────┐
│              Hafiz.Web (Presentation)        │  ← واجهة المستخدم
│         Controllers · Views · Areas          │
└───────────────┬──────────────────────────────┘
                │ يعتمد على
┌───────────────▼──────────────────────────────┐
│           Hafiz.Application                  │  ← منطق التطبيق
│    Interfaces · Services · DTOs · Extensions │
└───────────────┬──────────────────────────────┘
                │ يعتمد على
┌───────────────▼──────────────────────────────┐
│            Hafiz.Domain                      │  ← جوهر النظام (لا يعتمد على أحد)
│            Entities · Enums                  │
└──────────────────────────────────────────────┘
                ▲
                │ يطبّق
┌───────────────┴──────────────────────────────┐
│          Hafiz.Infrastructure                │  ← البنية التحتية
│   Repositories · Data · Migrations · Services│
└──────────────────────────────────────────────┘
```

---

## هيكل المشروع

```
Hifz.sln
└── src/
    ├── Hafiz.Domain/                  ← الطبقة الجوهرية (Core)
    │   ├── Entities/                  # User, Student, Teacher, Parent, Class
    │   │                              # StudentAttendance, TeacherAttendance
    │   │                              # WirdAssignment, ParentNote, Video
    │   └── Enums/                     # تعدادات النظام
    │
    ├── Hafiz.Application/             ← طبقة التطبيق (Application)
    │   ├── Interfaces/
    │   │   ├── Repositories/          # واجهات المستودعات
    │   │   └── Services/              # واجهات الخدمات
    │   ├── Services/                  # منطق الأعمال
    │   ├── DTO/                       # كائنات نقل البيانات
        ├── Resources/                 # ملفات الترجمة (.resx)
    │   ├── Common/Helper/             # أدوات مساعدة
    │   └── Extensions/                # Extension Methods للـ DI
    │
    ├── Hafiz.Infrastructure/          ← طبقة البنية التحتية (Infrastructure)
    │   ├── Data/                      # ApplicationDbContext
    │   ├── Repositories/              # تطبيق واجهات المستودعات
    │   ├── Migrations/                # ترحيلات EF Core
    │   ├── Services/                  # خدمات خارجية (Hashing, ...)
    │   ├── Common/                    # أدوات مشتركة
    │   └── Extensions/                # Extension Methods للـ DI
    │
    └── Hafiz.Web/                     ← طبقة العرض (Presentation)
        ├── Areas/
        │   ├── Admin/                 # لوحة تحكم الإدارة
        │   ├── Teacher/               # واجهة المعلّم
        │   ├── Student/               # واجهة الطالب
        │   └── Parent/                # واجهة وليّ الأمر
        ├── Controllers/               # كنترولات عامة (Auth, Home)
        ├── Models/                    # ViewModels
        ├── Views/                     # صفحات Razor المشتركة
        ├── Resources/                 # ملفات الترجمة (.resx)
        └── wwwroot/                   # ملفات ثابتة (CSS, JS, Icons)
```

---

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

# 3. تطبيق ترحيلات قاعدة البيانات
cd src/Hafiz.Web
dotnet ef database update

# 4. تشغيل المشروع
dotnet run
```

بعد التشغيل، افتح المتصفح على: `https://localhost:5001`

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
