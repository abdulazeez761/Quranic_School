using Hafiz.DTOs;
using Hafiz.Models;
using Hafiz.Models.enums;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class ClassService : IClassService
    {
        private readonly IClassRepository _classRepository;
        private readonly ITeacherRepository _teacherRepository;
        private readonly IStudentRepository _studentRepo;
        private readonly IStudyProgramRepository _studyProgramRepository;

        public ClassService(
            IClassRepository classRepository,
            ITeacherRepository teacherRepository,
            IStudentRepository studentService,
            IStudyProgramRepository studyProgramRepository
        )
        {
            _classRepository = classRepository;
            _teacherRepository = teacherRepository;
            _studentRepo = studentService;
            _studyProgramRepository = studyProgramRepository;
        }

        public async Task CreateAsync(CreateClassDto classDto, Guid? instituteId = null)
        {
            if (classDto is null)
                throw new ArgumentNullException(nameof(classDto));

            try
            {
                // 1. معالجة البرنامج التعليمي (الافتراضي قرآن)
                Guid? programId = classDto.StudyProgramId;
                if (!programId.HasValue && instituteId.HasValue)
                {
                    var defaultProg = await _studyProgramRepository.GetDefaultQuranProgramAsync(instituteId.Value);
                    programId = defaultProg?.Id;
                }

                var classTeachers = new List<Teacher>();
                var classStudents = new List<Student>();
                if (classDto.TeacherIds != null)
                {
                    foreach (var id in classDto.TeacherIds)
                    {
                        var teacher = await _teacherRepository.GetTeacherByIDAsync(id, instituteId);
                        if (teacher != null)
                            classTeachers.Add(teacher);
                    }
                }

                if (classDto.StudentsIds != null)
                {
                    foreach (var id in classDto.StudentsIds)
                    {
                        var student = await _studentRepo.GetByIdAsync(id, instituteId);
                        if (student != null)
                            classStudents.Add(student);
                    }
                }

                Class cls = new Class()
                {
                    Name = classDto.Name,
                    Gender = classDto.Gender,
                    Students = classStudents,
                    Teachers = classTeachers,
                    ClassDays = classDto.ClassDays,
                    ClassTime = classDto.ClassTime,
                    InstituteId = instituteId,
                    StudyProgramId = programId
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
                    InstituteId = c.InstituteId,
                    StudyProgramId = c.StudyProgramId,
                    StudyProgramName = c.StudyProgram?.Name,
                    MatnTitle = c.StudyProgram?.Matn?.Title,
                    ProgramType = c.StudyProgram?.Type
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

        public async Task<bool> DeleteClass(Guid Id, Guid? instituteId = null)
        {
            if (Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(Id));

            return await _classRepository.Delete(Id, instituteId);
        }

        public async Task<bool> RestoreClassAsync(Guid classId, Guid? instituteId = null)
        {
            if (classId == Guid.Empty)
                throw new ArgumentException("Id cannot be empty.", nameof(classId));

            return await _classRepository.RestoreClassAsync(classId, instituteId);
        }

        public async Task<ClassDto?> GetClassById(Guid id, Guid? instituteId = null)
        {
            var classFromDb = await _classRepository.GetById(id, instituteId);

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
                InstituteId = classFromDb.InstituteId,
                StudyProgramId = classFromDb.StudyProgramId,
                StudyProgramName = classFromDb.StudyProgram?.Name,
                MatnTitle = classFromDb.StudyProgram?.Matn?.Title,
                ProgramType = classFromDb.StudyProgram?.Type
            };

            return classDto;
        }

        public async Task<bool> UpdateAsync(ClassDto classDto)
        {
            if (!classDto.Id.HasValue)
                return false;

            var existingClass = await GetClassById(classDto.Id.Value);
            if (existingClass == null)
                return false;

            var instituteId = existingClass.InstituteId;

            var students = new List<Student>();
            if (classDto.StudentsIds != null)
            {
                foreach (var studentId in classDto.StudentsIds)
                {
                    var student = await _studentRepo.GetByIdAsync(studentId, instituteId);
                    if (student != null)
                    {
                        students.Add(student);
                    }
                }
            }

            var teachers = new List<Teacher>();
            if (classDto.TeacherIds != null)
            {
                foreach (var teacherId in classDto.TeacherIds)
                {
                    var teacher = await _teacherRepository.GetTeacherByIDAsync(teacherId, instituteId);
                    if (teacher != null)
                    {
                        teachers.Add(teacher);
                    }
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
                InstituteId = existingClass.InstituteId,
                StudyProgramId = classDto.StudyProgramId ?? existingClass.StudyProgramId
            };
            var isUpdated = await _classRepository.UpdateAsync(newClass);
            return isUpdated;
        }

        public async Task<List<ClassDto>> GetClassesByInstituteAsync(Guid instituteId)
        {
            var classes = await _classRepository.GetAllByInstituteAsync(instituteId);

            var classDtos = classes
                .Select(c => new ClassDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Gender = c.Gender,
                    ClassTime = c.ClassTime,
                    ClassDays = c.ClassDays.ToList(),
                    TeacherIds = c.Teachers.Select(t => t.UserId).ToList(),
                    InstituteId = c.InstituteId,
                    StudyProgramId = c.StudyProgramId,
                    StudyProgramName = c.StudyProgram?.Name,
                    MatnTitle = c.StudyProgram?.Matn?.Title,
                    ProgramType = c.StudyProgram?.Type
                })
                .ToList();

            return classDtos;
        }

        public async Task<IEnumerable<Class>> ViewClassesByInstitute(Guid instituteId)
        {
            return await _classRepository.GetAllByInstituteAsync(instituteId);
        }

        public Task<IEnumerable<Class>> ViewClassesByInstituteAndClassDaysAsync(
            Guid instituteId,
            DateTime workingDays
        )
        {
            ClassDaysEnum currentDay = (ClassDaysEnum)((int)workingDays.DayOfWeek + 1); // Convert DayOfWeek to ClassDaysEnum (assuming Sunday = 1, Monday = 2, ..., Saturday = 7)

            return _classRepository.GetAllByInstituteAndClassDaysAsync(instituteId, currentDay);
        }

        public Task<IEnumerable<Class>> GetArchivedClassesByInstituteAsync(Guid instituteId)
        {
            return _classRepository.GetArchivedClassesByInstituteAsync(instituteId);
        }

        public Task<IEnumerable<Class>> GetArchivedClassesAsync()
        {
            return _classRepository.GetArchivedClassesAsync();
        }

        public Task<(int activeCount, int archivedCount)> GetCountsAsync(Guid? instituteId = null)
        {
            return _classRepository.GetCountsAsync(instituteId);
        }
    }
}
