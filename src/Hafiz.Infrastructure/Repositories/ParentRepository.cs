using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class ParentRepository : IParentRepository
    {
        private readonly ApplicationDbContext _context;

        public ParentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Parent parent)
        {
            _context.Parents.Add(parent);
            return _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Parent>> GetAllAsync()
        {
            return await _context
                .Parents.Include(p => p.ParentInfo)
                .Include(p => p.Students)
                .ToListAsync();
        }

        public async Task<Parent?> GetByIdAsync(Guid id)
        {
            return await _context
                .Parents.Include(p => p.ParentInfo)
                .Include(p => p.Students)
                .FirstOrDefaultAsync(p => p.UserId == id);
        }

        public async Task<Parent?> GetByUserIdAsync(Guid userId)
        {
            return await _context
                .Parents.Include(p => p.ParentInfo)
                .Include(p => p.Students)
                    .ThenInclude(s => s.StudentInfo)
                .Include(p => p.Students)
                    .ThenInclude(s => s.Classes)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateAsync(EditParentDto dto)
        {
            var parent = await _context
                .Parents.Include(p => p.ParentInfo)
                .FirstOrDefaultAsync(p => p.UserId == dto.ParentID);

            if (parent == null)
                throw new Exception("Parent not found");

            // Update user info
            if (!string.IsNullOrEmpty(dto.FirstName))
                parent.ParentInfo.FirstName = dto.FirstName;

            if (!string.IsNullOrEmpty(dto.SecondName))
                parent.ParentInfo.SecondName = dto.SecondName;

            if (!string.IsNullOrEmpty(dto.Username))
                parent.ParentInfo.Username = dto.Username;

            if (!string.IsNullOrEmpty(dto.PhoneNumber))
                parent.ParentInfo.PhoneNumber = dto.PhoneNumber;

            if (!string.IsNullOrEmpty(dto.Email))
                parent.ParentInfo.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.Password))
                parent.ParentInfo.Password = dto.Password; // Password should already be hashed by service

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var parent = await _context
                .Parents.Include(p => p.ParentInfo)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (parent != null)
            {
                // parent.Students.ToList().ForEach(s => s.ParentId = null);
                _context.Parents.Remove(parent);
                _context.Users.Remove(parent.ParentInfo);
                await _context.SaveChangesAsync();
            }
        }
    }
}
