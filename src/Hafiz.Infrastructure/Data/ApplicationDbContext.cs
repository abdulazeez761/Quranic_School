using System;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

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
    }
}
