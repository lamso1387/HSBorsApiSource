using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class OnDeleteDepositCascadePurchases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Deposits_deposit_id",
                table: "Purchases");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Deposits_deposit_id",
                table: "Purchases",
                column: "deposit_id",
                principalTable: "Deposits",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Deposits_deposit_id",
                table: "Purchases");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Deposits_deposit_id",
                table: "Purchases",
                column: "deposit_id",
                principalTable: "Deposits",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
