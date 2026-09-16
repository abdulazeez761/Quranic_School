using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.DTOs.Matn;

namespace Hafiz.Services.Interfaces;

public interface IMatnService
{
    Task<MatnDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<MatnDto>> GetAllAvailableAsync(Guid? instituteId = null, Hafiz.Domain.Enums.MatnCategory? category = null);
    Task<(bool Success, string Message, Guid? Id)> CreateAsync(CreateMatnDto dto, Guid? instituteId = null);
    Task<(bool Success, string Message)> UpdateAsync(Guid id, CreateMatnDto dto, Guid? instituteId = null);
    Task<(bool Success, string Message)> DeleteAsync(Guid id, Guid? instituteId = null);
}
