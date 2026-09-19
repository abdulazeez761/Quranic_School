using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorStudyProgramMultipleMatuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add new columns to Matns and MatnAssignments first
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Matns",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Matns",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "StudyProgramId",
                table: "Matns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MatnId",
                table: "MatnAssignments",
                type: "uniqueidentifier",
                nullable: true);

            // Step 2: DATA MIGRATION - Transfer existing StudyProgram.MatnId to Matn.StudyProgramId
            migrationBuilder.Sql(@"
                UPDATE m
                SET m.StudyProgramId = sp.Id
                FROM Matns m
                INNER JOIN StudyPrograms sp ON sp.MatnId = m.Id
                WHERE sp.MatnId IS NOT NULL;
            ");

            // Ensure Order = 1 and IsActive = 1 for any pre-existing records
            migrationBuilder.Sql(@"
                UPDATE Matns
                SET [Order] = 1, [IsActive] = 1
                WHERE [Order] = 0 OR [Order] IS NULL;
            ");

            // Step 3: Create StudentMatnProgresses table
            migrationBuilder.CreateTable(
                name: "StudentMatnProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudyStatus = table.Column<int>(type: "int", nullable: false),
                    MemorizationStatus = table.Column<int>(type: "int", nullable: false),
                    ExamStatus = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TeacherNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StudyStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudyCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MemorizationCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUpdatedByTeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMatnProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentMatnProgresses_Matns_MatnId",
                        column: x => x.MatnId,
                        principalTable: "Matns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentMatnProgresses_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentMatnProgresses_Teachers_LastUpdatedByTeacherId",
                        column: x => x.LastUpdatedByTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            // Step 4: Add indexes and constraints for new relationships
            migrationBuilder.CreateIndex(
                name: "IX_Matns_StudyProgramId",
                table: "Matns",
                column: "StudyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Matns_StudyProgramId_Order",
                table: "Matns",
                columns: new[] { "StudyProgramId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_MatnAssignments_MatnId",
                table: "MatnAssignments",
                column: "MatnId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMatnProgresses_LastUpdatedByTeacherId",
                table: "StudentMatnProgresses",
                column: "LastUpdatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMatnProgresses_MatnId",
                table: "StudentMatnProgresses",
                column: "MatnId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMatnProgresses_StudentId_MatnId",
                table: "StudentMatnProgresses",
                columns: new[] { "StudentId", "MatnId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MatnAssignments_Matns_MatnId",
                table: "MatnAssignments",
                column: "MatnId",
                principalTable: "Matns",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Matns_StudyPrograms_StudyProgramId",
                table: "Matns",
                column: "StudyProgramId",
                principalTable: "StudyPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Step 5: Safely drop the old column only AFTER data has been copied
            migrationBuilder.DropForeignKey(
                name: "FK_StudyPrograms_Matns_MatnId",
                table: "StudyPrograms");

            migrationBuilder.DropIndex(
                name: "IX_StudyPrograms_MatnId",
                table: "StudyPrograms");

            migrationBuilder.DropColumn(
                name: "MatnId",
                table: "StudyPrograms");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add old MatnId column
            migrationBuilder.AddColumn<Guid>(
                name: "MatnId",
                table: "StudyPrograms",
                type: "uniqueidentifier",
                nullable: true);

            // Copy back data from Matns.StudyProgramId to StudyPrograms.MatnId
            migrationBuilder.Sql(@"
                UPDATE sp
                SET sp.MatnId = m.Id
                FROM StudyPrograms sp
                INNER JOIN Matns m ON m.StudyProgramId = sp.Id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPrograms_MatnId",
                table: "StudyPrograms",
                column: "MatnId");

            migrationBuilder.AddForeignKey(
                name: "FK_StudyPrograms_Matns_MatnId",
                table: "StudyPrograms",
                column: "MatnId",
                principalTable: "Matns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_MatnAssignments_Matns_MatnId",
                table: "MatnAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_Matns_StudyPrograms_StudyProgramId",
                table: "Matns");

            migrationBuilder.DropTable(
                name: "StudentMatnProgresses");

            migrationBuilder.DropIndex(
                name: "IX_Matns_StudyProgramId",
                table: "Matns");

            migrationBuilder.DropIndex(
                name: "IX_Matns_StudyProgramId_Order",
                table: "Matns");

            migrationBuilder.DropIndex(
                name: "IX_MatnAssignments_MatnId",
                table: "MatnAssignments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Matns");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Matns");

            migrationBuilder.DropColumn(
                name: "StudyProgramId",
                table: "Matns");

            migrationBuilder.DropColumn(
                name: "MatnId",
                table: "MatnAssignments");
        }
    }
}
