using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class ParentNoteService : IParentNoteService
    {
        private readonly IParentNoteRepository _parentNoteRepository;
        private readonly IUserRepository _userRepository;

        public ParentNoteService(
            IParentNoteRepository parentNoteRepository,
            IUserRepository userRepository
        )
        {
            _parentNoteRepository = parentNoteRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<ParentNote>> GetNotesByStudentIdAsync(Guid studentId)
        {
            return await _parentNoteRepository.GetNotesByStudentIdAsync(studentId);
        }

        public async Task<ParentNote?> GetNoteByIdAsync(Guid id)
        {
            return await _parentNoteRepository.GetNoteByIdAsync(id);
        }

        public async Task<ParentNote> CreateNoteAsync(Guid studentId, string content, Guid createdBy)
        {
            var user = await _userRepository.GetByIdAsync(createdBy);
            if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Teacher))
            {
                throw new UnauthorizedAccessException("Only admins and teachers can create parent notes.");
            }

            var note = new ParentNote
            {
                StudentId = studentId,
                Content = content,
                CreatedBy = createdBy
            };

            return await _parentNoteRepository.CreateNoteAsync(note);
        }

        public async Task<ParentNote> UpdateNoteAsync(Guid noteId, string content, Guid userId)
        {
            var note = await _parentNoteRepository.GetNoteByIdAsync(noteId);
            if (note == null)
            {
                throw new KeyNotFoundException("Note not found.");
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && note.CreatedBy != userId))
            {
                throw new UnauthorizedAccessException("You don't have permission to update this note.");
            }

            note.Content = content;
            return await _parentNoteRepository.UpdateNoteAsync(note);
        }

        public async Task<bool> DeleteNoteAsync(Guid noteId, Guid userId)
        {
            var note = await _parentNoteRepository.GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && note.CreatedBy != userId))
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this note.");
            }

            return await _parentNoteRepository.DeleteNoteAsync(noteId);
        }

        public async Task<bool> CanUserManageNoteAsync(Guid userId, Guid noteId)
        {
            var note = await _parentNoteRepository.GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            return user.Role == UserRole.Admin || note.CreatedBy == userId;
        }
    }
}
