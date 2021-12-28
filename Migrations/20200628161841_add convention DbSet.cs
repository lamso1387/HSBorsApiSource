using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class addconventionDbSet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Convention_Users_first_user_id",
                table: "Convention");

            migrationBuilder.DropForeignKey(
                name: "FK_Convention_Users_second_user_id",
                table: "Convention");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Convention_convention_id",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Convention",
                table: "Convention");

            migrationBuilder.RenameTable(
                name: "Convention",
                newName: "Conventions");

            migrationBuilder.RenameIndex(
                name: "IX_Convention_second_user_id",
                table: "Conventions",
                newName: "IX_Conventions_second_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Convention_first_user_id",
                table: "Conventions",
                newName: "IX_Conventions_first_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Conventions",
                table: "Conventions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Conventions_Users_first_user_id",
                table: "Conventions",
                column: "first_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Conventions_Users_second_user_id",
                table: "Conventions",
                column: "second_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Conventions_convention_id",
                table: "Purchases",
                column: "convention_id",
                principalTable: "Conventions",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conventions_Users_first_user_id",
                table: "Conventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Conventions_Users_second_user_id",
                table: "Conventions");

            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Conventions_convention_id",
                table: "Purchases");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Conventions",
                table: "Conventions");

            migrationBuilder.RenameTable(
                name: "Conventions",
                newName: "Convention");

            migrationBuilder.RenameIndex(
                name: "IX_Conventions_second_user_id",
                table: "Convention",
                newName: "IX_Convention_second_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_Conventions_first_user_id",
                table: "Convention",
                newName: "IX_Convention_first_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Convention",
                table: "Convention",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Convention_Users_first_user_id",
                table: "Convention",
                column: "first_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Convention_Users_second_user_id",
                table: "Convention",
                column: "second_user_id",
                principalTable: "Users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Convention_convention_id",
                table: "Purchases",
                column: "convention_id",
                principalTable: "Convention",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
