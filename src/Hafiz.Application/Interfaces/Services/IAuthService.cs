using System;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces;

public interface IAuthService
{
    Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterDto dto);
    Task<User?> LoginAsync(LoginDto dto);
}
