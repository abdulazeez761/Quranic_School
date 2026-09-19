using Hafiz.Application.Services;
using Hafiz.Services;
using Hafiz.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Hafiz.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITeacherService, TeacherService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<IParentService, ParentService>();
            services.AddScoped<IClassService, ClassService>();
            services.AddScoped<ITeacherAttendanceService, TeacherAttendanceService>();
            services.AddScoped<IStudentAttendanceService, StudentAttendanceService>();
            services.AddScoped<IWirdService, WirdService>();
            services.AddScoped<IParentNoteService, ParentNoteService>();
            services.AddScoped<IInstituteService, InstituteService>();
            services.AddScoped<IStudentRoutinePlanService, StudentRoutinePlanService>();
            services.AddScoped<IStudentWirdService, StudentWirdService>();
            services.AddScoped<IStudyProgramService, StudyProgramService>();
            services.AddScoped<IMatnService, MatnService>();
            services.AddScoped<IMatnAssignmentService, MatnAssignmentService>();
            services.AddScoped<IStudentMatnProgressService, StudentMatnProgressService>();
            return services;
        }
    }
}
