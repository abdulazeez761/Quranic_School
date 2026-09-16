using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyProgramsAndMatnSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Classes_InstituteId",
                table: "Classes");

            migrationBuilder.AddColumn<Guid>(
                name: "StudyProgramId",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatnAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceType = table.Column<int>(type: "int", nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    ChapterName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FromNumber = table.Column<int>(type: "int", nullable: true),
                    ToNumber = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    IsUpcoming = table.Column<bool>(type: "bit", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatnAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatnAssignments_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatnAssignments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Author = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalVerses = table.Column<int>(type: "int", nullable: true),
                    TotalChapters = table.Column<int>(type: "int", nullable: true),
                    DefaultUnit = table.Column<int>(type: "int", nullable: false),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matns_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StudyPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    MatnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InstituteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudyPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudyPrograms_Institutes_InstituteId",
                        column: x => x.InstituteId,
                        principalTable: "Institutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudyPrograms_Matns_MatnId",
                        column: x => x.MatnId,
                        principalTable: "Matns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_InstituteId_StudyProgramId",
                table: "Classes",
                columns: new[] { "InstituteId", "StudyProgramId" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_StudyProgramId",
                table: "Classes",
                column: "StudyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_MatnAssignments_ClassId",
                table: "MatnAssignments",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MatnAssignments_Student_Class_Date",
                table: "MatnAssignments",
                columns: new[] { "StudentId", "ClassId", "AssignedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Matns_InstituteId",
                table: "Matns",
                column: "InstituteId");

            migrationBuilder.CreateIndex(
                name: "IX_StudyPrograms_InstituteId_Type",
                table: "StudyPrograms",
                columns: new[] { "InstituteId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_StudyPrograms_MatnId",
                table: "StudyPrograms",
                column: "MatnId");

            // 1. Backfill: إنشاء برنامج القرآن الافتراضي لكل معهد حالي (Default Quran Program)
            migrationBuilder.Sql(@"
                INSERT INTO StudyPrograms (Id, Name, Description, Type, InstituteId, IsActive, CreatedAt, IsDeleted)
                SELECT NEWID(), N'برنامج تحفيظ القرآن الكريم', N'البرنامج الافتراضي لحفظ ومراجعة القرآن الكريم', 1, Id, 1, GETUTCDATE(), 0
                FROM Institutes
                WHERE Id NOT IN (SELECT InstituteId FROM StudyPrograms WHERE Type = 1 AND IsDeleted = 0);
            ");

            // 2. Backfill: ربط جميع الحلقات القائمة بالبرنامج القرآني الافتراضي لمعهدها
            migrationBuilder.Sql(@"
                UPDATE c
                SET c.StudyProgramId = sp.Id
                FROM Classes c
                INNER JOIN StudyPrograms sp ON sp.InstituteId = c.InstituteId AND sp.Type = 1 AND sp.IsDeleted = 0
                WHERE c.StudyProgramId IS NULL;
            ");

            // 3. Seed: بذور مكتبة المتون العامة للنظام (Global Classical Islamic Texts)
            migrationBuilder.Sql(@"
                INSERT INTO Matns (Id, Title, Author, Category, TotalVerses, TotalChapters, DefaultUnit, InstituteId, CreatedAt, IsDeleted)
                VALUES 
                (NEWID(), N'تحفة الأطفال والغلمان في تجويد القرآن', N'سليمان الجمزوري', N'تجويد', 61, 8, 1, NULL, GETUTCDATE(), 0),
                (NEWID(), N'المقدمة الجزرية', N'محمد بن الجزري الشافعي', N'تجويد', 107, 17, 1, NULL, GETUTCDATE(), 0),
                (NEWID(), N'المنظومة البيقونية', N'عمر بن محمد البيقوني', N'مصطلح الحديث', 34, 1, 1, NULL, GETUTCDATE(), 0),
                (NEWID(), N'متن الآجرومية', N'ابن آجروم الصنهاجي', N'لغة ونحو', NULL, 26, 4, NULL, GETUTCDATE(), 0),
                (NEWID(), N'الأربعون النووية', N'يحيى بن شرف النووي', N'حديث شريف', NULL, 42, 5, NULL, GETUTCDATE(), 0),
                (NEWID(), N'العقيدة الواسطية', N'ابن تيمية الحراني', N'عقيدة إسلامية', NULL, 15, 4, NULL, GETUTCDATE(), 0);
            ");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_StudyPrograms_StudyProgramId",
                table: "Classes",
                column: "StudyProgramId",
                principalTable: "StudyPrograms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_StudyPrograms_StudyProgramId",
                table: "Classes");

            migrationBuilder.DropTable(
                name: "MatnAssignments");

            migrationBuilder.DropTable(
                name: "StudyPrograms");

            migrationBuilder.DropTable(
                name: "Matns");

            migrationBuilder.DropIndex(
                name: "IX_Classes_InstituteId_StudyProgramId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_StudyProgramId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "StudyProgramId",
                table: "Classes");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_InstituteId",
                table: "Classes",
                column: "InstituteId");
        }
    }
}
