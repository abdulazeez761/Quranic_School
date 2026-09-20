using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.Models.Matn;

namespace Hafiz.Application.Interfaces.Data;

public interface IMatnDataProvider
{
    Task<IReadOnlyList<MatnDefinition>> GetAllAsync();
    Task<MatnDefinition?> GetByIdOrNameAsync(string identifier);
}
