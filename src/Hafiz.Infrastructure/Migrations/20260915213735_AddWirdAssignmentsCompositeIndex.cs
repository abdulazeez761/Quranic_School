using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWirdAssignmentsCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WirdAssignments_StudentId",
                table: "WirdAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_WirdAssignments_Student_Type_Date",
                table: "WirdAssignments",
                columns: new[] { "StudentId", "Type", "AssignedDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WirdAssignments_Student_Type_Date",
                table: "WirdAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_WirdAssignments_StudentId",
                table: "WirdAssignments",
                column: "StudentId");
        }
    }
}
