using HSBors.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Linq;

namespace HSBors.Migrations
{
    public partial class pdate_required : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.Sql("update unitcosts set pdate=FORMAT([date], 'yyyy/MM/dd', 'fa') ");
            migrationBuilder.AlterColumn<string>(
                name: "pdate",
                table: "UnitCosts",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);




        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "pdate",
                table: "UnitCosts",
                nullable: true,
                oldClrType: typeof(string));
        }
    }
}
