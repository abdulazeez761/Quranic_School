using Hafiz.Domain.Entities;
using Hafiz.DTOs;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IInstituteService
    {
        Task<List<Institute>> GetAllAsync();
        Task<Institute> GetByIdAsync(Guid id);
        Task<Institute> CreateAsync(
            string name,
            string? description,
            string? address,
            string? phoneNumber
        );
        Task<(bool Success, string ErrorMessage)> UpdateAsync(UpdateInstituteDto dto);
        Task DeleteAsync(Guid id);
        Task<(bool Success, string ErrorMessage)> CreateInstituteWithAdminAsync(
            CreateInstituteDto dto
        );
        Task<List<User>> GetInstituteAdminsAsync(Guid instituteId);
        Task<int> GetStudentCountAsync(Guid instituteId);
        Task<int> GetTeacherCountAsync(Guid instituteId);
        Task<int> GetClassCountAsync(Guid instituteId);
    }
}
