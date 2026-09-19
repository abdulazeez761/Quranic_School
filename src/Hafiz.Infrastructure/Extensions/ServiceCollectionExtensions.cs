using Hafiz.Application.Interfaces;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Application.Interfaces.Services;
using Hafiz.Data;
using Hafiz.Infrastructure.Repositories;
using Hafiz.Infrastructure.Security;
using Hafiz.Infrastructure.Services;
using Hafiz.Repositories;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hafiz.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure();
                    }
                )
            );

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<IParentRepository, ParentRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<ITeacherAttendanceRepository, TeacherAttendanceRepository>();
            services.AddScoped<IStudentAttendanceRepository, StudentAttendanceRepository>();
            services.AddScoped<IWirdRepository, WirdRepository>();
            services.AddScoped<IParentNoteRepository, ParentNoteRepository>();
            services.AddScoped<IInstituteRepository, InstituteRepository>();
            services.AddScoped<IStudentRoutinePlanRepository, StudentRoutinePlanRepository>();
            services.AddScoped<IStudyProgramRepository, StudyProgramRepository>();
            services.AddScoped<IMatnRepository, MatnRepository>();
            services.AddScoped<IMatnAssignmentRepository, MatnAssignmentRepository>();
            services.AddScoped<IStudentMatnProgressRepository, StudentMatnProgressRepository>();

            // Infrastructure services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IGoogleDriveUploader, GoogleDriveUploader>();
            services.AddScoped<IBackupService, BackupService>();

            return services;
        }
    }
}
