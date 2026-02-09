using System;
using Hifz.DTOs;
using Hifz.Models;

namespace Hifz.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterDto dto);
    Task<User?> LoginAsync(LoginDto dto);
}
