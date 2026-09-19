using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatnPassingGrade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PassingGrade",
                table: "Matns",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassingGrade",
                table: "Matns");
        }
    }
}
