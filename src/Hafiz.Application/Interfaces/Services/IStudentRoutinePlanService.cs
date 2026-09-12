using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hafiz.DTOs.StudentPlan;

namespace Hafiz.Services.Interfaces
{
    public interface IStudentRoutinePlanService
    {
        /// <summary>
        /// جلب خطة الروتين الحالية لطالب محدد
        /// </summary>
        Task<StudentRoutinePlanDto?> GetPlanByStudentIdAsync(Guid studentId);

        /// <summary>
        /// جلب خطط جميع طلاب حلقة محددة (للأداء عند توزيع الأوراد)
        /// </summary>
        Task<List<StudentRoutinePlanDto>> GetPlansByClassIdAsync(Guid classId);

        /// <summary>
        /// إضافة خطة روتين جديدة لطالب
        /// </summary>
        Task<(bool Success, string Message)> AddPlanAsync(UpsertStudentRoutinePlanDto planDto);

        /// <summary>
        /// تحديث خطة الروتين الحالية لطالب
        /// </summary>
        Task<(bool Success, string Message)> UpdatePlanAsync(UpdateStudentRoutinePlanDto planDto);

        /// <summary>
        /// حفظ الخطة (Upsert): إنشاء إذا لم تكن موجودة، وتحديث إذا كانت موجودة مسبقاً
        /// </summary>
        Task<(bool Success, string Message)> SetPlanAsync(UpsertStudentRoutinePlanDto planDto);

        /// <summary>
        /// حذف الخطة المخصصة للطالب وإعادته إلى الوضع الافتراضي
        /// </summary>
        Task<(bool Success, string Message)> DeletePlanAsync(Guid studentId);
    }
}
