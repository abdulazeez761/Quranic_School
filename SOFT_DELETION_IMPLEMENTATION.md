# 🛡️ دليل وخطة تطبيق الحذف الناعم (Soft Delete) وحماية الإحصائيات
### نظام إدارة مدرسة تحفيظ القرآن الكريم (`Hafiz`)

---

## 📌 نبذة عامة والهدف المعماري
الهدف من هذا الدليل هو تحويل عمليات الحذف في كامل النظام إلى **حذف ناعم (Soft Delete)** للكيانات الأساسية:
- **الطالب (`Student`)**
- **الشعبة / الحلقة (`Class`)**
- **المعلم (`Teacher`)**
- **المركز (`Institute`)**
- **المستخدم (`User`)**
- **ولي الأمر (`Parent`)**

### 🎯 النتيجة المطلوبة:
عند حذف أي طالب أو شعبة أو معلم أو مركز:
1. **عدم مسح أي سجلات مرتبطة** (مثل: الأوراد المحفوظة، أوراد المراجعة، سجلات حضور وغياب الطلاب والمعلمين، والملاحظات).
2. **عدم تصفير أو نقص إحصائيات المركز التاريخية والتراكمية** (إجمالي الصفحات والأجزاء المحفوظة للمركز تظل ثابتة ولا تتأثر بحذف الطالب لاحقاً).
3. **استبعاد المحذوفين فقط من الأعداد والأنشطة اللحظية** (عدد الطلاب الحاليين، المعلمين الحاليين، الحلقات النشطة، حضور اليوم المتوقع).
4. **منع تسجيل دخول أي مستخدم محذوف**.

---

## ⚡ القواعد الذهبية للحذف الناعم في هذا المشروع

> [!CAUTION]
> **1. إزالة الحذف العنيف الصريح في مستودع الحلقات (`ClassRepository`)**
> كان يوجد كود صريح في `ClassRepository.Delete`:
> ```csharp
> _context.StudentAttendances.RemoveRange(classToDelete.StudentAttendances);
> ```
> هذا السطر يمسح كل حضور الطلاب السابق في الحلقة للأبد! **تم حذفه** لضمان بقاء سجلات الحضور التاريخية.

> [!WARNING]
> **2. فخ استعلامات الـ Joined Entities في EF Core**
> عند وضع Global Query Filter على الطالب `!s.IsDeleted`، واستعلام صفحات المركز عبر:
> ```csharp
> w.Student.StudentInfo.InstituteId == instituteId
> ```
> فإن EF Core سيستبعد تلقائياً أوراد الطلاب المحذوفين مما يؤدي لنقص إنجازات المركز في لوحة التحكم!
> **الحل:** استخدام `.IgnoreQueryFilters()` دائماً في استعلامات الأوراد التراكمية التاريخية.

> [!IMPORTANT]
> **3. الفهرس الفريد لاسم المستخدم (Filtered Unique Index)**
> في SQL Server، وجود قيد تفرد عادي على `Username` يمنع إعادة تسجيل نفس اسم المستخدم إذا كان المستخدم القديم محذوفاً ناعماً (`IsDeleted = 1`).
> **الحل:** استخدام Filtered Index يطبق شرط التفرد فقط على غير المحذوفين:
> ```csharp
> modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique().HasFilter("[IsDeleted] = 0");
> ```

---

## 📋 قائمة المهام التنفيذية (Checklist)

---

### المرحلة 01: هندسة طبقة النطاق (Domain Layer)

- [x] **1.1 إنشاء واجهة `ISoftDeletable`**
  - **الملف:** `src/Hafiz.Domain/Common/ISoftDeletable.cs`
  ```csharp
  using System;

  namespace Hafiz.Domain.Common
  {
      public interface ISoftDeletable
      {
          bool IsDeleted { get; set; }
          DateTime? DeletedAt { get; set; }
          Guid? DeletedBy { get; set; }
      }
  }
  ```

- [x] **1.2 تطبيق الواجهة على كيان المستخدم `User.cs`**
  - **الملف:** `src/Hafiz.Domain/Entities/User.cs`
  ```csharp
  public class User : ISoftDeletable
  {
      // ...
      public bool IsDeleted { get; set; } = false;
      public DateTime? DeletedAt { get; set; }
      public Guid? DeletedBy { get; set; }
  }
  ```

- [x] **1.3 تطبيق الواجهة على الطالب والمعلم `Student.cs` و `Teacher.cs`**
  - **الملف:** `src/Hafiz.Domain/Entities/Student.cs`
  - **الملف:** `src/Hafiz.Domain/Entities/Teacher.cs`
  ```csharp
  public class Student : ISoftDeletable
  {
      // ...
      public bool IsDeleted { get; set; } = false;
      public DateTime? DeletedAt { get; set; }
      public Guid? DeletedBy { get; set; }
  }

  public class Teacher : ISoftDeletable
  {
      // ...
      public bool IsDeleted { get; set; } = false;
      public DateTime? DeletedAt { get; set; }
      public Guid? DeletedBy { get; set; }
  }
  ```

- [x] **1.4 تطبيق الواجهة على الشعبة والمركز وولي الأمر**
  - **الملفات:** `Class.cs`, `Institute.cs`, `Parent.cs`
  ```csharp
  public class Class : ISoftDeletable { ... }
  public class Institute : ISoftDeletable { ... }
  public class Parent : ISoftDeletable { ... }
  ```

---

### المرحلة 02: ضبط `ApplicationDbContext` و Entity Framework Core

- [x] **2.1 اعتراض عمليات الحذف تلقائياً في `SaveChangesAsync` و `SaveChanges`**
  - **الملف:** `src/Hafiz.Infrastructure/Data/ApplicationDbContext.cs`
  ```csharp
  public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
      ApplySoftDeleteRules();
      return base.SaveChangesAsync(cancellationToken);
  }

  public override int SaveChanges()
  {
      ApplySoftDeleteRules();
      return base.SaveChanges();
  }

  private void ApplySoftDeleteRules()
  {
      var entries = ChangeTracker.Entries<ISoftDeletable>()
          .Where(e => e.State == EntityState.Deleted);

      foreach (var entry in entries)
      {
          entry.State = EntityState.Modified; // تحويل الحذف لتعديل
          entry.Entity.IsDeleted = true;
          entry.Entity.DeletedAt = DateTime.UtcNow;
      }
  }
  ```

- [x] **2.2 تفعيل الـ Global Query Filters في `OnModelCreating`**
  - **الملف:** `src/Hafiz.Infrastructure/Data/ApplicationDbContext.cs`
  ```csharp
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
      base.OnModelCreating(modelBuilder);

      // Global Query Filters
      modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
      modelBuilder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
      modelBuilder.Entity<Teacher>().HasQueryFilter(t => !t.IsDeleted);
      modelBuilder.Entity<Class>().HasQueryFilter(c => !c.IsDeleted);
      modelBuilder.Entity<Institute>().HasQueryFilter(i => !i.IsDeleted);
      modelBuilder.Entity<Parent>().HasQueryFilter(p => !p.IsDeleted);
  }
  ```

- [x] **2.3 تعديل فهرس اسم المستخدم الفريد إلى Filtered Unique Index**
  - **الملف:** `src/Hafiz.Infrastructure/Data/ApplicationDbContext.cs`
  ```csharp
  modelBuilder.Entity<User>()
      .HasIndex(u => u.Username)
      .IsUnique()
      .HasFilter("[IsDeleted] = 0");
  ```

- [ ] **2.4 توليد وتطبيق ترحيل قاعدة البيانات (Migration)**
  - تشغيل الأوامر التالية في Terminal:
  ```bash
  dotnet ef migrations add AddSoftDeleteSupport --project src/Hafiz.Infrastructure --startup-project src/Hafiz.Web
  dotnet ef database update --project src/Hafiz.Infrastructure --startup-project src/Hafiz.Web
  ```

---

### المرحلة 03: تنظيف وتأمين المستودعات (Repositories)

- [x] **3.1 تأمين حذف الشعبة في `ClassRepository.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Repositories/ClassRepository.cs`
  - إزالة سطر مسح الحضور، والاكتفاء بالحذف الناعم:
  ```csharp
  public async Task<bool> Delete(Guid Id)
  {
      var classToDelete = await _context.Classes.FirstOrDefaultAsync(c => c.Id == Id);
      if (classToDelete != null)
      {
          _context.Classes.Remove(classToDelete); // يتحول تلقائياً إلى IsDeleted = true
          await _context.SaveChangesAsync();
          return true;
      }
      return false;
  }
  ```

- [x] **3.2 تأمين حذف الطالب ومستخدمه في `StudentRepository.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Repositories/StudentRepository.cs`
  ```csharp
  public async Task DeleteAsync(Guid id)
  {
      var student = await GetByIdAsync(id);
      if (student is null) return;

      _context.Students.Remove(student);
      if (student.StudentInfo != null)
      {
          _context.Users.Remove(student.StudentInfo);
      }
      await _context.SaveChangesAsync();
  }
  ```

- [x] **3.3 تأمين حذف المعلم في `TeacherRepository.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Repositories/TeacherRepository.cs`
  ```csharp
  public Task DeleteAsync(Teacher teacher)
  {
      _context.Teachers.Remove(teacher);
      _context.Users.Remove(teacher.TeacherInfo);
      return _context.SaveChangesAsync();
  }
  ```

- [x] **3.4 تأمين حذف المركز في `InstituteRepository.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Repositories/InstituteRepository.cs`
  ```csharp
  public async Task DeleteAsync(Guid id)
  {
      var institute = await _context.Institutes.FindAsync(id);
      if (institute != null)
      {
          _context.Institutes.Remove(institute);
          await _context.SaveChangesAsync();
      }
  }
  ```

- [ ] **3.5 إضافة دوال الاستعادة (Restore Pattern) عند الحاجة**
  - دالة استعادة الطالب كمثال:
  ```csharp
  public async Task<bool> RestoreStudentAsync(Guid studentId)
  {
      var student = await _context.Students
          .IgnoreQueryFilters()
          .Include(s => s.StudentInfo)
          .FirstOrDefaultAsync(s => s.UserId == studentId);

      if (student == null || !student.IsDeleted) return false;

      student.IsDeleted = false;
      student.DeletedAt = null;
      if (student.StudentInfo != null)
      {
          student.StudentInfo.IsDeleted = false;
          student.StudentInfo.DeletedAt = null;
      }
      await _context.SaveChangesAsync();
      return true;
  }
  ```

---

### المرحلة 04: حماية الإحصائيات ولوحة التحكم والتقارير

- [x] **4.1 حماية إحصائيات حفظ ومراجعة القرآن التراكمية في `DashboardService.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Services/DashboardService.cs`
  - في دالة `AggregateWirdUnitsAsync`:
  ```csharp
  private async Task<(double memPages, double memJuz, int memAyahs, double revPages, double revJuz, int revAyahs)> AggregateWirdUnitsAsync(Guid? instituteId, DashboardPeriod period)
  {
      // استخدام IgnoreQueryFilters لضمان بقاء أوراد الطلاب المحذوفين ضمن إنجاز المركز
      var wirdsQuery = _context.WirdAssignments
          .IgnoreQueryFilters()
          .AsNoTracking();

      if (instituteId.HasValue)
      {
          wirdsQuery = wirdsQuery.Where(w => w.Student.StudentInfo.InstituteId == instituteId);
      }

      var (from, toExclusive) = DashboardPeriodRange.Resolve(period);
      if (from.HasValue)
      {
          wirdsQuery = wirdsQuery.Where(w => w.AssignedDate >= from.Value && w.AssignedDate < toExclusive!.Value);
      }

      var assignments = await wirdsQuery
          .Where(w => w.Type == AssignmentType.Memorization || w.Type == AssignmentType.Revision)
          .Select(w => new WirdUnitsProjection { ... })
          .ToListAsync();

      // التجميع الحسابي يبقى كما هو
  }
  ```

- [x] **4.2 حماية تقارير الأوراد وتصدير Excel في `WirdRepository.cs`**
  - **الملف:** `src/Hafiz.Infrastructure/Repositories/WirdRepository.cs`
  - في دالة `BuildReportQuery`:
  ```csharp
  private IQueryable<WirdAssignment> BuildReportQuery(WirdReportFilterDto filter)
  {
      var query = _context.WirdAssignments
          .IgnoreQueryFilters()
          .AsNoTracking();

      if (filter.InstituteId.HasValue)
          query = query.Where(w => w.Student.StudentInfo.InstituteId == filter.InstituteId);

      // باقي التصفية كما هي
      return query;
  }
  ```

- [x] **4.3 فحص الأعداد اللحظية في لوحة التحكم (Live Counts)**
  - في `DashboardService.LoadCountsAsync`:
  - أعداد `MaleStudents`، `FemaleStudents`، `Teachers`، `Classes`، و `ExpectedStudentsToday` تعتمد على الفلتر التلقائي `!IsDeleted` وتستبعد المحذوفين بشكل سليم وتلقائي.
  - الحضور اللحظي الفعلي للطلاب والمعلمين مؤمن بشرط `IsDeleted == false` صريح.

- [x] **4.4 حماية التقارير اليومية وسجلات الحضور والأوراد للأيام السابقة (Historical Data Protection)**
  - في `StudentRepository.GetStudentByInstituteIdAsyncAndClassDay`: عند طلب تاريخ سابق، يُستخدم `.IgnoreQueryFilters()` لجلب الطلاب الذين أنجزوا أوراداً أو سجلوا حضوراً في ذلك اليوم.
  - في `StudentAttendanceRepository.GetStudentsByClassId`: للأيام السابقة يتم جلب الطلاب المحذوفين الذين حضروا في تلك الحلقة.
  - في `TeacherAttendanceRepository`: للأيام السابقة يتم جلب المعلمين المحذوفين الذين سجلوا دواماً.
  - في `WirdRepository.GetWirdAssignmentsByClassIdAsync`: دعم بقاء أوراد الطلاب المحذوفين في استعراض الحلقة.

---

### المرحلة 05: الأمان والمصادقة (Authentication & Authorization)

- [ ] **5.1 حظر تسجيل الدخول للمستخدمين المحذوفين ناعماً**
  - في `AuthService.LoginAsync`: استعلام `UserRepository.GetByUsernameAsync` يستبعد المحذوفين تلقائياً بفضل الفلتر العام، فتفشل محاولة الدخول مباشرة.
- [ ] **5.2 إبطال الجلسات النشطة فور الحذف (Cookie Invalidation)**
  - في إعدادات `CookieAuthenticationEvents.OnValidatePrincipal` في `Program.cs`:
  ```csharp
  options.Events = new CookieAuthenticationEvents
  {
      OnValidatePrincipal = async context =>
      {
          var db = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
          var userIdStr = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
          if (Guid.TryParse(userIdStr, out var userId))
          {
              var userExists = await db.Users.AnyAsync(u => u.Id == userId);
              if (!userExists) // محذوف ناعماً
              {
                  context.RejectPrincipal();
                  await context.HttpContext.SignOutAsync();
              }
          }
      }
  };
  ```

---

### المرحلة 06: واجهة المستخدم وإدارة الأرشيف (UI)

- [ ] **6.1 تحديث نصوص نوافذ تأكيد الحذف (Delete Modals)**
  - تعديل رسالة التأكيد لتوضيح: *"سيتم أرشفة السجل وإيقاف الحساب، ولن تتأثر إحصائيات الحفظ وسجلات الحضور والتقارير السابقة."*
- [ ] **6.2 إضافة خيار استعراض المؤرشفين وزر الاستعادة (Restore)**
  - في شاشات الإدارة (الطلاب، الحلقات، المعلمين)، توفير تبويب أو فلتر لعرض السجلات المؤرشفة مع زر `استعادة`.

---

## 🧪 مصفوفة الاختبار والتحقق العملي

| الاختبار | الإجراء | النتيجة المتوقعة |
| :--- | :--- | :--- |
| **حذف طالب** | تسجيل 10 صفحات حفظ وحضور 5 أيام لطالب، ثم حذفه | عدد الطلاب النشطين ينقص بـ 1، إجمالي صفحات حفظ المركز **لا تنقص أبداً**، وسجلات الحضور والأوراد تبقى في DB |
| **حذف شعبة** | حذف شعبة مسجل بها حضور سابق | الشعبة تختفي من الحلقات النشطة، سجلات الحضور السابقة **تبقى كاملة** في التقارير |
| **حذف معلم** | حذف معلم مسجل له حضور وساعات عمل | المعلم لا يمكنه تسجيل الدخول، وسجلات دوامه السابقة **تبقى كاملة** |
| **تسجيل الدخول** | محاولة دخول بحساب طالب أو معلم محذوف | تظهر رسالة: "اسم المستخدم أو كلمة المرور غير صحيحة" |
| **إعادة التسجيل** | تسجيل مستخدم جديد بنفس اسم مستخدم قديم محذوف | ينجح التسجيل دون أي خطأ بفضل الـ Filtered Unique Index |

### 🔍 استعلامات SQL للفحص المباشر في قاعدة البيانات:
```sql
-- 1. التأكد من بقاء الطالب مع وسم IsDeleted = 1
SELECT UserId, IsDeleted, DeletedAt FROM Students WHERE IsDeleted = 1;

-- 2. التأكد من بقاء أوراد الطالب المحذوف سليمة
SELECT COUNT(*) AS PreservedWirds 
FROM WirdAssignments 
WHERE StudentId IN (SELECT UserId FROM Students WHERE IsDeleted = 1);

-- 3. التأكد من بقاء سجلات حضور الطالب المحذوف سليمة
SELECT COUNT(*) AS PreservedAttendance 
FROM StudentAttendances 
WHERE StudentId IN (SELECT UserId FROM Students WHERE IsDeleted = 1);

-- 4. التأكد من بقاء الحلقات المحذوفة ناعماً
SELECT Id, Name, IsDeleted, DeletedAt FROM Classes WHERE IsDeleted = 1;
```

---
*تم إنشاء هذا الدليل ليكون مرجعاً تقنياً وتنفيذياً شاملاً لمشروع Hafiz.*
