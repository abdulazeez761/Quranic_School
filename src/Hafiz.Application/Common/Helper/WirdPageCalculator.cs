using Hafiz.Models;

namespace Hafiz.Common.Helper
{
    public static class WirdPageCalculator
    {
        public const decimal PagesPerJuz = 20m;
        public const int TotalQuranPages = 604;

        /// <summary>
        /// Converts a wird's Amount + AmountUnit into a page-equivalent value.
        /// Returns 0 when the wird carries no measurable quantity.
        /// </summary>
        public static decimal ToPages(WirdAssignment wird)
        {
            if (wird.Amount is not { } amount || wird.AmountUnit is not { } unit)
                return 0m;

            return unit switch
            {
                WirdUnit.Pages => amount,
                WirdUnit.Juz => amount * PagesPerJuz,
                WirdUnit.Ayahs => wird.EquivalentPages ?? 0m,
                _ => 0m,
            };
        }

        /// <summary>
        /// True when this wird counts toward the student's memorized progress.
        /// </summary>
        public static bool IsCompletedMemorization(WirdAssignment wird) =>
            wird.Type == AssignmentType.Memorization && wird.Status != AssignmentStatus.notSet;

        /// <summary>
        /// True when this wird counts toward the student's reviewed progress.
        /// </summary>
        public static bool IsCompletedRevision(WirdAssignment wird) =>
            wird.Type == AssignmentType.Revision && wird.Status != AssignmentStatus.notSet;

        /// <summary>
        /// Total memorized pages including the baseline juz the student joined with.
        /// </summary>
        public static decimal TotalMemorizedPages(Student student) =>
            student.MemorizedJuz * PagesPerJuz + student.MemorizedPages;

        /// <summary>
        /// Splits a total page count into (whole juz, remaining pages).
        /// </summary>
        public static (int Juz, decimal Pages) SplitJuzAndPages(decimal totalPages)
        {
            if (totalPages <= 0)
                return (0, 0m);
            int juz = (int)(totalPages / PagesPerJuz);
            decimal remainder = totalPages - juz * PagesPerJuz;
            return (juz, remainder);
        }

        public static bool IsHafiz(Student student) =>
            TotalMemorizedPages(student) >= TotalQuranPages;
    }
}
