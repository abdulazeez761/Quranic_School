using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.Services.Interfaces
{
    public interface IParentNoteService
    {
        Task<IEnumerable<ParentNote>> GetNotesByStudentIdAsync(Guid studentId);
        Task<ParentNote?> GetNoteByIdAsync(Guid id);
        Task<ParentNote> CreateNoteAsync(Guid studentId, string content, Guid createdBy);
        Task<ParentNote> UpdateNoteAsync(Guid noteId, string content, Guid userId);
        Task<bool> DeleteNoteAsync(Guid noteId, Guid userId);
        Task<bool> CanUserManageNoteAsync(Guid userId, Guid noteId);
    }
}
