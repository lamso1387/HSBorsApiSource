using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class rmconventionunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conventions_first_user_id_second_user_id",
                table: "Conventions");

            migrationBuilder.CreateIndex(
                name: "IX_Conventions_first_user_id",
                table: "Conventions",
                column: "first_user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conventions_first_user_id",
                table: "Conventions");

            migrationBuilder.CreateIndex(
                name: "IX_Conventions_first_user_id_second_user_id",
                table: "Conventions",
                columns: new[] { "first_user_id", "second_user_id" },
                unique: true);
        }
    }
}
