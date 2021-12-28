using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class add_update_type_column_to_all_tables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "UnitCosts",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Settings",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Purchases",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Payments",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Funds",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Deposits",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Conventions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "update_type",
                table: "Accounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "UnitCosts");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Funds");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Deposits");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Conventions");

            migrationBuilder.DropColumn(
                name: "update_type",
                table: "Accounts");
        }
    }
}
