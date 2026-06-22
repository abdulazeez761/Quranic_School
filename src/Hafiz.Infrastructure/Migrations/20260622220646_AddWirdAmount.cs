using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWirdAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "WirdAssignments",
                type: "decimal(4,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AmountUnit",
                table: "WirdAssignments",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "WirdAssignments");

            migrationBuilder.DropColumn(
                name: "AmountUnit",
                table: "WirdAssignments");
        }
    }
}
