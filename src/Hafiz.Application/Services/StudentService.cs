using Hafiz.Application.Interfaces;
using Hafiz.DTOs;
using Hafiz.DTOs.Wird;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Hafiz.Services.Interfaces;

namespace Hafiz.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IUserRepository _UserRepo;
        private readonly IClassService _classService;
        private readonly IWirdRepository _wirdRepository;
        private readonly IPasswordHasher _passwordHasher;

        public StudentService(
            IStudentRepository studentRepository,
            IUserRepository userRepository,
            IClassService classService,
            IWirdRepository wirdRepository,
            IPasswordHasher passwordHasher
        )
        {
            _studentRepository = studentRepository;
            _UserRepo = userRepository;
            _classService = classService;
            _wirdRepository = wirdRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<(bool Success, string ErrorMessage)> AddAsync(RegisterStudentDto student)
        {
            if (student is null)
                throw new ArgumentNullException(nameof(student));
            User? isUserExist = await _UserRepo.GetByUsernameAsync(student.Username);
            if (isUserExist is not null)
                return (false, "اسم المستخدم مستخدم بالفعل");
            if (student.Password != student.ConfirmPassword)
                return (false, "كلمة المرور وتأكيدها غير متطابقين.");

            try
            {
                User user = new User
                {
                    Username = student.Username,
                    FirstName = student.FirstName,
                    SecondName = student.SecondName,
                    Email = student.Email,
                    PhoneNumber = student.PhoneNumber,
                    Password = _passwordHasher.HashPassword(student.Password),
                    Role = student.Role,
                };
                var studentClasses = new List<Class>();
                var classesFromDb = await _classService.ViewClasses();

                foreach (var id in student.ClassesIds)
                {
                    var matchedClass = classesFromDb.FirstOrDefault(c => c.Id == id);
                    if (matchedClass != null)
                    {
                        studentClasses.Add(matchedClass);
                    }
                }

                Student studentEntity = new Student
                {
                    UserId = user.Id,
                    DateOfBirth = student.DateOfBirth,
                    ClassId = (student.ClassId == Guid.Empty) ? null : student.ClassId,
                    ParentId = (student.ParentId == Guid.Empty) ? null : student.ParentId,
                    MemorizedJuz = student.MemorizedJuz,
                    TajwidLevel = student.TajwidLevel.Value,
                    Classes = studentClasses,
                    sex = student.sex,
                };
                await _studentRepository.AddAsync(user, studentEntity);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
            return (true, "تم التسجيل بنجاح");
        }

        public Task DeleteAsync(Guid id)
        {
            return _studentRepository.DeleteAsync(id);
        }

        public async Task<StudentDto?> GetByIdAsync(Guid id)
        {
            Student? student = await _studentRepository.GetByIdAsync(id);
            if (student is null)
                return null;
            StudentDto studentDto = new StudentDto
            {
                Username = student.StudentInfo.Username,
                FirstName = student.StudentInfo.FirstName,
                SecondName = student.StudentInfo.SecondName,
                Email = student.StudentInfo.Email,
                PhoneNumber = student.StudentInfo.PhoneNumber,
                DateOfBirth = student.DateOfBirth,
                sex = student.sex,
                ClassId = student.ClassId,
                ParentId = student.ParentId,
                MemorizedJuz = student.MemorizedJuz,
                TajwidLevel = student.TajwidLevel,
                ClassesIds = student.Classes.Select(c => c.Id).ToList(),
            };
            return studentDto;
        }

        public Task<IEnumerable<Student>> GetAllAsync()
        {
            return _studentRepository.GetAllAsync();
        }

        public Task UpdateAsync(EditStudentDto student)
        {
            if (student is null)
                throw new ArgumentNullException(nameof(student));
            if (!string.IsNullOrEmpty(student.Password))
                student.Password = _passwordHasher.HashPassword(student.Password);
            return _studentRepository.UpdateAsync(student);
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassID(Guid? classID)
        {
            IEnumerable<Student>? students = await this.GetAllAsync();
            students = students.Where(s => s.Classes.Any(c => c.Id == classID)).ToList();
            return students;
        }

        // Student Portal Methods
        public async Task<Student?> GetStudentByUserIdAsync(Guid userId)
        {
            var students = await _studentRepository.GetAllAsync();
            return students.FirstOrDefault(s => s.UserId == userId);
        }

        public async Task<Student?> GetStudentByIdAsync(Guid studentId)
        {
            return await _studentRepository.GetByIdAsync(studentId);
        }

        public async Task<IEnumerable<StudentAttendance>> GetStudentAttendanceAsync(Guid studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            return student?.Attendances ?? new List<StudentAttendance>();
        }

        public async Task<IEnumerable<WirdAssignment>> GetStudentWirdsAsync(Guid studentId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            return student?.wirds ?? new List<WirdAssignment>();
        }

        public async Task<PaginatedWirdsResponse> GetStudentWirdsPaginatedAsync(
            Guid studentId,
            int pageNumber = 1,
            int pageSize = 5,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null
        )
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 5;

            var (wirds, totalCount, completedCount, pendingCount) =
                await _wirdRepository.GetWirdAssignmentsByStudentIdPaginatedAsync(
                    studentId,
                    pageNumber,
                    pageSize,
                    isCompleted,
                    assignmentType
                );

            return new PaginatedWirdsResponse(
                wirds,
                pageNumber,
                pageSize,
                totalCount,
                completedCount,
                pendingCount
            );
        }
    }
}
