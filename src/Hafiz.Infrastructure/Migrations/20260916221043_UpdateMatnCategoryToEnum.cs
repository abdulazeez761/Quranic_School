using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hafiz.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatnCategoryToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [Matns]
                SET [Category] = CASE 
                    WHEN [Category] LIKE N'%تجويد%' OR [Category] LIKE N'%قراءات%' THEN '1'
                    WHEN [Category] LIKE N'%عقيدة%' OR [Category] LIKE N'%توحيد%' THEN '2'
                    WHEN [Category] LIKE N'%حديث%' OR [Category] LIKE N'%مصطلح%' THEN '3'
                    WHEN [Category] LIKE N'%فقه%' OR [Category] LIKE N'%أصول%' THEN '4'
                    WHEN [Category] LIKE N'%نحو%' OR [Category] LIKE N'%لغة%' OR [Category] LIKE N'%إعراب%' THEN '5'
                    WHEN [Category] LIKE N'%سيرة%' OR [Category] LIKE N'%تاريخ%' THEN '6'
                    WHEN [Category] LIKE N'%آداب%' OR [Category] LIKE N'%تزكية%' THEN '7'
                    ELSE '0'
                END
                WHERE [Category] IS NOT NULL;
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "Matns",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "Matns",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
