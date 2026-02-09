using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hifz.DTOs;
using Hifz.Models;
using Hifz.Repositories.Interfaces;
using Hifz.Services.Interfaces;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace Hifz.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepo;

        public ClassService(
            IClassRepository classRepository,
            ITeacherRepository teacherRepository,
            IStudentRepository studentService
        )
        {
            _classRepository = classRepository;
            _teacherRepository = teacherRepository;
            _studentRepo = studentService;
        }

        public async Task CreateAsync(CreateClassDto classDto)
        {
            if (classDto is null)
                throw new ArgumentNullException(nameof(classDto));

            try
            {
                var classTeachers = new List<Teacher>();
                var classStudents = new List<Student>();
                foreach (var id in classDto.TeacherIds)
                {
                    var teacher = await _teacherRepository.GetTeacherByIDAsync(id);
                    classTeachers.Add(teacher);
                }
                foreach (var id in classDto.StudentsIds)
                {
                    var students = await _studentRepo.GetByIdAsync(id);
                    classStudents.Add(students);
                }
                Class cls = new Class()
                {
                    Name = classDto.Name,
                    Gender = classDto.Gender,
                    Students = classStudents,
                    Teachers = classTeachers,
                    ClassDays = classDto.ClassDays,
                    ClassTime = classDto.ClassTime,
                };
                await _classRepository.AddAsync(cls);
            }
            catch (Exception ex)
            {
                throw ex.GetBaseException();
            }
        }

        public async Task<List<ClassDto>> GetClassesAsync()
        {
            var classes = await _classRepository.GetAllAsync();

            var classDtos = classes
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Gender = c.Gender,
                    ClassTime = c.ClassTime,
                    ClassDays = c.ClassDays.ToList(),
                    TeacherIds = c.Teachers.Select(t => t.UserId).ToList(),
                })
                .ToList();

            return classDtos;
        }

        // if I want to make a view data will be more expencive and more hit cocuming on the data base and I wont use it more than once
        public async Task<IEnumerable<Class>> ViewClasses()
        {
            var classes = await _classRepository.GetAllAsync();

            return classes;
        }

        public async Task<bool> DeleteClass(Guid Id)
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(Id));

            return await _classRepository.Delete(Id);
        }

        public async Task<ClassDto> GetClassById(Guid id)
        {
            var classFromDb = await _classRepository.GetById(id);

            if (classFromDb == null)
                return null;

            var classDto = new ClassDto
            {
                Id = classFromDb.Id,
                Name = classFromDb.Name,
                Gender = classFromDb.Gender,
                ClassTime = classFromDb.ClassTime,
                ClassDays = classFromDb.ClassDays.ToList(), // safe fallback
                TeacherIds =
                    classFromDb.Teachers?.Select(t => t.UserId).ToList()
                    ?? new List<Guid>() // safe fallback
                ,
                StudentsIds = classFromDb.Students?.Select(t => t.UserId).ToList(),
            };

            return classDto;
        }

        public async Task<bool> UpdateAsync(ClassDto classDto)
        {
            var IsClassExist = await GetClassById(classDto.Id.Value);
            if (IsClassExist == null)
                return false;

            var students = new List<Student>();
            foreach (var studentId in classDto.StudentsIds)
            {
                var student = await _studentRepo.GetByIdAsync(studentId);

                if (student != null)
                {
                    students.Add(student);
                }
            }
            var teachers = new List<Teacher>();
            foreach (var teacherId in classDto.TeacherIds)
            {
                var teacher = await _teacherRepository.GetTeacherByIDAsync(teacherId); // it return teacher class(wrong approach)

                if (teacher != null)
                {
                    teachers.Add(teacher);
                }
            }
            Class newClass = new Class()
            {
                Id = classDto.Id.Value,
                Name = classDto.Name,
                Gender = classDto.Gender,
                ClassTime = classDto.ClassTime,
                Students = students,
                Teachers = teachers,
                ClassDays = classDto.ClassDays,
            };
            var isUpdated = await _classRepository.UpdateAsync(newClass);
            return isUpdated;
        }
    }
}
