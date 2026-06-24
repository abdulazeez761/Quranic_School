using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMemorizedAndReviewedPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MemorizedPages",
                table: "Students",
                type: "decimal(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReviewedPages",
                table: "Students",
                type: "decimal(7,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "EquivalentPages",
                table: "WirdAssignments",
                type: "decimal(5,2)",
                nullable: true);

            // Backfill MemorizedPages from completed Memorization wirds.
            // AmountUnit: 0 = Pages, 1 = Ayahs, 2 = Juz.
            // AssignmentType: 1 = Memorization, 2 = Revision.
            // AssignmentStatus: 0 = notSet (anything else counts as completed).
            migrationBuilder.Sql(@"
                UPDATE s
                SET s.MemorizedPages = ISNULL(agg.TotalPages, 0)
                FROM Students s
                LEFT JOIN (
                    SELECT w.StudentId,
                           SUM(
                               CASE w.AmountUnit
                                   WHEN 0 THEN ISNULL(w.Amount, 0)
                                   WHEN 2 THEN ISNULL(w.Amount, 0) * 20
                                   WHEN 1 THEN ISNULL(w.EquivalentPages, 0)
                                   ELSE 0
                               END
                           ) AS TotalPages
                    FROM WirdAssignments w
                    WHERE w.Type = 1 AND w.Status <> 0
                    GROUP BY w.StudentId
                ) agg ON agg.StudentId = s.UserId;
            ");

            migrationBuilder.Sql(@"
                UPDATE s
                SET s.ReviewedPages = ISNULL(agg.TotalPages, 0)
                FROM Students s
                LEFT JOIN (
                    SELECT w.StudentId,
                           SUM(
                               CASE w.AmountUnit
                                   WHEN 0 THEN ISNULL(w.Amount, 0)
                                   WHEN 2 THEN ISNULL(w.Amount, 0) * 20
                                   WHEN 1 THEN ISNULL(w.EquivalentPages, 0)
                                   ELSE 0
                               END
                           ) AS TotalPages
                    FROM WirdAssignments w
                    WHERE w.Type = 2 AND w.Status <> 0
                    GROUP BY w.StudentId
                ) agg ON agg.StudentId = s.UserId;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MemorizedPages",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ReviewedPages",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EquivalentPages",
                table: "WirdAssignments");
        }
    }
}
