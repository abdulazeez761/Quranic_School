using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class ParentNoteService : IParentNoteService
    {
        private readonly IParentNoteRepository _parentNoteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;

        public ParentNoteService(
            IParentNoteRepository parentNoteRepository,
            IUserRepository userRepository,
            IStudentRepository studentRepository
        )
        {
            _parentNoteRepository = parentNoteRepository;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
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

            var student = await _studentRepository.GetStudentBasicByIdAsync(studentId, user.InstituteId);
            if (student == null)
            {
                throw new KeyNotFoundException("Student not found or access denied.");
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
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            if (user.InstituteId.HasValue && note.Student?.StudentInfo?.InstituteId.HasValue == true &&
                note.Student.StudentInfo.InstituteId != user.InstituteId)
            {
                throw new UnauthorizedAccessException("Cross-tenant access forbidden.");
            }

            if (user.Role != UserRole.Admin && note.CreatedBy != userId)
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
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            if (user.InstituteId.HasValue && note.Student?.StudentInfo?.InstituteId.HasValue == true &&
                note.Student.StudentInfo.InstituteId != user.InstituteId)
            {
                throw new UnauthorizedAccessException("Cross-tenant access forbidden.");
            }

            if (user.Role != UserRole.Admin && note.CreatedBy != userId)
            {
                throw new UnauthorizedAccessException("You don't have permission to delete this note.");
            }

            return await _parentNoteRepository.DeleteNoteAsync(noteId);
        }

        public async Task<bool> MarkNoteAsReadAsync(Guid noteId, Guid userId)
        {
            var note = await _parentNoteRepository.GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Teacher))
            {
                throw new UnauthorizedAccessException("Only admins and teachers can mark notes as read.");
            }

            if (user.InstituteId.HasValue && note.Student?.StudentInfo?.InstituteId.HasValue == true &&
                note.Student.StudentInfo.InstituteId != user.InstituteId)
            {
                throw new UnauthorizedAccessException("Cross-tenant access forbidden.");
            }

            return await _parentNoteRepository.MarkAsReadAsync(noteId);
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

            if (user.InstituteId.HasValue && note.Student?.StudentInfo?.InstituteId.HasValue == true &&
                note.Student.StudentInfo.InstituteId != user.InstituteId)
            {
                return false;
            }

            return user.Role == UserRole.Admin || note.CreatedBy == userId;
        }
    }
}
