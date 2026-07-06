using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Data;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class ParentNoteRepository : IParentNoteRepository
    {
        private readonly ApplicationDbContext _context;

        public ParentNoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ParentNote>> GetNotesByStudentIdAsync(Guid studentId)
        {
            return await _context.ParentNotes
                .Include(pn => pn.CreatedByUser)
                .Where(pn => pn.StudentId == studentId)
                .OrderByDescending(pn => pn.CreatedAt)
                .ToListAsync();
        }

        public async Task<ParentNote?> GetNoteByIdAsync(Guid id)
        {
            return await _context.ParentNotes
                .Include(pn => pn.CreatedByUser)
                .Include(pn => pn.Student)
                .FirstOrDefaultAsync(pn => pn.Id == id);
        }

        public async Task<ParentNote> CreateNoteAsync(ParentNote note)
        {
            note.Id = Guid.NewGuid();
            note.CreatedAt = DateTime.UtcNow;
            _context.ParentNotes.Add(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<ParentNote> UpdateNoteAsync(ParentNote note)
        {
            note.UpdatedAt = DateTime.UtcNow;
            _context.ParentNotes.Update(note);
            await _context.SaveChangesAsync();
            return note;
        }

        public async Task<bool> DeleteNoteAsync(Guid id)
        {
            var note = await _context.ParentNotes.FindAsync(id);
            if (note == null)
                return false;

            _context.ParentNotes.Remove(note);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Dictionary<Guid, int>> GetUnreadNotesCountByStudentIdsAsync(
            IEnumerable<Guid> studentIds
        )
        {
            var ids = studentIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, int>();

            return await _context.ParentNotes
                .Where(pn => !pn.IsRead && ids.Contains(pn.StudentId))
                .GroupBy(pn => pn.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentId, x => x.Count);
        }

        public async Task<bool> MarkAsReadAsync(Guid id)
        {
            var note = await _context.ParentNotes.FindAsync(id);
            if (note == null)
                return false;

            if (!note.IsRead)
            {
                note.IsRead = true;
                note.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
