using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hifz.Models;

namespace Hifz.Repositories.Interfaces
{
    public interface IParentNoteRepository
    {
        Task<IEnumerable<ParentNote>> GetNotesByStudentIdAsync(Guid studentId);
        Task<ParentNote?> GetNoteByIdAsync(Guid id);
        Task<ParentNote> CreateNoteAsync(ParentNote note);
        Task<ParentNote> UpdateNoteAsync(ParentNote note);
        Task<bool> DeleteNoteAsync(Guid id);
    }
}
