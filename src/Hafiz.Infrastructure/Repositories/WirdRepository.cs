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
using Microsoft.Extensions.Logging;

namespace Hafiz.Repositories
{
    public class WirdRepository : IWirdRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WirdRepository> _logger;

        public WirdRepository(ApplicationDbContext context, ILogger<WirdRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AddWirdAsync(WirdAssignment wird)
        {
            _context.WirdAssignments.Add(wird);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<(bool IsSuccess, string Message)> AddWirdsBatchAtomicAsync(
            List<WirdAssignment> wirds,
            Guid studentId,
            decimal totalMemDelta,
            decimal totalRevDelta
        )
        {
            if (wirds == null || !wirds.Any())
                return (false, "لا توجد أوراد لحفظها.");

            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    await _context.WirdAssignments.AddRangeAsync(wirds);
                    await _context.SaveChangesAsync();

                    if (totalMemDelta != 0 || totalRevDelta != 0)
                    {
                        await _context
                            .Students.Where(s => s.UserId == studentId)
                            .ExecuteUpdateAsync(setters =>
                                setters
                                    .SetProperty(
                                        s => s.MemorizedPages,
                                        s => s.MemorizedPages + totalMemDelta
                                    )
                                    .SetProperty(
                                        s => s.ReviewedPages,
                                        s => s.ReviewedPages + totalRevDelta
                                    )
                            );
                    }

                    await transaction.CommitAsync();

                    return (true, $"تم حفظ {wirds.Count} أوراد للطالب بنجاح!");
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "فشل حفظ أوراد الطالب {StudentId} دفعة واحدة بعد استنفاد المحاولات",
                    studentId
                );
                return (false, "حدث خطأ غير متوقع أثناء حفظ الأوراد. يرجى إعادة المحاولة.");
            }
        }

        public async Task<(
            WirdAssignment? memorization,
            WirdAssignment? recentRevision,
            WirdAssignment? oldRevision,
            WirdAssignment? recitation
        )> GetLatestWirdsForContextAsync(Guid studentId, DateTime todayDate)
        {
            var todayStart = todayDate.Date;

            // Fast single query to fetch recent assignments for this student
            var recentWirds = await _context
                .WirdAssignments
                .AsNoTracking()
                .Where(w => w.StudentId == studentId)
                .OrderByDescending(w => w.AssignedDate)
                .Take(40)
                .ToListAsync();

            var baseQuery = _context
                .WirdAssignments
                .AsNoTracking()
                .Where(w => w.StudentId == studentId);

            // Memorization
            var memList = recentWirds.Where(w => w.Type == AssignmentType.Memorization).ToList();
            var lastMem = memList.FirstOrDefault(w => w.AssignedDate < todayStart) ?? memList.FirstOrDefault();
            if (lastMem == null && !recentWirds.Any(w => w.Type == AssignmentType.Memorization))
            {
                var memQuery = baseQuery.Where(w => w.Type == AssignmentType.Memorization);
                lastMem = await memQuery.OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();
            }

            // Recent Revision (AmountUnit != Juz)
            var recentRevList = recentWirds.Where(w => w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz).ToList();
            var lastRecentRev = recentRevList.FirstOrDefault(w => w.AssignedDate < todayStart) ?? recentRevList.FirstOrDefault();
            if (lastRecentRev == null && !recentWirds.Any(w => w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz))
            {
                var recentRevQuery = baseQuery.Where(w => w.Type == AssignmentType.Revision && w.AmountUnit != WirdUnit.Juz);
                lastRecentRev = await recentRevQuery.OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();
            }

            // Old Revision (AmountUnit == Juz)
            var oldRevList = recentWirds.Where(w => w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz).ToList();
            var lastOldRev = oldRevList.FirstOrDefault(w => w.AssignedDate < todayStart) ?? oldRevList.FirstOrDefault();
            if (lastOldRev == null && !recentWirds.Any(w => w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz))
            {
                var oldRevQuery = baseQuery.Where(w => w.Type == AssignmentType.Revision && w.AmountUnit == WirdUnit.Juz);
                lastOldRev = await oldRevQuery.OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();
            }

            // Recitation (Tajwid)
            var tajwidList = recentWirds.Where(w => w.Type == AssignmentType.Tajwid).ToList();
            var lastRecitation = tajwidList.FirstOrDefault(w => w.AssignedDate < todayStart) ?? tajwidList.FirstOrDefault();
            if (lastRecitation == null && !recentWirds.Any(w => w.Type == AssignmentType.Tajwid))
            {
                var tajwidQuery = baseQuery.Where(w => w.Type == AssignmentType.Tajwid);
                lastRecitation = await tajwidQuery.OrderByDescending(w => w.AssignedDate).FirstOrDefaultAsync();
            }

            return (lastMem, lastRecentRev, lastOldRev, lastRecitation);
        }

        public async Task<List<WirdAssignment>> GetTodayWirdsAsync(
            Guid studentId,
            DateTime todayDate
        )
        {
            var todayStartDate = todayDate.Date;
            var tomorrowDate = todayStartDate.AddDays(1);

            return await _context
                .WirdAssignments
                .AsNoTracking()
                .Where(w =>
                    w.StudentId == studentId
                    && w.AssignedDate >= todayStartDate
                    && w.AssignedDate < tomorrowDate
                )
                .ToListAsync();
        }

        public async Task<bool> UpdateWirdAsync(WirdAssignment wird, Guid? instituteId = null)
        {
            var query = _context.WirdAssignments.Where(w => w.Id == wird.Id);
            if (instituteId.HasValue)
            {
                query = query.Where(w => w.Student.StudentInfo.InstituteId == instituteId.Value);
            }

            var updatedRowsCount = await query.ExecuteUpdateAsync(setters =>
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

        public async Task<bool> UpdateWirdWithProgressDeltaAsync(
            WirdAssignment wird,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        )
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    bool updated = await UpdateWirdAsync(wird, instituteId);
                    if (!updated)
                        return false;

                    if (memDelta != 0 || revDelta != 0)
                    {
                        await _context
                            .Students.Where(s => s.UserId == wird.StudentId)
                            .ExecuteUpdateAsync(setters =>
                                setters
                                    .SetProperty(
                                        s => s.MemorizedPages,
                                        s => s.MemorizedPages + memDelta
                                    )
                                    .SetProperty(
                                        s => s.ReviewedPages,
                                        s => s.ReviewedPages + revDelta
                                    )
                            );
                    }

                    await transaction.CommitAsync();
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل تحديث الورد {WirdId} مع صفحات الطالب ذرّياً", wird.Id);
                return false;
            }
        }

        public async Task<List<WirdAssignment>> GetWirdAssignmentsByClassIdAsync(
            Guid classID,
            DateTime fromDate,
            DateTime toDate
        )
        {
            var from = fromDate.Date;
            var toExclusive = toDate.Date.AddDays(1);

            var wirds = await FilterByClass(
                    _context
                        .WirdAssignments.IgnoreQueryFilters()
                        .AsNoTracking()
                        .Include(w => w.Student)
                        .Include(w => w.Student.StudentInfo),
                    classID
                )
                .Where(w => w.AssignedDate >= from && w.AssignedDate < toExclusive)
                .OrderByDescending(w => w.AssignedDate)
                .ToListAsync();

            return wirds;
        }

        // A wird belongs to the class it was assigned within (WirdAssignment.ClassId).
        // Legacy rows created before class-scoping fall back to student→class membership so
        // they keep appearing under the student's class(es) until backfilled/edited.
        private static IQueryable<WirdAssignment> FilterByClass(
            IQueryable<WirdAssignment> query,
            Guid classId
        ) =>
            query.Where(w =>
                w.ClassId == classId
                || (w.ClassId == null && w.Student.Classes.Any(c => c.Id == classId))
            );

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
                    ClassNames =
                        w.Class != null
                            ? new List<string> { w.Class.Name }
                            : w.Student.Classes.Select(c => c.Name).ToList(),
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
            var query = _context.WirdAssignments.IgnoreQueryFilters().AsNoTracking();

            if (filter.InstituteId.HasValue)
                query = query.Where(w => w.Student.StudentInfo.InstituteId == filter.InstituteId);

            if (filter.ClassId.HasValue)
                query = FilterByClass(query, filter.ClassId.Value);

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

        // يُضيف بيانات الطالب وحلقة الورد اللازمة لعرض أسطر التفاصيل.
        // حلقات الطالب تبقى مضمّنة كبديل للأوراد القديمة التي لا تحمل ClassId.
        private static IQueryable<WirdAssignment> WithStudentDetails(
            IQueryable<WirdAssignment> query
        ) =>
            query
                .Include(w => w.Class)
                .Include(w => w.Student)
                .ThenInclude(s => s.StudentInfo)
                .Include(w => w.Student)
                .ThenInclude(s => s.Classes);

        public async Task<WirdAssignment?> GetWirdByID(Guid Id, Guid? instituteId = null)
        {
            var query = _context
                .WirdAssignments.AsNoTracking()
                .Include(w => w.Student)
                .ThenInclude(s => s.StudentInfo)
                .Where(c => c.Id == Id);

            if (instituteId.HasValue)
            {
                query = query.Where(c => c.Student.StudentInfo.InstituteId == instituteId.Value);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateStatus(
            Guid Id,
            AssignmentStatus status,
            Guid? instituteId = null
        )
        {
            var query = _context.WirdAssignments.Where(c => c.Id == Id);
            if (instituteId.HasValue)
            {
                query = query.Where(c => c.Student.StudentInfo.InstituteId == instituteId.Value);
            }

            var assignment = await query.FirstOrDefaultAsync();
            if (assignment == null)
                return false;

            assignment.Status = status;
            assignment.IsCompleted = status != AssignmentStatus.notSet;
            if (status != AssignmentStatus.notSet)
                assignment.IsUpcoming = false;

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> UpdateStatusWithProgressDeltaAsync(
            Guid id,
            AssignmentStatus status,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        )
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    var query = _context.WirdAssignments.Where(c => c.Id == id);
                    if (instituteId.HasValue)
                    {
                        query = query.Where(c =>
                            c.Student.StudentInfo.InstituteId == instituteId.Value
                        );
                    }

                    var assignment = await query.FirstOrDefaultAsync();
                    if (assignment == null)
                        return false;

                    assignment.Status = status;
                    assignment.IsCompleted = status != AssignmentStatus.notSet;
                    if (status != AssignmentStatus.notSet)
                        assignment.IsUpcoming = false;

                    await _context.SaveChangesAsync();

                    if (memDelta != 0 || revDelta != 0)
                    {
                        await _context
                            .Students.Where(s => s.UserId == assignment.StudentId)
                            .ExecuteUpdateAsync(setters =>
                                setters
                                    .SetProperty(
                                        s => s.MemorizedPages,
                                        s => s.MemorizedPages + memDelta
                                    )
                                    .SetProperty(
                                        s => s.ReviewedPages,
                                        s => s.ReviewedPages + revDelta
                                    )
                            );
                    }

                    await transaction.CommitAsync();
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل تحديث حالة الورد {WirdId} مع صفحات الطالب ذرّياً", id);
                return false;
            }
        }

        public async Task<bool> UpdateNote(Guid Id, string note, Guid? instituteId = null)
        {
            var query = _context.WirdAssignments.Where(c => c.Id == Id);
            if (instituteId.HasValue)
            {
                query = query.Where(c => c.Student.StudentInfo.InstituteId == instituteId.Value);
            }

            var assignment = await query.FirstOrDefaultAsync();
            if (assignment == null)
                return false;

            assignment.Note = note;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteWirdAssignment(Guid id, Guid? instituteId = null)
        {
            var query = _context.WirdAssignments.Where(w => w.Id == id);
            if (instituteId.HasValue)
            {
                query = query.Where(w => w.Student.StudentInfo.InstituteId == instituteId.Value);
            }

            var wirdToDelete = await query.FirstOrDefaultAsync();
            if (wirdToDelete == null)
                return false;

            _context.WirdAssignments.Remove(wirdToDelete);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteWirdWithProgressDeltaAsync(
            Guid id,
            decimal memDelta,
            decimal revDelta,
            Guid? instituteId = null
        )
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    _context.ChangeTracker.Clear();
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    var query = _context.WirdAssignments.Where(w => w.Id == id);
                    if (instituteId.HasValue)
                    {
                        query = query.Where(w =>
                            w.Student.StudentInfo.InstituteId == instituteId.Value
                        );
                    }

                    var wirdToDelete = await query.FirstOrDefaultAsync();
                    if (wirdToDelete == null)
                        return false;

                    var studentId = wirdToDelete.StudentId;
                    _context.WirdAssignments.Remove(wirdToDelete);
                    await _context.SaveChangesAsync();

                    if (memDelta != 0 || revDelta != 0)
                    {
                        await _context
                            .Students.Where(s => s.UserId == studentId)
                            .ExecuteUpdateAsync(setters =>
                                setters
                                    .SetProperty(
                                        s => s.MemorizedPages,
                                        s => s.MemorizedPages + memDelta
                                    )
                                    .SetProperty(
                                        s => s.ReviewedPages,
                                        s => s.ReviewedPages + revDelta
                                    )
                            );
                    }

                    await transaction.CommitAsync();
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "فشل حذف الورد {WirdId} مع صفحات الطالب ذرّياً", id);
                return false;
            }
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
                .WirdAssignments.IgnoreQueryFilters()
                .Include(w => w.Student)
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
                .WirdAssignments.IgnoreQueryFilters()
                .Where(w => w.StudentId == studentID && w.IsUpcoming)
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
