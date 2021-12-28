using HSBors.Services;
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
        public long creator_id { get; set; } = UserSession.Id;
        public long? modifier_id { get; set; }
        public DateTime create_date { get; set; } = DateTime.Now;
        public DateTime? modify_date { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public UpdateType update_type { get; set; }


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
        public DateTime date { get => unit_cost.pdate.ToEnDate(); }
        [NotMapped]
        public long issue_cost { get => unit_cost.issue_cost; }
        [NotMapped]
        public long? cancel_cost { get => unit_cost?.cancel_cost; }
        [NotMapped]
        public int? purchases_count { get => purchases?.Count; }
        [NotMapped]
        public long fund_id { get => account.fund.id; }
        //dashboard:  
        [NotMapped, DisplayName("قیمت ابطال روز صندوق")]
        public long final_cancel_cost { get => unit_cost?.fund?.final_cancel_cost ?? 0; }
        [NotMapped]
        public DateTime? final_cancel_cost_date { get => unit_cost?.fund?.final_cancel_cost_date; }
        [NotMapped, DisplayName("درصد سود")]
        public decimal profit_percantage { get => (((decimal)final_cancel_cost / issue_cost) - 1) * 100; }
        [NotMapped]
        // public decimal total_fund_amount { get =>Startup.HSBorsDbContext.UnitCosts.Where(x=>x.fund_id==fund_id).Include(x=>x.deposits).ToList().Sum(y => y.deposits.Sum(c => c.amount)); }
        public decimal total_fund_amount { get => account.fund.unit_costs.Sum(y => y.deposits.Sum(c => c.amount)); }
        [NotMapped, DisplayName("درصد سود موزون")]
        public decimal balanced_interest_rate { get => profit_percantage * amount / total_fund_amount; }
        [NotMapped, DisplayName("ارزش روز سهم")]
        public long value_of_shares { get => final_cancel_cost * count; }
        [NotMapped, DisplayName("میزان سود از آغاز")]
        public long profit_rate { get => value_of_shares - (issue_cost * count); }
        [NotMapped, DisplayName("مدت روز سرمایه گذاری")]
        public int investment_day { get => (int)(final_cancel_cost_date  - date)?.TotalDays; }
        [NotMapped, DisplayName("بازدهی روزانه سود")]
        public decimal daily_profit_returns { get => investment_day == 0 ? 0 : profit_percantage / investment_day; }

    }
    public class Purchase : CommonProperty
    {
        public long buyer_id { get; set; }
        public User buyer { get; set; }
        public long deposit_id { get; set; }
        public Deposit deposit { get; set; }
        public long? convention_id { get; set; }
        public Convention convention { get; set; }
        public long amount { get; set; }
        //others:
        [NotMapped]
        public decimal deposit_ratio { get => (decimal)amount / deposit.amount; }
        [NotMapped]
        public string bank_copartner_name { get => $"{convention?.second_user?.first_name} {convention?.second_user?.last_name}"; }
        [NotMapped]
        public string buyer_name { get => $"{buyer?.first_name} {buyer?.last_name}"; }
        //dashboard:
        [NotMapped, DisplayName("سود مورد انتظار بانکی")]
        public decimal expected_bank_interest { get => amount * deposit.investment_day * ((((decimal)(convention?.bank_copartner_percent ?? 0)) / 100) / 12) / 30; }
        [NotMapped, DisplayName("ارزش روز سهم")]
        public decimal value_of_shares { get => deposit.value_of_shares * deposit_ratio; }
        [NotMapped, DisplayName("میزان سود از آغاز")]
        public decimal profit_rate { get => deposit.profit_rate * deposit_ratio; }
        [NotMapped, DisplayName("اختلاف سود خالص و مورد انتظار بانکی")]
        public decimal net_and_expected_bank_profits_diff { get => profit_rate - expected_bank_interest; }
        [NotMapped, DisplayName("سهم شریک")]
        public decimal partner_share { get => net_and_expected_bank_profits_diff * ((decimal)(convention?.bank_copartner_intrest ?? 0) / 100); }

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
        public DateTime? final_cancel_cost_date { get { if (unit_costs?.Count > 0) return unit_costs.Max(x => x.pdate.ToEnDate()); else return null; } }
        [NotMapped, DisplayName("قیمت ابطال روز صندوق")]
        public long? final_cancel_cost { get { var unit = unit_costs.Where(x => x.pdate == final_cancel_cost_date.ToPDate()); if (unit.Any()) return unit.First().cancel_cost; else return null; } }
        [NotMapped, DisplayName("هزینه خرید واحدهای صندوق")]
        public long total_amount { get => accounts.SelectMany(x => x.deposits).Sum(x => x.amount); }
        [NotMapped, DisplayName("درصد سود صندوق")]
        public decimal interest_rate { get => accounts.Sum(y => y.deposits.Sum(c => c.balanced_interest_rate)); }
    }
    public class UnitCost : CommonProperty
    {
        public long fund_id { get; set; }
        public Fund fund { get; set; }
        public ICollection<Deposit> deposits { get; set; }
        [Required]
        public string pdate { get; set; }
        public long issue_cost { get; set; }
        public long cancel_cost { get; set; }

        [NotMapped]
        public string fund_name { get => fund?.name; }
        [NotMapped] public DateTime date { get => pdate.ToEnDate(); }//=new DateTime();// 

    }
    public class User : CommonProperty
    {
        public ICollection<Purchase> purchases { get; set; }
        public ICollection<Purchase> bank_partnerships { get; set; }
        public ICollection<Payment> payments { get; set; }
        public ICollection<Account> accounts { get; set; }
        public ICollection<Convention> first_conventions { get; set; }
        public ICollection<Convention> second_conventions { get; set; }
        public ICollection<UserRole> user_roles { get; set; }
        public ICollection<Gainer> primary_gainers { get; set; }
        public ICollection<Gainer> secondary_gainers { get; set; }

        [Required]
        public string national_code { get; set; }
        [Required]
        public string first_name { get; set; }
        [Required]
        public string last_name { get; set; }
        [Required]
        public string mobile { get; set; }
        [Required]
        public byte[] password_hash { get; set; }
        [NotMapped]
        public string password { get; set; }
        [Required]
        public byte[] password_salt { get; set; }

        public DateTime? last_login { get; set; }
        public EntityStatus status { get; set; }
        [NotMapped, DisplayName("نام و نام خانوادگی")]
        public string full_name { get => $"{first_name} {last_name}"; }
        public class UserConfiguration : IEntityTypeConfiguration<User>
        {
            public void Configure(EntityTypeBuilder<User> builder)
            {
                //for private propety: builder.Property(c => c.password_hash).IsRequired();
                //for max length
                builder.Property(e => e.mobile).HasMaxLength(11).IsFixedLength();
                builder.Property(e => e.national_code).HasMaxLength(10).IsFixedLength();
            }
        }
    }

    public class Gainer : CommonProperty
    {       
        public User primary_user { get; set; }
        public User secondary_user { get; set; }
        public long primary_user_id { get; set; }
        public long secondary_user_id { get; set; }
        public EntityStatus status { get; set; }
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
    public class Role : CommonProperty
    {
        public ICollection<UserRole> user_roles { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string accesses { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public EntityStatus status { get; set; }

    }
    public class UserRole : CommonProperty
    {
        public long user_id { get; set; }
        public User user { get; set; }
        public long role_id { get; set; }
        public Role role { get; set; }

    }
    public class HSBorsDb : DbContext
    {
        public HSBorsDb(DbContextOptions<HSBorsDb> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //many to many:
            modelBuilder.Entity<UserRole>().HasOne(bc => bc.user).WithMany(b => b.user_roles).HasForeignKey(bc => bc.user_id);
            modelBuilder.Entity<UserRole>().HasOne(bc => bc.role).WithMany(c => c.user_roles).HasForeignKey(bc => bc.role_id);

            modelBuilder.Entity<Fund>().HasMany(c => c.unit_costs).WithOne(e => e.fund).HasForeignKey(e => e.fund_id);
            modelBuilder.Entity<Fund>().HasMany(c => c.accounts).WithOne(e => e.fund).HasForeignKey(e => e.fund_id);
            modelBuilder.Entity<UnitCost>().HasMany(c => c.deposits).WithOne(e => e.unit_cost).HasForeignKey(e => e.unit_cost_id);
            modelBuilder.Entity<User>().HasMany(c => c.purchases).WithOne(e => e.buyer).HasForeignKey(e => e.buyer_id);
            modelBuilder.Entity<User>().HasMany(c => c.accounts).WithOne(e => e.accounter).HasForeignKey(e => e.accounter_id);
            modelBuilder.Entity<User>().HasMany(c => c.payments).WithOne(e => e.receiver).HasForeignKey(e => e.receiver_id);
            modelBuilder.Entity<Setting>().HasMany(c => c.payments).WithOne(e => e.type).HasForeignKey(e => e.type_id);
            modelBuilder.Entity<Account>().HasMany(c => c.deposits).WithOne(e => e.account).HasForeignKey(e => e.account_id);
            modelBuilder.Entity<Convention>().HasMany(c => c.purchases).WithOne(e => e.convention).HasForeignKey(e => e.convention_id);
            modelBuilder.Entity<User>().HasMany(c => c.first_conventions).WithOne(e => e.first_user).HasForeignKey(e => e.first_user_id);
            modelBuilder.Entity<User>().HasMany(c => c.second_conventions).WithOne(e => e.second_user).HasForeignKey(e => e.second_user_id);
            modelBuilder.Entity<User>().HasMany(c => c.primary_gainers).WithOne(e => e.primary_user).HasForeignKey(e => e.primary_user_id);
            modelBuilder.Entity<User>().HasMany(c => c.secondary_gainers).WithOne(e => e.secondary_user).HasForeignKey(e => e.secondary_user_id);

            modelBuilder.Entity<User>().HasIndex(p => new { p.national_code }).IsUnique();
            modelBuilder.Entity<Setting>().HasIndex(p => new { p.type, p.key }).IsUnique();
            modelBuilder.Entity<Fund>().HasIndex(p => new { p.name, p.creator_id }).IsUnique();
            modelBuilder.Entity<Account>().HasIndex(p => new { p.name, p.creator_id,p.fund_id }).IsUnique();
            modelBuilder.Entity<Payment>().HasIndex(p => new { p.date });
            modelBuilder.Entity<Convention>().HasIndex(p => new { p.creator_id, p.first_user_id, p.second_user_id }).IsUnique();
            modelBuilder.Entity<Role>().HasIndex(p => new { p.name }).IsUnique();
            modelBuilder.Entity<UserRole>().HasIndex(bc => new { bc.user_id, bc.role_id }).IsUnique();
            modelBuilder.Entity<UnitCost>().HasIndex(p => new { p.pdate, p.creator_id, p.fund_id }).IsUnique();
            modelBuilder.Entity<Gainer>().HasIndex(p => new { p.primary_user_id, p.secondary_user_id }).IsUnique();

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
        public DbSet<Convention> Conventions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Gainer> Gainers { get; set; }

    }

#pragma warning restore CS1591
}
