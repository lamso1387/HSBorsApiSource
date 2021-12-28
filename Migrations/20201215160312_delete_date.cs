using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class delete_date : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitCosts_fund_id_date",
                table: "UnitCosts");

            migrationBuilder.DropColumn(
                name: "date",
                table: "UnitCosts");

            migrationBuilder.CreateIndex(
                name: "IX_UnitCosts_fund_id",
                table: "UnitCosts",
                column: "fund_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitCosts_fund_id",
                table: "UnitCosts");

            migrationBuilder.AddColumn<DateTime>(
                name: "date",
                table: "UnitCosts",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_UnitCosts_fund_id_date",
                table: "UnitCosts",
                columns: new[] { "fund_id", "date" },
                unique: true);
        }
    }
}
