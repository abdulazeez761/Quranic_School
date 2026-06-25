using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddParentNoteReadState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "ParentNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "ParentNotes",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "ParentNotes");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "ParentNotes");
        }
    }
}
