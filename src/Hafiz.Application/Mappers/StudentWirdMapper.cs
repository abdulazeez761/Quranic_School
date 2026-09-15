using System;
using Hafiz.Application.DTO.StudentWird;
using Hafiz.Models;

namespace Hafiz.Application.Mappers
{
    public static class StudentWirdMapper
    {
        public static StudentSummaryDto MapStudentSummary(Student student)
        {
            var firstName = student.StudentInfo?.FirstName ?? string.Empty;
            var secondName = student.StudentInfo?.SecondName ?? string.Empty;
            var fullName = $"{firstName} {secondName}".Trim();
            var initials = (
                (firstName.Length > 0 ? firstName[0].ToString() : string.Empty) +
                (secondName.Length > 0 ? secondName[0].ToString() : string.Empty)
            ).ToUpper();

            return new StudentSummaryDto
            {
                Id = student.UserId,
                Name = fullName,
                Initials = initials,
                Level = student.TajwidLevel.ToString()
            };
        }

        public static LastWirdItemDto? MapLastWirdItem(WirdAssignment? wird)
        {
            if (wird == null) return null;

            return new LastWirdItemDto
            {
                FromSurah = (int?)wird.FromSurah,
                FromAyah = wird.FromAyah,
                ToSurah = (int?)wird.ToSurah,
                ToAyah = wird.ToAyah,
                Amount = wird.Amount,
                AmountUnit = (int?)wird.AmountUnit,
                AssignedDate = wird.AssignedDate.ToString("yyyy-MM-dd")
            };
        }

        public static TodayWirdItemDto? MapTodayWirdItem(WirdAssignment? wird)
        {
            if (wird == null) return null;

            return new TodayWirdItemDto
            {
                Id = wird.Id,
                Amount = wird.Amount,
                AmountUnit = (int?)wird.AmountUnit,
                EquivalentPages = wird.EquivalentPages,
                FromSurah = (int?)wird.FromSurah,
                FromAyah = wird.FromAyah,
                ToSurah = (int?)wird.ToSurah,
                ToAyah = wird.ToAyah,
                Status = (int)wird.Status,
                Note = wird.Note,
                IsUpcoming = wird.IsUpcoming,
                IsCompleted = wird.IsCompleted
            };
        }
    }
}
