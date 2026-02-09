using System;
using System.Collections.Generic;
using Hifz.Models;
using StudentModel = Hifz.Models.Student;

namespace Hifz.DTOs.Student
{
    public class StudentDetailsViewModel
    {
        public StudentModel Student { get; set; } = new();
        public List<WirdAssignment> PaginatedWirds { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalWirds { get; set; }
        public int PageSize { get; set; } = 10;

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;
    }
}
