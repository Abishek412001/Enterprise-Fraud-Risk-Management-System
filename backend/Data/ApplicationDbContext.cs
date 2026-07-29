using EnterpriseFraudRiskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<FraudAlert> FraudAlerts => Set<FraudAlert>();
    public DbSet<CustomerRiskScore> CustomerRiskScores => Set<CustomerRiskScore>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.NationalIdNumber).IsUnique();
            e.HasMany(x => x.Accounts).WithOne(a => a.Customer).HasForeignKey(a => a.CustomerId);
            e.HasOne(x => x.RiskScore).WithOne(r => r.Customer).HasForeignKey<CustomerRiskScore>(r => r.CustomerId);
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.HasIndex(x => x.AccountNumber).IsUnique();
            e.HasMany(x => x.Cards).WithOne(c => c.Account).HasForeignKey(c => c.AccountId);
        });

        modelBuilder.Entity<Card>(e =>
        {
            e.HasIndex(x => x.CardNumberHash).IsUnique();
        });

        modelBuilder.Entity<Transaction>(e =>
        {
            e.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            e.Property(x => x.GpsLatitude).HasColumnType("decimal(9,6)");
            e.Property(x => x.GpsLongitude).HasColumnType("decimal(9,6)");
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId);
            e.HasOne(x => x.Card).WithMany().HasForeignKey(x => x.CardId);
            e.HasOne(x => x.Merchant).WithMany().HasForeignKey(x => x.MerchantId);
        });

        modelBuilder.Entity<FraudAlert>(e =>
        {
            e.HasOne(x => x.Transaction).WithMany().HasForeignKey(x => x.TransactionId);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId);
        });

        modelBuilder.Entity<CustomerRiskScore>(e =>
        {
            e.HasIndex(x => x.CustomerId).IsUnique();
        });

        modelBuilder.Entity<Account>().Property(x => x.Balance).HasColumnType("decimal(18,2)");
    }
}
