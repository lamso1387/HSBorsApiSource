using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class addconvention : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "convention_id",
                table: "Purchases",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Convention",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    first_user_id = table.Column<long>(nullable: false),
                    second_user_id = table.Column<long>(nullable: false),
                    bank_copartner_percent = table.Column<int>(nullable: true),
                    bank_copartner_intrest = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Convention", x => x.id);
                    table.ForeignKey(
                        name: "FK_Convention_Users_first_user_id",
                        column: x => x.first_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Convention_Users_second_user_id",
                        column: x => x.second_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_convention_id",
                table: "Purchases",
                column: "convention_id");

            migrationBuilder.CreateIndex(
                name: "IX_Convention_first_user_id",
                table: "Convention",
                column: "first_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Convention_second_user_id",
                table: "Convention",
                column: "second_user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Purchases_Convention_convention_id",
                table: "Purchases",
                column: "convention_id",
                principalTable: "Convention",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Purchases_Convention_convention_id",
                table: "Purchases");

            migrationBuilder.DropTable(
                name: "Convention");

            migrationBuilder.DropIndex(
                name: "IX_Purchases_convention_id",
                table: "Purchases");

            migrationBuilder.DropColumn(
                name: "convention_id",
                table: "Purchases");
        }
    }
}
