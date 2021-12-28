using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class userFieldsAdded_nationalCode_passHashTypeChange_passSalt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "password_hash_temp",
                table: "Users",
                nullable: true);
            migrationBuilder.Sql("Update Users SET password_hash_temp = Convert(varbinary, password_hash)");
            migrationBuilder.DropColumn(
            name: "password_hash",
            table: "Users");
            migrationBuilder.RenameColumn(
                name: "password_hash_temp",
                 table: "Users",
                 newName: "password_hash");
            migrationBuilder.AlterColumn<byte[]>(
        name: "password_hash",
        table: "Users",
        nullable: false,
        oldNullable: true);


            migrationBuilder.AddColumn<string>(
                name: "national_code",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "password_salt",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "national_code",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "password_salt",
                table: "Users");

            migrationBuilder.AddColumn<string>(
               name: "password_hash_temp",
               table: "Users",
               nullable: true);
            migrationBuilder.Sql("Update Users SET password_hash_temp = Convert(nvarchar, password_hash)");
            migrationBuilder.DropColumn(
            name: "password_hash",
            table: "Users");
            migrationBuilder.RenameColumn(
                name: "password_hash_temp",
                 table: "Users",
                 newName: "password_hash");
            migrationBuilder.AlterColumn<string>(
       name: "password_hash",
       table: "Users",
       nullable: false,
       oldNullable: true);
        }
    }
}
