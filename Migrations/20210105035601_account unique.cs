using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class accountunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Accounts",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_name_creator_id_fund_id",
                table: "Accounts",
                columns: new[] { "name", "creator_id", "fund_id" },
                unique: true,
                filter: "[name] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Accounts_name_creator_id_fund_id",
                table: "Accounts");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "Accounts",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
