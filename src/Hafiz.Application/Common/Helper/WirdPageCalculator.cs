using System;
using System.Linq.Expressions;
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
        public static decimal ToPages(WirdAssignment wird) =>
            ToPages(wird.Amount, wird.AmountUnit, wird.EquivalentPages);

        /// <summary>
        /// Page-equivalent from the raw amount fields. Lets callers that project only these
        /// columns (e.g. the report rankings) reuse the same rule without loading the entity.
        /// </summary>
        public static decimal ToPages(decimal? amount, WirdUnit? unit, decimal? equivalentPages)
        {
            if (amount is not { } value || unit is not { } wirdUnit)
                return 0m;

            return wirdUnit switch
            {
                WirdUnit.Pages => value,
                WirdUnit.Juz => value * PagesPerJuz,
                WirdUnit.Ayahs => equivalentPages ?? 0m,
                _ => 0m,
            };
        }

        /// <summary>
        /// SQL-translatable mirror of <see cref="ToPages(WirdAssignment)"/>, used to sum
        /// page-equivalents directly in the database for report aggregates.
        /// Keep in sync with the switch above.
        /// </summary>
        public static readonly Expression<Func<WirdAssignment, decimal>> ToPagesExpression =
            wird =>
                wird.Amount == null || wird.AmountUnit == null ? 0m
                : wird.AmountUnit == WirdUnit.Pages ? wird.Amount.Value
                : wird.AmountUnit == WirdUnit.Juz ? wird.Amount.Value * PagesPerJuz
                : wird.AmountUnit == WirdUnit.Ayahs ? (wird.EquivalentPages ?? 0m)
                : 0m;

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
