using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class addunitcostunique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "pdate",
                table: "UnitCosts",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.CreateIndex(
                name: "IX_UnitCosts_pdate_creator_id_fund_id",
                table: "UnitCosts",
                columns: new[] { "pdate", "creator_id", "fund_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitCosts_pdate_creator_id_fund_id",
                table: "UnitCosts");

            migrationBuilder.AlterColumn<string>(
                name: "pdate",
                table: "UnitCosts",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
