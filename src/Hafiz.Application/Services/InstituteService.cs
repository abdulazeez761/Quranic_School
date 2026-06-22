using Hafiz.Application.Interfaces;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Domain.Entities;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class InstituteService : IInstituteService
    {
        private readonly IInstituteRepository _instituteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public InstituteService(
            IInstituteRepository instituteRepository,
            IUserRepository userRepository,
            IPasswordHasher passwordHasher
        )
        {
            _instituteRepository = instituteRepository;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public Task<List<Institute>> GetAllAsync()
        {
            return _instituteRepository.GetAllAsync();
        }

        public Task<Institute> GetByIdAsync(Guid id)
        {
            return _instituteRepository.GetByIdAsync(id);
        }

        public async Task<Institute> CreateAsync(
            string name,
            string? description,
            string? address,
            string? phoneNumber
        )
        {
            var institute = new Institute
            {
                Name = name,
                Description = description,
                Address = address,
                PhoneNumber = phoneNumber,
            };
            return await _instituteRepository.CreateAsync(institute);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateAsync(UpdateInstituteDto dto)
        {
            var institute = await _instituteRepository.GetByIdAsync(dto.Id);
            if (institute == null)
                return (false, "المركز غير موجود.");

            if (!string.Equals(institute.Name, dto.Name, StringComparison.Ordinal)
                && await _instituteRepository.NameExistsAsync(dto.Name))
                return (false, "اسم المركز مستخدم بالفعل.");

            institute.Name = dto.Name;
            institute.Description = dto.Description;
            institute.Address = dto.Address;
            institute.PhoneNumber = dto.PhoneNumber;

            await _instituteRepository.UpdateAsync(institute);
            return (true, "تم تحديث المركز بنجاح.");
        }

        public Task DeleteAsync(Guid id)
        {
            return _instituteRepository.DeleteAsync(id);
        }

        public async Task<(bool Success, string ErrorMessage)> CreateInstituteWithAdminAsync(
            CreateInstituteDto dto
        )
        {
            // Check if institute name already exists
            if (await _instituteRepository.NameExistsAsync(dto.InstituteName))
                return (false, "اسم المركز مستخدم بالفعل.");

            // Check if admin username already exists
            if (await _userRepository.GetByUsernameAsync(dto.AdminUsername) is not null)
                return (false, "اسم المستخدم مستخدم بالفعل.");

            // Create the institute
            var institute = new Institute
            {
                Name = dto.InstituteName,
                Description = dto.Description,
                Address = dto.Address,
                PhoneNumber = dto.InstitutePhone,
            };

            var createdInstitute = await _instituteRepository.CreateAsync(institute);
            if (createdInstitute == null)
                return (false, "حدث خطأ أثناء إنشاء المركز.");

            // Create the admin user for this institute
            var adminUser = new User
            {
                Username = dto.AdminUsername,
                FirstName = dto.AdminFirstName,
                SecondName = dto.AdminSecondName,
                PhoneNumber = dto.AdminPhoneNumber,
                Email = dto.AdminEmail,
                Password = _passwordHasher.HashPassword(dto.AdminPassword),
                Role = UserRole.Admin,
                InstituteId = createdInstitute.Id,
            };

            await _userRepository.AddAsync(adminUser);

            // Set the manager of the institute
            createdInstitute.ManagerId = adminUser.Id;
            await _instituteRepository.UpdateAsync(createdInstitute);

            return (true, "تم إنشاء المركز والمدير بنجاح.");
        }

        public Task<List<User>> GetInstituteAdminsAsync(Guid instituteId)
        {
            return _instituteRepository.GetInstituteAdminsAsync(instituteId);
        }

        public Task<int> GetStudentCountAsync(Guid instituteId)
        {
            return _instituteRepository.GetInstituteStudentCountAsync(instituteId);
        }

        public Task<int> GetTeacherCountAsync(Guid instituteId)
        {
            return _instituteRepository.GetInstituteTeacherCountAsync(instituteId);
        }

        public Task<int> GetClassCountAsync(Guid instituteId)
        {
            return _instituteRepository.GetInstituteClassCountAsync(instituteId);
        }
    }
}
