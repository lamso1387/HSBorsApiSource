using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace HSBors.Models
{
#pragma warning disable CS1591


    public abstract class CreationProperty
    {
        public long creator_id { get; set; }
        public long? modifier_id { get; set; }
        public DateTime create_date { get; set; }
        public DateTime? modify_date { get; set; }


    }
    public abstract class CommonProperty : CreationProperty
    {
        [Key]
        public long id { get; set; }
    }
    public class Account : CommonProperty
    {
        public long fund_id { get; set; }
        public Fund fund { get; set; }
        public long accounter_id { get; set; }
        public User accounter { get; set; }
        public ICollection<Deposit> deposits { get; set; }

        public string name { get; set; }
        public string no { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public EntityStatus status { get; set; }

        [NotMapped]
        public string fund_name { get => fund?.name; }
        [NotMapped]
        public string accounter_name { get => $"{accounter?.first_name} {accounter?.last_name}"; }

    }
    public class Deposit : CommonProperty
    {
        public long unit_cost_id { get; set; }
        public UnitCost unit_cost { get; set; }
        public long account_id { get; set; }
        public Account account { get; set; }
        public ICollection<Purchase> purchases { get; set; }

        public long count { get; set; }
        public long amount { get; set; }
        public EntityStatus status { get; set; }

        [NotMapped]
        public string account_name { get => account?.name; }
        [NotMapped]
        public string fund_name { get => account?.fund?.name; }
        [NotMapped]
        public DateTime? date { get => unit_cost?.date; }
        [NotMapped]
        public long? issue_cost { get => unit_cost?.issue_cost; }
        [NotMapped]
        public long? cancel_cost { get => unit_cost?.cancel_cost; }
        [NotMapped]
        public int? purchases_count { get => purchases?.Count; }
        [NotMapped]
        public long? fund_id { get => account?.fund?.id; }
        //dashboard:  
        [NotMapped]
        public long? final_cancel_cost { get => unit_cost?.fund?.final_cancel_cost; }
        public DateTime? final_cancel_cost_date { get => unit_cost?.fund?.final_cancel_cost_date; }
        [NotMapped, DisplayName("درصد سود")]
        public decimal? profit_percantage { get => (((decimal?)final_cancel_cost / issue_cost) - 1) * 100; }
        [NotMapped]
        public decimal? total_fund_amount { get => account?.fund?.unit_costs?.Sum(y => y.deposits.Sum(c => c.amount)); }
        [NotMapped, DisplayName("درصد سود موزون")]
        public decimal? balanced_interest_rate { get => profit_percantage * amount / total_fund_amount; }
        [NotMapped, DisplayName("ارزش روز سهم")]
        public long? value_of_shares { get => final_cancel_cost * count; }
        [NotMapped, DisplayName("میزان سود از آغاز")]
        public long? profit_rate { get => value_of_shares - (issue_cost * count); }
        [NotMapped, Description("مدت روز سرمایه گذاری")]
        public int? investment_day { get => final_cancel_cost_date.HasValue && date.HasValue ? (int?)(final_cancel_cost_date.Value - date.Value).TotalDays : null; }
        [NotMapped, Description("بازدهی روزانه سود")]
        public decimal? daily_profit_returns { get => investment_day == 0 ? 0 : profit_percantage / investment_day; }

    }
    public class Purchase : CommonProperty
    {
        public long buyer_id { get; set; }
        public User buyer { get; set; }
        public long deposit_id { get; set; }
        public Deposit deposit { get; set; }
        public long? bank_copartner_id { get; set; }
        public User bank_copartner { get; set; }
        public long? convention_id { get; set; }
        public Convention convention { get; set; }
        public long amount { get; set; }
        public int? bank_copartner_percent { get; set; }
        public int? bank_copartner_intrest { get; set; }
        [NotMapped]
        public string bank_copartner_name { get => $"{bank_copartner?.first_name} {bank_copartner?.last_name}"; }
        [NotMapped]
        public string buyer_name { get => $"{buyer?.first_name} {buyer?.last_name}"; }
        [NotMapped, Description("سود مورد انتظار بانکی")]
        public decimal? expected_bank_interest { get => (decimal?)amount * deposit?.investment_day * ((bank_copartner_percent / 12) / 30); }
    }
    public class Fund : CommonProperty
    {
        public ICollection<UnitCost> unit_costs { get; set; }
        public ICollection<Account> accounts { get; set; }

        [Required]
        public string name { get; set; }
        public string no { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public EntityStatus status { get; set; }
        //dashboard:
        [NotMapped, DisplayName("تاریخ قیمت ابطال")]
        public DateTime? final_cancel_cost_date { get => unit_costs?.Max(x => x.date); }
        [NotMapped, DisplayName("قیمت ابطال روز صندوق")]
        public long? final_cancel_cost { get => unit_costs?.Where(x => x.date == final_cancel_cost_date).First().cancel_cost; }
        [NotMapped, DisplayName("هزینه خرید واحدهای صندوق")]
        public long? total_amount { get => accounts?.Sum(x => x.deposits.Sum(c => c.amount)); }
        [NotMapped, DisplayName("درصد سود صندوق")]
        public decimal? interest_rate { get => accounts?.Sum((y => y.deposits.Sum(c => c.balanced_interest_rate))); }
    }
    public class UnitCost : CommonProperty
    {
        public long fund_id { get; set; }
        public Fund fund { get; set; }
        public ICollection<Deposit> deposits { get; set; }

        public DateTime date { get; set; }
        public long issue_cost { get; set; }
        public long cancel_cost { get; set; }

        [NotMapped]
        public string fund_name { get => fund?.name; }
    }
    public class User : CommonProperty
    {
        public ICollection<Purchase> purchases { get; set; }
        public ICollection<Purchase> bank_partnerships { get; set; }
        public ICollection<Payment> payments { get; set; }
        public ICollection<Account> accounts { get; set; }
        public ICollection<Convention> first_conventions { get; set; }
        public ICollection<Convention> second_conventions { get; set; }

        [Required]
        public string username { get; set; }
        [Required]
        public string first_name { get; set; }
        [Required]
        public string last_name { get; set; }
        [Required]
        public string mobile { get; set; }
        private string password_hash { get; set; }
        [NotMapped]
        public string password
        {
            get { return password_hash; }
            set
            {
                password_hash = SRL.Security.GetHashString(value, SRL.Security.HashAlgoritmType.Sha256);
            }
        }
        public DateTime? last_login { get; set; }
        public EntityStatus status { get; set; }
        public class UserConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                //for private propety:
                builder.Property(c => c.password_hash).IsRequired();
                //for max length
                builder.Property(e => e.mobile).HasMaxLength(11).IsFixedLength();
            }
        }
    }
    public class Convention : CommonProperty
    {
        public ICollection<Purchase> purchases { get; set; }
        public User first_user { get; set; }
        public User second_user { get; set; }
        public long first_user_id { get; set; }
        public long second_user_id { get; set; }
        public int? bank_copartner_percent { get; set; }
        public int? bank_copartner_intrest { get; set; } 
        
    }
    public class Payment : CommonProperty
    {
        public long receiver_id { get; set; }
        public User receiver { get; set; }
        public long type_id { get; set; }
        public Setting type { get; set; }

        public DateTime date { get; set; }
        public long amount { get; set; }
        public string explain { get; set; }
    }
    public class Setting : CommonProperty
    {
        public ICollection<Payment> payments { get; set; }

        [Column(TypeName = "nvarchar(50)")]
        public SettingType type { get; set; }
        [Required]
        public string key { get; set; }
        [Required]
        public string value { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public EntityStatus status { get; set; }

    }


    public class HSBorsDb : DbContext
    {
        public HSBorsDb(DbContextOptions<HSBorsDb> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(p => new { p.username }).IsUnique();
            modelBuilder.Entity<UnitCost>().HasIndex(p => new { p.fund_id, p.date }).IsUnique();
            modelBuilder.Entity<Setting>().HasIndex(p => new { p.type, p.key }).IsUnique();
            modelBuilder.Entity<Fund>().HasIndex(p => new { p.name }).IsUnique();
            modelBuilder.Entity<Payment>().HasIndex(p => new { p.date });
            // combined key: modelBuilder.Entity<ProductOrder>().HasKey(bc => new { bc.order_id, bc.product_id });
            //many to many:
            //modelBuilder.Entity<ProductOrder>().HasOne(bc => bc.order).WithMany(b => b.product_orders).HasForeignKey(bc => bc.order_id);
            //modelBuilder.Entity<ProductOrder>().HasOne(bc => bc.product).WithMany(c => c.product_orders).HasForeignKey(bc => bc.product_id);
            modelBuilder.Entity<Fund>().HasMany(c => c.unit_costs).WithOne(e => e.fund).HasForeignKey(e => e.fund_id);
            modelBuilder.Entity<Fund>().HasMany(c => c.accounts).WithOne(e => e.fund).HasForeignKey(e => e.fund_id);
            modelBuilder.Entity<UnitCost>().HasMany(c => c.deposits).WithOne(e => e.unit_cost).HasForeignKey(e => e.unit_cost_id);
            modelBuilder.Entity<User>().HasMany(c => c.purchases).WithOne(e => e.buyer).HasForeignKey(e => e.buyer_id);
            modelBuilder.Entity<User>().HasMany(c => c.bank_partnerships).WithOne(e => e.bank_copartner).HasForeignKey(e => e.bank_copartner_id);
            modelBuilder.Entity<User>().HasMany(c => c.accounts).WithOne(e => e.accounter).HasForeignKey(e => e.accounter_id);
            modelBuilder.Entity<User>().HasMany(c => c.payments).WithOne(e => e.receiver).HasForeignKey(e => e.receiver_id);
            modelBuilder.Entity<Setting>().HasMany(c => c.payments).WithOne(e => e.type).HasForeignKey(e => e.type_id);
            modelBuilder.Entity<Account>().HasMany(c => c.deposits).WithOne(e => e.account).HasForeignKey(e => e.account_id);
            modelBuilder.Entity<Convention>().HasMany(c => c.purchases).WithOne(e => e.convention).HasForeignKey(e => e.convention_id);
            modelBuilder.Entity<User>().HasMany(c => c.first_conventions).WithOne(e => e.first_user).HasForeignKey(e => e.first_user_id);
            modelBuilder.Entity<User>().HasMany(c => c.second_conventions).WithOne(e => e.second_user).HasForeignKey(e => e.second_user_id);


            modelBuilder.ApplyConfiguration(new User.UserConfiguration());

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<Deposit>().HasMany(c => c.purchases).WithOne(e => e.deposit).HasForeignKey(e => e.deposit_id).OnDelete(DeleteBehavior.Cascade);
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<UnitCost> UnitCosts { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Account> Accounts { get; set; }

    }

#pragma warning restore CS1591
}
