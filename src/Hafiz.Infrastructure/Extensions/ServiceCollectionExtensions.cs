using Hafiz.Application.Interfaces;
using Hafiz.Data;
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
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
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

            // Infrastructure services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IMeetingService, MeetingService>();

            return services;
        }
    }
}
