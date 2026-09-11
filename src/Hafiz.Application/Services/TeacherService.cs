using Hafiz.Application.Interfaces;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IPasswordHasher _passwordHasher;

        public TeacherService(ITeacherRepository teacherRepository, IPasswordHasher passwordHasher)
        {
            _teacherRepository = teacherRepository;
            _passwordHasher = passwordHasher;
        }

        public Task<IEnumerable<Teacher>> GetAllTeachersAsync()
        {
            return _teacherRepository.GetAllAsync();
        }

        public Task<IEnumerable<Teacher>> GetAllTeachersByInstituteAsync(Guid instituteId)
        {
            return _teacherRepository.GetAllByInstituteAsync(instituteId);
        }

        public async Task<bool> DeleteTeacherAsync(Guid teacherId, Guid? instituteId = null)
        {
            return await _teacherRepository.DeleteAsync(teacherId, instituteId);
        }

        public async Task<bool> RestoreTeacherAsync(Guid teacherId, Guid? instituteId = null)
        {
            return await _teacherRepository.RestoreTeacherAsync(teacherId, instituteId);
        }

        public async Task<TeacherDto?> GetTeacherByIDAsync(Guid teacherId, Guid? instituteId = null)
        {
            Teacher? teacher = await _teacherRepository.GetTeacherByIDAsync(teacherId, instituteId);
            if (teacher == null)
                return null;

            var teacherDto = new TeacherDto
            {
                Id = teacher.UserId,
                Username = teacher.TeacherInfo?.Username ?? string.Empty,
                FirstName = teacher.TeacherInfo?.FirstName ?? string.Empty,
                SecondName = teacher.TeacherInfo?.SecondName ?? string.Empty,
                PhoneNumber = teacher.TeacherInfo?.PhoneNumber ?? string.Empty,
                Email = teacher.TeacherInfo?.Email,
            };
            return teacherDto;
        }

        public async Task UpdateTeacherAsync(TeacherDto teacher)
        {
            if (!string.IsNullOrEmpty(teacher.Password))
            {
                teacher.Password = _passwordHasher.HashPassword(teacher.Password);
            }

            await _teacherRepository.UpdateAsync(teacher);
        }

        public async Task<IList<Class>?> GetTeacherClasses(Guid teacherId)
        {
            if (teacherId == Guid.Empty)
                return null;
            var teacher = await _teacherRepository.GetTeacherByIDAsync(teacherId);
            if (teacher == null)
                return null;

            var teacherClasses = await _teacherRepository.GetTeacherClasses(teacherId);
            return teacherClasses;
        }

        public Task<IEnumerable<Teacher>> GetArchivedTeachersByInstituteAsync(Guid instituteId)
        {
            return _teacherRepository.GetArchivedTeachersByInstituteAsync(instituteId);
        }

        public Task<IEnumerable<Teacher>> GetArchivedTeachersAsync()
        {
            return _teacherRepository.GetArchivedTeachersAsync();
        }

        public Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null)
        {
            return _teacherRepository.GetCountsAsync(instituteId);
        }
    }
}
