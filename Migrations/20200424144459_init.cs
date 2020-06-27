using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HSBors.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Funds",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(nullable: false),
                    no = table.Column<string>(nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    type = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    key = table.Column<string>(nullable: false),
                    value = table.Column<string>(nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(nullable: false),
                    first_name = table.Column<string>(nullable: false),
                    last_name = table.Column<string>(nullable: false),
                    mobile = table.Column<string>(fixedLength: true, maxLength: 11, nullable: false),
                    password_hash = table.Column<string>(nullable: false),
                    last_login = table.Column<DateTime>(nullable: true),
                    status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "UnitCosts",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    fund_id = table.Column<long>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    issue_cost = table.Column<long>(nullable: false),
                    cancel_cost = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitCosts", x => x.id);
                    table.ForeignKey(
                        name: "FK_UnitCosts_Funds_fund_id",
                        column: x => x.fund_id,
                        principalTable: "Funds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    fund_id = table.Column<long>(nullable: false),
                    accounter_id = table.Column<long>(nullable: false),
                    name = table.Column<string>(nullable: true),
                    no = table.Column<string>(nullable: true),
                    status = table.Column<string>(type: "nvarchar(50)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_accounter_id",
                        column: x => x.accounter_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Accounts_Funds_fund_id",
                        column: x => x.fund_id,
                        principalTable: "Funds",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    receiver_id = table.Column<long>(nullable: false),
                    type_id = table.Column<long>(nullable: false),
                    date = table.Column<DateTime>(nullable: false),
                    amount = table.Column<long>(nullable: false),
                    explain = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.id);
                    table.ForeignKey(
                        name: "FK_Payments_Users_receiver_id",
                        column: x => x.receiver_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Settings_type_id",
                        column: x => x.type_id,
                        principalTable: "Settings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Deposits",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    unit_cost_id = table.Column<long>(nullable: false),
                    account_id = table.Column<long>(nullable: false),
                    count = table.Column<long>(nullable: false),
                    amount = table.Column<long>(nullable: false),
                    status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deposits", x => x.id);
                    table.ForeignKey(
                        name: "FK_Deposits_Accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "Accounts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deposits_UnitCosts_unit_cost_id",
                        column: x => x.unit_cost_id,
                        principalTable: "UnitCosts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Purchases",
                columns: table => new
                {
                    creator_id = table.Column<long>(nullable: false),
                    modifier_id = table.Column<long>(nullable: true),
                    create_date = table.Column<DateTime>(nullable: false),
                    modify_date = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    buyer_id = table.Column<long>(nullable: false),
                    deposit_id = table.Column<long>(nullable: false),
                    bank_copartner_id = table.Column<long>(nullable: true),
                    amount = table.Column<long>(nullable: false),
                    bank_copartner_percent = table.Column<int>(nullable: false),
                    bank_copartner_intrest = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purchases", x => x.id);
                    table.ForeignKey(
                        name: "FK_Purchases_Users_bank_copartner_id",
                        column: x => x.bank_copartner_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_Users_buyer_id",
                        column: x => x.buyer_id,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Purchases_Deposits_deposit_id",
                        column: x => x.deposit_id,
                        principalTable: "Deposits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_accounter_id",
                table: "Accounts",
                column: "accounter_id");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_fund_id",
                table: "Accounts",
                column: "fund_id");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_account_id",
                table: "Deposits",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_Deposits_unit_cost_id",
                table: "Deposits",
                column: "unit_cost_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_date",
                table: "Payments",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_receiver_id",
                table: "Payments",
                column: "receiver_id");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_type_id",
                table: "Payments",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_bank_copartner_id",
                table: "Purchases",
                column: "bank_copartner_id");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_buyer_id",
                table: "Purchases",
                column: "buyer_id");

            migrationBuilder.CreateIndex(
                name: "IX_Purchases_deposit_id",
                table: "Purchases",
                column: "deposit_id");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_type_key",
                table: "Settings",
                columns: new[] { "type", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UnitCosts_fund_id_date",
                table: "UnitCosts",
                columns: new[] { "fund_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_username",
                table: "Users",
                column: "username",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Purchases");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Deposits");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "UnitCosts");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Funds");
        }
    }
}
