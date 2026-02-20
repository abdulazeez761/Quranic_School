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
