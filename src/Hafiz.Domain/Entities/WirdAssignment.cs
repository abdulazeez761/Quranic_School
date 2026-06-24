using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hafiz.Models
{
    public class WirdAssignment
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }

        [Required(ErrorMessage = "Assignment Type is required.")]
        public AssignmentType Type { get; set; }

        // The quantity to memorize/revise (e.g. 3 pages, 20 ayahs, 1.5 juz).
        // This is the primary input when assigning a wird; the From/To range below is optional detail.
        [Column(TypeName = "decimal(4,2)")]
        [Range(0.01, 9999, ErrorMessage = "Amount must be greater than 0.")]
        public decimal? Amount { get; set; }

        public WirdUnit? AmountUnit { get; set; }

        // Page equivalent for an Ayahs-unit wird (e.g. 0.25, 0.5, 1, 2).
        // Required when AmountUnit = Ayahs so we can count progress in pages.
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.01, 999, ErrorMessage = "EquivalentPages must be greater than 0.")]
        public decimal? EquivalentPages { get; set; }

        [Range(1, 30, ErrorMessage = "FromJuz must be between 1 and 30.")]
        public int? FromJuz { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "FromPage must be positive.")]
        public int? FromPage { get; set; }

        // [Required(ErrorMessage = "FromSurah is required.")]
        public Surah? FromSurah { get; set; }

        // [Required(ErrorMessage = "FromAyah is required.")]
        [StringLength(10, ErrorMessage = "FromAyah length can't be more than 10.")]
        public string? FromAyah { get; set; }

        [Range(1, 30, ErrorMessage = "ToJuz must be between 1 and 30.")]
        public int? ToJuz { get; set; }

        // [Required(ErrorMessage = "ToSurah is required.")]
        public Surah? ToSurah { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ToAyah must be positive.")]
        public int? ToAyah { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "ToPage must be positive.")]
        public int? ToPage { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        public bool IsCompleted { get; set; }

        [StringLength(500, ErrorMessage = "Note length can't exceed 500 characters.")]
        public string? Note { get; set; }

        [Required]
        public AssignmentStatus Status { get; set; }

        public Student Student { get; set; }
    }
}
