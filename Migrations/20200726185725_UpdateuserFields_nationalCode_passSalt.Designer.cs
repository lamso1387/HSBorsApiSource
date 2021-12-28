﻿// <auto-generated />
using System;
using HSBors.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HSBors.Migrations
{
    [DbContext(typeof(HSBorsDb))]
    [Migration("20200726185725_UpdateuserFields_nationalCode_passSalt")]
    partial class UpdateuserFields_nationalCode_passSalt
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.14-servicing-32113")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("HSBors.Models.Account", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("accounter_id");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<long>("fund_id");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("name");

                    b.Property<string>("no");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("accounter_id");

                    b.HasIndex("fund_id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("HSBors.Models.Convention", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("bank_copartner_intrest");

                    b.Property<int?>("bank_copartner_percent");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<long>("first_user_id");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<long>("second_user_id");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("second_user_id");

                    b.HasIndex("first_user_id", "second_user_id")
                        .IsUnique();

                    b.ToTable("Conventions");
                });

            modelBuilder.Entity("HSBors.Models.Deposit", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("account_id");

                    b.Property<long>("amount");

                    b.Property<long>("count");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<int>("status");

                    b.Property<long>("unit_cost_id");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("account_id");

                    b.HasIndex("unit_cost_id");

                    b.ToTable("Deposits");
                });

            modelBuilder.Entity("HSBors.Models.Fund", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("name")
                        .IsRequired();

                    b.Property<string>("no");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("name")
                        .IsUnique();

                    b.ToTable("Funds");
                });

            modelBuilder.Entity("HSBors.Models.Payment", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("amount");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<DateTime>("date");

                    b.Property<string>("explain");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<long>("receiver_id");

                    b.Property<long>("type_id");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("date");

                    b.HasIndex("receiver_id");

                    b.HasIndex("type_id");

                    b.ToTable("Payments");
                });

            modelBuilder.Entity("HSBors.Models.Purchase", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long?>("Userid");

                    b.Property<long>("amount");

                    b.Property<long>("buyer_id");

                    b.Property<long?>("convention_id");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<long>("deposit_id");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("Userid");

                    b.HasIndex("buyer_id");

                    b.HasIndex("convention_id");

                    b.HasIndex("deposit_id");

                    b.ToTable("Purchases");
                });

            modelBuilder.Entity("HSBors.Models.Setting", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<string>("key")
                        .IsRequired();

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("status")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("value")
                        .IsRequired();

                    b.HasKey("id");

                    b.HasIndex("type", "key")
                        .IsUnique();

                    b.ToTable("Settings");
                });

            modelBuilder.Entity("HSBors.Models.UnitCost", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("cancel_cost");

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<DateTime>("date");

                    b.Property<long>("fund_id");

                    b.Property<long>("issue_cost");

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("id");

                    b.HasIndex("fund_id", "date")
                        .IsUnique();

                    b.ToTable("UnitCosts");
                });

            modelBuilder.Entity("HSBors.Models.User", b =>
                {
                    b.Property<long>("id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("create_date");

                    b.Property<long>("creator_id");

                    b.Property<string>("first_name")
                        .IsRequired();

                    b.Property<DateTime?>("last_login");

                    b.Property<string>("last_name")
                        .IsRequired();

                    b.Property<string>("mobile")
                        .IsRequired()
                        .IsFixedLength(true)
                        .HasMaxLength(11);

                    b.Property<long?>("modifier_id");

                    b.Property<DateTime?>("modify_date");

                    b.Property<string>("national_code");

                    b.Property<byte[]>("password_hash")
                        .IsRequired();

                    b.Property<byte[]>("password_salt");

                    b.Property<int>("status");

                    b.Property<string>("update_type")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("username")
                        .IsRequired();

                    b.HasKey("id");

                    b.HasIndex("username")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("HSBors.Models.Account", b =>
                {
                    b.HasOne("HSBors.Models.User", "accounter")
                        .WithMany("accounts")
                        .HasForeignKey("accounter_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.Fund", "fund")
                        .WithMany("accounts")
                        .HasForeignKey("fund_id")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("HSBors.Models.Convention", b =>
                {
                    b.HasOne("HSBors.Models.User", "first_user")
                        .WithMany("first_conventions")
                        .HasForeignKey("first_user_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.User", "second_user")
                        .WithMany("second_conventions")
                        .HasForeignKey("second_user_id")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("HSBors.Models.Deposit", b =>
                {
                    b.HasOne("HSBors.Models.Account", "account")
                        .WithMany("deposits")
                        .HasForeignKey("account_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.UnitCost", "unit_cost")
                        .WithMany("deposits")
                        .HasForeignKey("unit_cost_id")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("HSBors.Models.Payment", b =>
                {
                    b.HasOne("HSBors.Models.User", "receiver")
                        .WithMany("payments")
                        .HasForeignKey("receiver_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.Setting", "type")
                        .WithMany("payments")
                        .HasForeignKey("type_id")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("HSBors.Models.Purchase", b =>
                {
                    b.HasOne("HSBors.Models.User")
                        .WithMany("bank_partnerships")
                        .HasForeignKey("Userid")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.User", "buyer")
                        .WithMany("purchases")
                        .HasForeignKey("buyer_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.Convention", "convention")
                        .WithMany("purchases")
                        .HasForeignKey("convention_id")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("HSBors.Models.Deposit", "deposit")
                        .WithMany("purchases")
                        .HasForeignKey("deposit_id")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("HSBors.Models.UnitCost", b =>
                {
                    b.HasOne("HSBors.Models.Fund", "fund")
                        .WithMany("unit_costs")
                        .HasForeignKey("fund_id")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
