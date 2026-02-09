<div align="center">

# Hifz

**نظام متكامل لإدارة حلقات تحفيظ القرآن الكريم**

[![.NET](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)](https://dotnet.microsoft.com/)
[![EF Core](https://img.shields.io/badge/EF%20Core-9.0-blue?logo=nuget)](https://learn.microsoft.com/en-us/ef/core/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Latest-red?logo=microsoftsqlserver)](https://www.microsoft.com/en-us/sql-server)
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

- **Backend:** ASP.NET Core 8 MVC
- **ORM:** Entity Framework Core 9
- **Database:** SQL Server
- **Frontend:** Razor Views + Bootstrap + JavaScript
- **Architecture:** Repository Pattern + Service Layer + DTOs

---

## هيكل المشروع

```
Hifz/
├── Areas/
│   ├── Admin/          # لوحة تحكم الإدارة
│   ├── Teacher/        # واجهة المعلّم
│   ├── Student/        # واجهة الطالب
│   └── Parent/         # واجهة وليّ الأمر
├── Controllers/        # الكنترولات العامة (Auth, Home)
├── Models/             # نماذج قاعدة البيانات
├── DTOs/               # كائنات نقل البيانات
├── Services/           # طبقة منطق الأعمال
├── Repositories/       # طبقة الوصول للبيانات
├── Common/             # أدوات مساعدة (Hashing, Formatting)
├── Views/              # صفحات Razor المشتركة
├── Resources/          # ملفات الترجمة
├── Migrations/         # ملفات ترحيل قاعدة البيانات
└── wwwroot/            # الملفات الثابتة (CSS, JS, Icons)
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

# 2. تحديث إعدادات قاعدة البيانات في appsettings.json
# عدّل ConnectionStrings حسب بيئتك

# 3. إنشاء قاعدة البيانات
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
3. نفّذ التعديلات واعمل Commit: `git commit -m "Add: your feature"`
4. ارفع الفرع: `git push origin feature/your-feature`
5. افتح **Pull Request**

---

## التواصل

- تواصل معي عبر [LinkedIn](https://linkedin.com/in/abdulziz-alhariri)

---

<div align="center">

اذا أعجبك المشروع، لا تنسَ تعمل **Star** للمستودع

</div>
