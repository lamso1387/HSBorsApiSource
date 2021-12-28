using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class delete_purchse_bank_3_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_bank_copartner_id",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "bank_copartner_intrest",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "bank_copartner_percent",
                table: "Purchases");

            migrationBuilder.RenameColumn(
                name: "bank_copartner_id",
                table: "Purchases",
                newName: "Userid");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_bank_copartner_id",
                table: "Purchases",
                newName: "IX_Purchases_Userid");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_Userid",
                table: "Purchases",
                column: "Userid",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Users_Userid",
                table: "Purchases");

            migrationBuilder.RenameColumn(
                name: "Userid",
                table: "Purchases",
                newName: "bank_copartner_id");

            migrationBuilder.RenameIndex(
                name: "IX_Purchases_Userid",
                table: "Purchases",
                newName: "IX_Purchases_bank_copartner_id");

            migrationBuilder.AddColumn<int>(
                name: "bank_copartner_intrest",
                table: "Purchases",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "bank_copartner_percent",
                table: "Purchases",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Users_bank_copartner_id",
                table: "Purchases",
                column: "bank_copartner_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
