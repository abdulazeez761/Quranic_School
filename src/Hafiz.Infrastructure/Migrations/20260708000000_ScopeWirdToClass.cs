using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ScopeWirdToClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "WirdAssignments",
                type: "uniqueidentifier",
                nullable: true);

            // Backfill existing wirds with the student's legacy single-class column so
            // per-class filtering (WirdAssignment.ClassId == currentClass) keeps returning
            // them under whichever class they were originally attached to.
            migrationBuilder.Sql(@"
                UPDATE w
                SET    w.ClassId = s.ClassId
                FROM   WirdAssignments AS w
                INNER  JOIN Students   AS s ON s.UserId = w.StudentId
                WHERE  w.ClassId IS NULL
                  AND  s.ClassId IS NOT NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_WirdAssignments_ClassId",
                table: "WirdAssignments",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_WirdAssignments_Classes_ClassId",
                table: "WirdAssignments",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WirdAssignments_Classes_ClassId",
                table: "WirdAssignments");

            migrationBuilder.DropIndex(
                name: "IX_WirdAssignments_ClassId",
                table: "WirdAssignments");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "WirdAssignments");
        }
    }
}
