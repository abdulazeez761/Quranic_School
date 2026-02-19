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

        public async Task<bool> DeleteTeacherAsync(Guid teacherId)
        {
            var teachers = await _teacherRepository.GetAllAsync();
            var teacher = teachers.FirstOrDefault(t => t.UserId == teacherId);
            if (teacher == null)
            {
                return false;
            }
            await _teacherRepository.DeleteAsync(teacher);
            return true;
        }

        public async Task<TeacherDto?> GetTeacherByIDAsync(Guid teacherId)
        {
            Teacher? teacher = await _teacherRepository.GetTeacherByIDAsync(teacherId);
            var teacherDto = new TeacherDto
            {
                Id = teacher.UserId,
                Username = teacher.TeacherInfo.Username,
                FirstName = teacher.TeacherInfo.FirstName,
                SecondName = teacher.TeacherInfo.SecondName,
                PhoneNumber = teacher.TeacherInfo.PhoneNumber,
                Email = teacher.TeacherInfo.Email,
            };
            if (teacher == null)
                return null;
            else
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
    }
}
