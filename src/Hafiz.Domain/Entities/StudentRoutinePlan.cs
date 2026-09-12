using System.ComponentModel.DataAnnotations;
using Hafiz.Models;

public class StudentRoutinePlan
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid StudentId { get; set; }
    public Guid? ClassId { get; set; }

    // Memorization Defaults
    public bool MemActive { get; set; } = true;
    public decimal MemAmount { get; set; } = 1.0m;
    public WirdUnit MemUnit { get; set; } = WirdUnit.Pages;
    public decimal? MemEquivalentPages { get; set; }

    // Recent Revision Defaults (Near)
    public bool RecentRevActive { get; set; } = true;
    public decimal RecentRevAmount { get; set; } = 5.0m;
    public WirdUnit RecentRevUnit { get; set; } = WirdUnit.Pages;

    // Old Revision Defaults (Far)
    public bool OldRevActive { get; set; } = true;
    public decimal OldRevAmount { get; set; } = 1.0m;
    public WirdUnit OldRevUnit { get; set; } = WirdUnit.Juz;

    // Recitation & Tajwid Defaults
    public bool RecitationActive { get; set; } = true;
    public decimal RecitationAmount { get; set; } = 2.0m;
    public WirdUnit RecitationUnit { get; set; } = WirdUnit.Pages;

    public string? DefaultNote { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Student Student { get; set; }
}
