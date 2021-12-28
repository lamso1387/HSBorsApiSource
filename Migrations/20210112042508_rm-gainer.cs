using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class rmgainer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gainers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gainers",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    create_date = table.Column<DateTime>(nullable: false),
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    modify_date = table.Column<DateTime>(nullable: true),
                    primary_gainer_id = table.Column<long>(nullable: false),
                    secondary_gainer_id = table.Column<long>(nullable: false),
                    status = table.Column<int>(nullable: false),
                    update_type = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gainers", x => x.id);
                    table.ForeignKey(
                        name: "FK_Gainers_Users_primary_gainer_id",
                        column: x => x.primary_gainer_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gainers_Users_secondary_gainer_id",
                        column: x => x.secondary_gainer_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gainers_secondary_gainer_id",
                table: "Gainers",
                column: "secondary_gainer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Gainers_primary_gainer_id_secondary_gainer_id",
                table: "Gainers",
                columns: new[] { "primary_gainer_id", "secondary_gainer_id" },
                unique: true);
        }
    }
}
