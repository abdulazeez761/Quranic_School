using Hifz.Repositories;
using Hifz.Repositories.Interfaces;
using Hifz.Services;
using Hifz.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hifz.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<ITeacherRepository, TeacherRepository>();
            services.AddScoped<IParentRepository, ParentRepository>();
            services.AddScoped<IClassRepository, ClassRepository>();
            services.AddScoped<ITeacherAttendanceRepository, TeacherAttendanceRepository>();
            services.AddScoped<IStudentAttendanceRepository, StudentAttendanceRepository>();
            services.AddScoped<IWirdRepository, WridRepository>();
            services.AddScoped<IParentNoteRepository, ParentNoteRepository>();

            return services;
        }

        public static IServiceCollection AddAppServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IParentService, ParentService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<ITeacherAttendanceService, TeacherAttendanceService>();
            services.AddScoped<IStudentAttendanceService, StudentAttendanceService>();
            services.AddScoped<IWirdService, WirdService>();
            services.AddScoped<IMeetingService, MeetingService>();
            services.AddScoped<IParentNoteService, ParentNoteService>();
            return services;
        }
    }
}
