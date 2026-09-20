using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.Models.Matn;

namespace Hafiz.Application.Interfaces.Services;

public interface IMatnReferenceService
{
    Task<IReadOnlyList<MatnDefinition>> GetAllAsync();
    Task<MatnDefinition?> GetByIdOrNameAsync(string identifier);
}
