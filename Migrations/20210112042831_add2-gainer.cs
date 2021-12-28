using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class add2gainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gainers",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    update_type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    primary_user_id = table.Column<long>(nullable: false),
                    secondary_user_id = table.Column<long>(nullable: false),
                    status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gainers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Gainers_Users_primary_user_id",
                        column: x => x.primary_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gainers_Users_secondary_user_id",
                        column: x => x.secondary_user_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gainers_secondary_user_id",
                table: "Gainers",
                column: "secondary_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Gainers_primary_user_id_secondary_user_id",
                table: "Gainers",
                columns: new[] { "primary_user_id", "secondary_user_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gainers");
        }
    }
}
