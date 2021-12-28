using HSBors.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Security.Cryptography.X509Certificates;

namespace HSBors.Migrations
{
    public partial class add_pdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "pdate",
                table: "UnitCosts",
                nullable: true);
             
            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pdate",
                table: "UnitCosts");
        }
    }
}
