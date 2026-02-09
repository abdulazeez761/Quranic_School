using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetUserByUsernameAndPasswordAsync(string username, string password);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
    }
}
