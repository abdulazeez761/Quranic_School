using System;
using Hafiz.Domain.Common;
using Hafiz.Domain.Entities;
using Hafiz.Models;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Parent> Parents { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<WirdAssignment> WirdAssignments { get; set; }
    public DbSet<StudentAttendance> StudentAttendances { get; set; }
    public DbSet<TeacherAttendance> teacherAttendances { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<ParentNote> ParentNotes { get; set; }
    public DbSet<Institute> Institutes { get; set; }
    public DbSet<StudentRoutinePlan> StudentRoutinePlans { get; set; }
    public DbSet<StudyProgram> StudyPrograms { get; set; }
    public DbSet<Matn> Matns { get; set; }
    public DbSet<MatnAssignment> MatnAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // to not ignore the base modelBuilder configurations

        // Global Query Filters لإخفاء المحذوفين ناعماً
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Student>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Teacher>().HasQueryFilter(t => !t.IsDeleted);
        modelBuilder.Entity<Class>().HasQueryFilter(c => !c.IsDeleted);
        modelBuilder.Entity<Institute>().HasQueryFilter(i => !i.IsDeleted);
        modelBuilder.Entity<Parent>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<StudyProgram>().HasQueryFilter(sp => !sp.IsDeleted);
        modelBuilder.Entity<Matn>().HasQueryFilter(m => !m.IsDeleted);

        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique(); // Ensure unique usernames even if soft-deleted because i might return the user
        modelBuilder
            .Entity<Class>()
            .HasMany(c => c.Teachers)
            .WithMany(t => t.Classes)
            .UsingEntity<Dictionary<string, object>>( // automatically creates join table
                "ClassTeachers", // join table name
                j =>
                    j.HasOne<Teacher>()
                        .WithMany()
                        .HasForeignKey("TeacherId")
                        .OnDelete(DeleteBehavior.Cascade),
                j =>
                    j.HasOne<Class>()
                        .WithMany()
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
            );

        // Student → User
        modelBuilder
            .Entity<Student>()
            .HasOne(s => s.StudentInfo)
            .WithOne(u => u.Student)
            .HasForeignKey<Student>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict); // ⛔ Prevent cascade delete

        // Student → Parent
        modelBuilder
            .Entity<Student>()
            .HasOne(s => s.Parent)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Restrict); // ⛔ Prevent cascade delete

        modelBuilder
            .Entity<Class>()
            .HasMany(c => c.Students)
            .WithMany(t => t.Classes)
            .UsingEntity<Dictionary<string, object>>( // automatically creates join table
                "ClassStudents", // join table name
                j =>
                    j.HasOne<Student>()
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade),
                j =>
                    j.HasOne<Class>()
                        .WithMany()
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
            );

        modelBuilder
            .Entity<Teacher>()
            .HasMany(t => t.Attendances)
            .WithOne(a => a.Teacher)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<TeacherAttendance>()
            .HasOne(a => a.Class)
            .WithMany(c => c.TeacherAttendance)
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Student>()
            .HasMany(s => s.Attendances)
            .WithOne(a => a.Student)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<StudentAttendance>()
            .HasOne(a => a.Class)
            .WithMany(c => c.StudentAttendances)
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Student>()
            .HasMany(s => s.wirds)
            .WithOne(a => a.Student)
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<WirdAssignment>()
            .HasOne(a => a.Student)
            .WithMany(c => c.wirds)
            .HasForeignKey(w => w.StudentId);

        // Wird belongs to the class it was assigned within. SetNull on delete so
        // deleting a class doesn't wipe historical wirds — they just lose the class link.
        modelBuilder
            .Entity<WirdAssignment>()
            .HasOne(w => w.Class)
            .WithMany()
            .HasForeignKey(w => w.ClassId)
            .OnDelete(DeleteBehavior.SetNull);

        // High-performance composite index for latest-wird and historical queries
        modelBuilder
            .Entity<WirdAssignment>()
            .HasIndex(w => new { w.StudentId, w.Type, w.AssignedDate })
            .HasDatabaseName("IX_WirdAssignments_Student_Type_Date");

        //parent --> student
        modelBuilder
            .Entity<Parent>()
            .HasMany(p => p.Students)
            .WithOne(s => s.Parent)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Student>()
            .HasOne(s => s.Parent)
            .WithMany(p => p.Students)
            .HasForeignKey(s => s.ParentId)
            .OnDelete(DeleteBehavior.SetNull);

        // ParentNote relationships
        modelBuilder
            .Entity<ParentNote>()
            .HasOne(pn => pn.Student)
            .WithMany()
            .HasForeignKey(pn => pn.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ParentNote>()
            .HasOne(pn => pn.CreatedByUser)
            .WithMany()
            .HasForeignKey(pn => pn.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<Class>()
            .HasMany(c => c.StudentAttendances)
            .WithOne(sa => sa.Class)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder
            .Entity<Class>()
            .HasMany(c => c.TeacherAttendance)
            .WithOne(sa => sa.Class)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<Institute>()
            .HasMany(i => i.Users)
            .WithOne(u => u.Institute)
            .HasForeignKey(u => u.InstituteId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder
            .Entity<Institute>()
            .HasMany(i => i.Classes)
            .WithOne(c => c.Institute)
            .HasForeignKey(c => c.InstituteId)
            .OnDelete(DeleteBehavior.Restrict);

        // StudyProgram configurations
        modelBuilder
            .Entity<StudyProgram>()
            .HasOne(sp => sp.Institute)
            .WithMany()
            .HasForeignKey(sp => sp.InstituteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder
            .Entity<StudyProgram>()
            .HasOne(sp => sp.Matn)
            .WithMany()
            .HasForeignKey(sp => sp.MatnId)
            .OnDelete(DeleteBehavior.Restrict);

        // Class -> StudyProgram
        modelBuilder
            .Entity<Class>()
            .HasOne(c => c.StudyProgram)
            .WithMany(sp => sp.Classes)
            .HasForeignKey(c => c.StudyProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        // MatnAssignment configurations
        modelBuilder
            .Entity<MatnAssignment>()
            .HasOne(ma => ma.Student)
            .WithMany()
            .HasForeignKey(ma => ma.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<MatnAssignment>()
            .HasOne(ma => ma.Class)
            .WithMany()
            .HasForeignKey(ma => ma.ClassId)
            .OnDelete(DeleteBehavior.Restrict);

        // Composite Performance & Multi-Tenancy Indexes
        modelBuilder
            .Entity<StudyProgram>()
            .HasIndex(sp => new { sp.InstituteId, sp.Type })
            .HasDatabaseName("IX_StudyPrograms_InstituteId_Type");

        modelBuilder
            .Entity<Class>()
            .HasIndex(c => new { c.InstituteId, c.StudyProgramId })
            .HasDatabaseName("IX_Classes_InstituteId_StudyProgramId");

        modelBuilder
            .Entity<MatnAssignment>()
            .HasIndex(ma => new { ma.StudentId, ma.ClassId, ma.AssignedDate })
            .HasDatabaseName("IX_MatnAssignments_Student_Class_Date");
    }

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
        // change tracker is a property of the DbContext that monitors all the entities in the context and that being modified.
        var entries = ChangeTracker
            .Entries<ISoftDeletable>()
            .Where(e => e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            /*
            the state = entitystate.modified is used to indicate that the entity has been modified and
            should be updated in the database.
            because in case of deletion this will be stat = delete and will delete the recode from the db we want to avoid that
            and just update the isdeleted and deletedat fields so we will change the state to modified

            State = Modified
            IsDeleted = true
            DeletedAt = now
            */
            entry.State = EntityState.Modified;

            entry.CurrentValues["IsDeleted"] = true;
            entry.CurrentValues["DeletedAt"] = DateTime.UtcNow;
        }
    }
}
