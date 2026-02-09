using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Common.Security;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Hifz.Services.Interfaces;

namespace Hifz.Services
{
    public class ParentService : IParentService
    {
        private readonly IParentRepository _parentRepository;
        private readonly IUserRepository _userRepository;

        public ParentService(
            IParentRepository parentRepository,
            IUserRepository userRepository
        )
        {
            _parentRepository = parentRepository;
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string ErrorMessage)> AddAsync(RegisterParentDto parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            User? isUserExist = await _userRepository.GetByUsernameAsync(parent.Username);
            if (isUserExist is not null)
                return (false, "اسم المستخدم مستخدم بالفعل");

            if (parent.Password != parent.ConfirmPassword)
                return (false, "كلمة المرور وتأكيدها غير متطابقين.");

            try
            {
                User user = new User
                {
                    Username = parent.Username,
                    FirstName = parent.FirstName,
                    SecondName = parent.SecondName,
                    Email = parent.Email,
                    PhoneNumber = parent.PhoneNumber,
                    Password = PasswordHasher.HashPassword(parent.Password),
                    Role = UserRole.Parent,
                };

                Parent parentEntity = new Parent
                {
                    UserId = user.Id,
                    ParentInfo = user
                };

                await _userRepository.AddAsync(user);
                await _parentRepository.AddAsync(parentEntity);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }

            return (true, "تم التسجيل بنجاح");
        }

        public Task<IEnumerable<Parent>> GetAllAsync()
        {
            return _parentRepository.GetAllAsync();
        }

        public async Task<ParentDto?> GetByIdAsync(Guid id)
        {
            Parent? parent = await _parentRepository.GetByIdAsync(id);
            if (parent is null)
                return null;

            ParentDto parentDto = new ParentDto
            {
                Username = parent.ParentInfo.Username,
                FirstName = parent.ParentInfo.FirstName,
                SecondName = parent.ParentInfo.SecondName,
                Email = parent.ParentInfo.Email,
                PhoneNumber = parent.ParentInfo.PhoneNumber,
                ChildrenCount = parent.Students?.Count ?? 0
            };

            return parentDto;
        }

        public Task UpdateAsync(EditParentDto parent)
        {
            if (parent is null)
                throw new ArgumentNullException(nameof(parent));

            if (!string.IsNullOrEmpty(parent.Password))
                parent.Password = PasswordHasher.HashPassword(parent.Password);

            return _parentRepository.UpdateAsync(parent);
        }

        public async Task<Parent?> GetParentByUserIdAsync(Guid userId)
        {
            return await _parentRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Student>> GetParentChildrenAsync(Guid userId)
        {
            Parent? parent = await _parentRepository.GetByUserIdAsync(userId);
            if (parent is null)
                return Enumerable.Empty<Student>();

            return parent.Students ?? Enumerable.Empty<Student>();
        }

        public Task DeleteAsync(Guid id)
        {
            return _parentRepository.DeleteAsync(id);
        }
    }
}
