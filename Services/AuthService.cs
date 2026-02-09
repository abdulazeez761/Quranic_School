using System;
using System.Security.Cryptography;
using System.Text;
using Hifz.Common.Security;
using Hifz.Data;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Hifz.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hifz.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ITeacherRepository _teacherRepository;
    private readonly IParentRepository _parentRepository;

    public AuthService(
        IUserRepository userRepository,
        IStudentRepository studentRepository,
        ITeacherRepository teacherRepository,
        IParentRepository parentRepository
    )
    {
        _userRepository = userRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _parentRepository = parentRepository;
    }

    public async Task<User?> LoginAsync(LoginDto dto)
    {
        var hashedPassword = PasswordHasher.HashPassword(dto.Password);
        return await _userRepository.GetUserByUsernameAndPasswordAsync(dto.Username, hashedPassword);
    }

    public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepository.GetByUsernameAsync(dto.Username) is not null)
            return (false, "اسم المستخدم مستخدم بالفعل.");

        if (dto.Password != dto.ConfirmPassword)
            return (false, "كلمة المرور وتأكيدها غير متطابقين.");
        User user = new User
        {
            Username = dto.Username,
            FirstName = dto.FirstName,
            SecondName = dto.SecondName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Password = PasswordHasher.HashPassword(dto.Password),
            Role = dto.Role,
        };
        await _userRepository.AddAsync(user);

        //? adding teacher, student, parent data
        switch (dto.Role)
        {
            case UserRole.Student:

                break;

            case UserRole.Teacher:
                Teacher teacher = new Teacher { UserId = user.Id };
                await _teacherRepository.AddAsync(teacher);
                break;

            case UserRole.Parent:
                var parent = new Parent { UserId = user.Id };
                await _parentRepository.AddAsync(parent);
                break;
        }

        return (true, "تم التسجيل بنجاح");
    }
}
