using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class add_fundNameCreatorUniqe : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Funds",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_Funds_name_creator_id",
                table: "Funds",
                columns: new[] { "name", "creator_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Funds_name_creator_id",
                table: "Funds");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Funds",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
