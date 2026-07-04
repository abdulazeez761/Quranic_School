using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hafiz.Common.Helper;
using Hafiz.Data;
using Hafiz.DTOs.Reports;
using Hafiz.Models;
using Hafiz.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hafiz.Repositories
{
    public class WirdRepository : IWirdRepository
    {
        private readonly ApplicationDbContext _context;

        public WirdRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddWirdAsync(WirdAssignment wird)
        {
            _context.WirdAssignments.Add(wird);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateWirdAsync(WirdAssignment wird)
        {
            var updatedRowsCount = await _context
                .WirdAssignments.Where(w => w.Id == wird.Id)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(w => w.Type, wird.Type)
                        .SetProperty(w => w.Amount, wird.Amount)
                        .SetProperty(w => w.AmountUnit, wird.AmountUnit)
                        .SetProperty(w => w.EquivalentPages, wird.EquivalentPages)
                        .SetProperty(w => w.FromJuz, wird.FromJuz)
                        .SetProperty(w => w.FromPage, wird.FromPage)
                        .SetProperty(w => w.FromSurah, wird.FromSurah)
                        .SetProperty(w => w.FromAyah, wird.FromAyah)
                        .SetProperty(w => w.ToJuz, wird.ToJuz)
                        .SetProperty(w => w.ToPage, wird.ToPage)
                        .SetProperty(w => w.ToSurah, wird.ToSurah)
                        .SetProperty(w => w.ToAyah, wird.ToAyah)
                        .SetProperty(w => w.Status, wird.Status)
                        .SetProperty(w => w.IsUpcoming, wird.IsUpcoming)
                        .SetProperty(w => w.Note, wird.Note)
                );

            return updatedRowsCount > 0;
        }

        public async Task<List<WirdAssignment>> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            DateTime fromDate,
            DateTime toDate
        )
        {
            var from = fromDate.Date;
            var toExclusive = toDate.Date.AddDays(1);

            var wirds = await _context
                .WirdAssignments.Include(w => w.Student)
                .Include(w => w.Student.StudentInfo)
                .Where(wird => wird.Student.Classes.Any(cl => cl.Id == classID))
                .Where(w => w.AssignedDate >= from && w.AssignedDate < toExclusive)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();

            return wirds;
        }

        public Task<List<WirdAssignment>> GetWirdAssignmentsByStudentIdAsync(Guid studentID)
        {
            throw new NotImplementedException();
        }

        public async Task<List<WirdAssignment>> GetWirdReportDetailsPageAsync(
            WirdReportFilterDto filter
        )
        {
            return await WithStudentDetails(BuildReportQuery(filter))
                .OrderByDescending(w => w.AssignedDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();
        }

        public async Task<List<WirdAssignment>> GetWirdReportDetailsAsync(
            WirdReportFilterDto filter
        )
        {
            return await WithStudentDetails(BuildReportQuery(filter))
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();
        }

        public async Task<(
            int Total,
            int Completed,
            int Upcoming,
            decimal TotalPages,
            decimal CompletedPages
        )> GetWirdReportAggregatesAsync(WirdReportFilterDto filter)
        {
            var query = BuildReportQuery(filter);

            var total = await query.CountAsync();
            if (total == 0)
                return (0, 0, 0, 0m, 0m);

            var completed = await query.CountAsync(w => w.Status != AssignmentStatus.notSet);
            var upcoming = await query.CountAsync(w => w.IsUpcoming);

            // المجاميع تُحسب في قاعدة البيانات؛ نتجنّب SUM على مجموعة فارغة (يُرجع NULL) بالحارس.
            var totalPages = await query.SumAsync(WirdPageCalculator.ToPagesExpression);
            var completedPages =
                completed == 0
                    ? 0m
                    : await query
                        .Where(w => w.Status != AssignmentStatus.notSet)
                        .SumAsync(WirdPageCalculator.ToPagesExpression);

            return (total, completed, upcoming, totalPages, completedPages);
        }

        public async Task<List<WirdRankingSourceRow>> GetWirdReportRankingSourceAsync(
            WirdReportFilterDto filter
        )
        {
            return await BuildReportQuery(filter)
                .Select(w => new WirdRankingSourceRow
                {
                    StudentId = w.StudentId,
                    FirstName = w.Student.StudentInfo.FirstName,
                    SecondName = w.Student.StudentInfo.SecondName,
                    ClassNames = w.Student.Classes.Select(c => c.Name).ToList(),
                    IsCompleted = w.Status != AssignmentStatus.notSet,
                    Amount = w.Amount,
                    AmountUnit = w.AmountUnit,
                    EquivalentPages = w.EquivalentPages,
                })
                .ToListAsync();
        }

        // يبني استعلام التقرير بتطبيق كل الفلاتر (دون ترتيب أو تضمين) — مصدر واحد للفلترة
        // تشترك فيه صفحة التفاصيل والإحصائيات والترتيب.
        private IQueryable<WirdAssignment> BuildReportQuery(WirdReportFilterDto filter)
        {
            var query = _context.WirdAssignments.AsNoTracking();

            if (filter.InstituteId.HasValue)
                query = query.Where(w => w.Student.StudentInfo.InstituteId == filter.InstituteId);

            if (filter.ClassId.HasValue)
                query = query.Where(w => w.Student.Classes.Any(c => c.Id == filter.ClassId));

            if (filter.StudentId.HasValue)
                query = query.Where(w => w.StudentId == filter.StudentId);

            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(w => w.AssignedDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var toExclusive = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(w => w.AssignedDate < toExclusive);
            }

            if (filter.Type.HasValue)
                query = query.Where(w => w.Type == filter.Type);

            if (filter.IsCompleted.HasValue)
            {
                query = filter.IsCompleted.Value
                    ? query.Where(w => w.Status != AssignmentStatus.notSet)
                    : query.Where(w => w.Status == AssignmentStatus.notSet);
            }

            return query;
        }

        // يُضيف بيانات الطالب وشُعبه اللازمة لعرض أسطر التفاصيل.
        private static IQueryable<WirdAssignment> WithStudentDetails(
            IQueryable<WirdAssignment> query
        ) =>
            query
                .Include(w => w.Student)
                .ThenInclude(s => s.StudentInfo)
                .Include(w => w.Student)
                .ThenInclude(s => s.Classes);

        public async Task<WirdAssignment?> GetWirdByID(Guid Id)
        {
            // No tracking: callers read this for display or recompute then persist via a
            // separate ExecuteUpdate/own query. Tracking here would make a re-fetch after
            // ExecuteUpdateAsync return the stale cached entity instead of fresh DB values.
            return await _context
                .WirdAssignments.AsNoTracking()
                .Include(w => w.Student)
                .ThenInclude(s => s.StudentInfo)
                .Where(c => c.Id == Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatus(Guid Id, AssignmentStatus status)
        {
            var assignment = _context.WirdAssignments.Where(c => c.Id == Id).FirstOrDefault();
            assignment!.Status = status;
            // Grading a wird means it's no longer an upcoming (future) assignment.
            if (status != AssignmentStatus.notSet)
                assignment.IsUpcoming = false;
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> UpdateNote(Guid Id, string note)
        {
            var assignment = _context.WirdAssignments.Where(c => c.Id == Id).FirstOrDefault();
            assignment!.Note = note;
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<bool> DeleteWirdAssignment(Guid id)
        {
            WirdAssignment? wirdToDelete = _context.WirdAssignments.FirstOrDefault(w => w.Id == id);
            _context.WirdAssignments.Remove(wirdToDelete!);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<(
            List<WirdAssignment> wirds,
            int totalCount,
            int completedCount,
            int pendingCount,
            int upcomingCount
        )> GetWirdAssignmentsByStudentIdPaginatedAsync(
            Guid studentID,
            int pageNumber,
            int pageSize,
            bool? isCompleted = null,
            AssignmentType? assignmentType = null,
            bool? isUpcoming = null
        )
        {
            var query = _context
                .WirdAssignments.Include(w => w.Student)
                .Include(w => w.Student.StudentInfo)
                .Where(w => w.StudentId == studentID);

            if (isUpcoming.HasValue)
            {
                query = query.Where(w => w.IsUpcoming == isUpcoming.Value);
            }

            if (isCompleted.HasValue)
            {
                query = isCompleted.Value
                    ? query.Where(w => w.Status != AssignmentStatus.notSet)
                    : query.Where(w => w.Status == AssignmentStatus.notSet);
            }

            if (assignmentType.HasValue)
            {
                query = query.Where(w => w.Type == assignmentType.Value);
            }

            var totalCount = await query.CountAsync();
            var completedCount = await query
                .Where(w => w.Status != AssignmentStatus.notSet)
                .CountAsync();
            var pendingCount = totalCount - completedCount;
            // Upcoming count is always over the whole student so the summary stays
            // meaningful even while the list is filtered by status/type.
            var upcomingCount = await _context
                .WirdAssignments.Where(w => w.StudentId == studentID && w.IsUpcoming)
                .CountAsync();

            var wirds = await query
                .OrderByDescending(w => w.AssignedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (wirds, totalCount, completedCount, pendingCount, upcomingCount);
        }
    }
}
