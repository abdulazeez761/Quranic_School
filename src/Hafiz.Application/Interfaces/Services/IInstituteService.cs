using Hafiz.Domain.Entities;
using Hafiz.Models;

namespace Hafiz.Services.Interfaces
{
    public interface IInstituteService
    {
        Task<List<Institute>> GetAllAsync();
        Task<Institute> GetByIdAsync(Guid id);
        Task<Institute> CreateAsync(string name, string? description, string? address, string? phoneNumber);
        Task UpdateAsync(Institute institute);
        Task DeleteAsync(Guid id);
        Task<(bool Success, string ErrorMessage)> CreateInstituteWithAdminAsync(
            string instituteName, string? description, string? address, string? phoneNumber,
            string adminUsername, string adminFirstName, string adminSecondName,
            string adminPhoneNumber, string? adminEmail, string adminPassword);
        Task<List<User>> GetInstituteAdminsAsync(Guid instituteId);
        Task<int> GetStudentCountAsync(Guid instituteId);
        Task<int> GetTeacherCountAsync(Guid instituteId);
        Task<int> GetClassCountAsync(Guid instituteId);
    }
}
