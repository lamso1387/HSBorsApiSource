using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class nationalLenght10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "national_code",
                table: "Users",
                fixedLength: true,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "national_code",
                table: "Users",
                nullable: false,
                oldClrType: typeof(string),
                oldFixedLength: true,
                oldMaxLength: 10);
        }
    }
}
