using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Application.Interfaces.Repositories;
using Hafiz.DTOs.StudentPlan;
using Hafiz.Services.Interfaces;

namespace Hafiz.Application.Services
{
    public class StudentRoutinePlanService : IStudentRoutinePlanService
    {
        private readonly IStudentRoutinePlanRepository _studentRoutinePlanRepository;

        public StudentRoutinePlanService(IStudentRoutinePlanRepository studentRoutinePlanRepository)
        {
            _studentRoutinePlanRepository = studentRoutinePlanRepository;
        }

        public async Task<StudentRoutinePlanDto?> GetPlanByStudentIdAsync(Guid studentId)
        {
            var entity = await _studentRoutinePlanRepository.GetPlanByStudentIdAsync(studentId);
            if (entity == null)
                return null;

            return MapToDto(entity);
        }

        public async Task<List<StudentRoutinePlanDto>> GetPlansByClassIdAsync(Guid classId)
        {
            var entities = await _studentRoutinePlanRepository.GetPlansByClassIdAsync(classId);
            return entities.Select(MapToDto).ToList();
        }

        public async Task<(bool Success, string Message)> AddPlanAsync(UpsertStudentRoutinePlanDto planDto)
        {
            if (planDto == null)
                return (false, "بيانات الخطة غير صحيحة");

            var exists = await _studentRoutinePlanRepository.ExistsByStudentIdAsync(planDto.StudentId);
            if (exists)
                return (false, "توجد خطة مسجلة مسبقاً لهذا الطالب");

            var entity = new StudentRoutinePlan
            {
                Id = Guid.NewGuid(),
                StudentId = planDto.StudentId,
                ClassId = planDto.ClassId,
                MemActive = planDto.MemActive,
                MemAmount = planDto.MemAmount,
                MemUnit = planDto.MemUnit,
                MemEquivalentPages = planDto.MemEquivalentPages,
                RecentRevActive = planDto.RecentRevActive,
                RecentRevAmount = planDto.RecentRevAmount,
                RecentRevUnit = planDto.RecentRevUnit,
                OldRevActive = planDto.OldRevActive,
                OldRevAmount = planDto.OldRevAmount,
                OldRevUnit = planDto.OldRevUnit,
                RecitationActive = planDto.RecitationActive,
                RecitationAmount = planDto.RecitationAmount,
                RecitationUnit = planDto.RecitationUnit,
                DefaultNote = planDto.DefaultNote,
                UpdatedAt = DateTime.UtcNow
            };

            await _studentRoutinePlanRepository.AddAsync(entity);
            return (true, "تم إضافة خطة الطالب بنجاح");
        }

        public async Task<(bool Success, string Message)> UpdatePlanAsync(UpdateStudentRoutinePlanDto planDto)
        {
            if (planDto == null)
                return (false, "بيانات الخطة غير صحيحة");

            var existing = await _studentRoutinePlanRepository.GetPlanByStudentIdAsync(planDto.StudentId);
            if (existing == null)
                return (false, "الخطة غير موجودة للتعديل");

            existing.ClassId = planDto.ClassId ?? existing.ClassId;
            existing.MemActive = planDto.MemActive;
            existing.MemAmount = planDto.MemAmount;
            existing.MemUnit = planDto.MemUnit;
            existing.MemEquivalentPages = planDto.MemEquivalentPages;
            existing.RecentRevActive = planDto.RecentRevActive;
            existing.RecentRevAmount = planDto.RecentRevAmount;
            existing.RecentRevUnit = planDto.RecentRevUnit;
            existing.OldRevActive = planDto.OldRevActive;
            existing.OldRevAmount = planDto.OldRevAmount;
            existing.OldRevUnit = planDto.OldRevUnit;
            existing.RecitationActive = planDto.RecitationActive;
            existing.RecitationAmount = planDto.RecitationAmount;
            existing.RecitationUnit = planDto.RecitationUnit;
            existing.DefaultNote = planDto.DefaultNote;
            existing.UpdatedAt = DateTime.UtcNow;

            await _studentRoutinePlanRepository.UpdateAsync(existing);
            return (true, "تم تحديث خطة الطالب بنجاح");
        }

        public async Task<(bool Success, string Message)> SetPlanAsync(UpsertStudentRoutinePlanDto planDto)
        {
            if (planDto == null)
                return (false, "بيانات الخطة غير صالحة");

            var existing = await _studentRoutinePlanRepository.GetPlanByStudentIdAsync(planDto.StudentId);
            if (existing == null)
            {
                var newEntity = new StudentRoutinePlan
                {
                    Id = Guid.NewGuid(),
                    StudentId = planDto.StudentId,
                    ClassId = planDto.ClassId,
                    MemActive = planDto.MemActive,
                    MemAmount = planDto.MemAmount,
                    MemUnit = planDto.MemUnit,
                    MemEquivalentPages = planDto.MemEquivalentPages,
                    RecentRevActive = planDto.RecentRevActive,
                    RecentRevAmount = planDto.RecentRevAmount,
                    RecentRevUnit = planDto.RecentRevUnit,
                    OldRevActive = planDto.OldRevActive,
                    OldRevAmount = planDto.OldRevAmount,
                    OldRevUnit = planDto.OldRevUnit,
                    RecitationActive = planDto.RecitationActive,
                    RecitationAmount = planDto.RecitationAmount,
                    RecitationUnit = planDto.RecitationUnit,
                    DefaultNote = planDto.DefaultNote,
                    UpdatedAt = DateTime.UtcNow
                };

                await _studentRoutinePlanRepository.AddAsync(newEntity);
                return (true, "تم حفظ خطة الطالب بنجاح");
            }
            else
            {
                existing.ClassId = planDto.ClassId ?? existing.ClassId;
                existing.MemActive = planDto.MemActive;
                existing.MemAmount = planDto.MemAmount;
                existing.MemUnit = planDto.MemUnit;
                existing.MemEquivalentPages = planDto.MemEquivalentPages;
                existing.RecentRevActive = planDto.RecentRevActive;
                existing.RecentRevAmount = planDto.RecentRevAmount;
                existing.RecentRevUnit = planDto.RecentRevUnit;
                existing.OldRevActive = planDto.OldRevActive;
                existing.OldRevAmount = planDto.OldRevAmount;
                existing.OldRevUnit = planDto.OldRevUnit;
                existing.RecitationActive = planDto.RecitationActive;
                existing.RecitationAmount = planDto.RecitationAmount;
                existing.RecitationUnit = planDto.RecitationUnit;
                existing.DefaultNote = planDto.DefaultNote;
                existing.UpdatedAt = DateTime.UtcNow;

                await _studentRoutinePlanRepository.UpdateAsync(existing);
                return (true, "تم تحديث خطة الطالب بنجاح");
            }
        }

        public async Task<(bool Success, string Message)> DeletePlanAsync(Guid studentId)
        {
            var existing = await _studentRoutinePlanRepository.GetPlanByStudentIdAsync(studentId);
            if (existing == null)
                return (true, "الطالب يعمل بالخطة الافتراضية العامة");

            await _studentRoutinePlanRepository.DeleteAsync(existing);
            return (true, "تم إعادة تعيين الخطة للوضع الافتراضي بنجاح");
        }

        private static StudentRoutinePlanDto MapToDto(StudentRoutinePlan entity)
        {
            return new StudentRoutinePlanDto
            {
                Id = entity.Id,
                StudentId = entity.StudentId,
                ClassId = entity.ClassId,
                MemActive = entity.MemActive,
                MemAmount = entity.MemAmount,
                MemUnit = entity.MemUnit,
                MemEquivalentPages = entity.MemEquivalentPages,
                RecentRevActive = entity.RecentRevActive,
                RecentRevAmount = entity.RecentRevAmount,
                RecentRevUnit = entity.RecentRevUnit,
                OldRevActive = entity.OldRevActive,
                OldRevAmount = entity.OldRevAmount,
                OldRevUnit = entity.OldRevUnit,
                RecitationActive = entity.RecitationActive,
                RecitationAmount = entity.RecitationAmount,
                RecitationUnit = entity.RecitationUnit,
                DefaultNote = entity.DefaultNote,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
