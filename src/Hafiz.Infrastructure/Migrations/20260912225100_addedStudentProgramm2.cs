using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedStudentProgramm2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentRoutinePlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MemActive = table.Column<bool>(type: "bit", nullable: false),
                    MemAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MemUnit = table.Column<int>(type: "int", nullable: false),
                    MemEquivalentPages = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RecentRevActive = table.Column<bool>(type: "bit", nullable: false),
                    RecentRevAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecentRevUnit = table.Column<int>(type: "int", nullable: false),
                    OldRevActive = table.Column<bool>(type: "bit", nullable: false),
                    OldRevAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OldRevUnit = table.Column<int>(type: "int", nullable: false),
                    RecitationActive = table.Column<bool>(type: "bit", nullable: false),
                    RecitationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RecitationUnit = table.Column<int>(type: "int", nullable: false),
                    DefaultNote = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentRoutinePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentRoutinePlans_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentRoutinePlans_StudentId",
                table: "StudentRoutinePlans",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentRoutinePlans");
        }
    }
}
