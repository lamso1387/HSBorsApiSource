using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class addconventionunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Conventions_creator_id_first_user_id_second_user_id",
                table: "Conventions",
                columns: new[] { "creator_id", "first_user_id", "second_user_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Conventions_creator_id_first_user_id_second_user_id",
                table: "Conventions");
        }
    }
}
