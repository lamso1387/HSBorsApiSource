using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class nationalCodeUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update users set national_code=cast(id as nchar(10))");
            migrationBuilder.CreateIndex(
                name: "IX_Users_national_code",
                table: "Users",
                column: "national_code",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_national_code",
                table: "Users");
        }
    }
}
