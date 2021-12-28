using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class update_type_column_value_to_add : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sql = "UPDATE Accounts set update_type='add'; ";
             sql += "UPDATE Conventions set update_type='add'; ";
             sql += "UPDATE Deposits set update_type='add'; ";
             sql += "UPDATE Funds set update_type='add'; ";
             sql += "UPDATE Payments set update_type='add'; ";
            sql += "UPDATE Purchases set update_type='add'; ";
            sql += "UPDATE Settings set update_type='add'; ";
            sql += "UPDATE UnitCosts set update_type='add'; ";
            sql += "UPDATE Users set update_type='add'; ";

            migrationBuilder.Sql(sql); 
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            string sql = "UPDATE Accounts set update_type=''; ";
            sql += "UPDATE Conventions set update_type=''; ";
            sql += "UPDATE Deposits set update_type=''; ";
            sql += "UPDATE Funds set update_type=''; ";
            sql += "UPDATE Payments set update_type=''; ";
            sql += "UPDATE Purchases set update_type=''; ";
            sql += "UPDATE Settings set update_type=''; ";
            sql += "UPDATE UnitCosts set update_type=''; ";
            sql += "UPDATE Users set update_type=''; ";

            migrationBuilder.Sql(sql);

        }
    }
}
