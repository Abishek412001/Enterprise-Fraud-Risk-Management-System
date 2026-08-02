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

    public DbSet<FRMAlert> FRMAlerts => Set<FRMAlert>();
    public DbSet<AlertAssignment> AlertAssignments => Set<AlertAssignment>();
    public DbSet<AlertHistory> AlertHistories => Set<AlertHistory>();
    public DbSet<AlertComment> AlertComments => Set<AlertComment>();

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<CustomerSession> CustomerSessions => Set<CustomerSession>();
    public DbSet<ATOAlert> ATOAlerts => Set<ATOAlert>();

    public DbSet<SentinelAlert> SentinelAlerts => Set<SentinelAlert>();
    public DbSet<SentinelIncident> SentinelIncidents => Set<SentinelIncident>();
    public DbSet<ThreatIndicator> ThreatIndicators => Set<ThreatIndicator>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();

    public DbSet<Case> Cases => Set<Case>();
    public DbSet<CaseAlert> CaseAlerts => Set<CaseAlert>();
    public DbSet<CaseTransaction> CaseTransactions => Set<CaseTransaction>();
    public DbSet<CaseNote> CaseNotes => Set<CaseNote>();
    public DbSet<CaseTimeline> CaseTimelines => Set<CaseTimeline>();
    public DbSet<CaseAttachment> CaseAttachments => Set<CaseAttachment>();
    public DbSet<CaseEscalation> CaseEscalations => Set<CaseEscalation>();
    public DbSet<SLATracking> SLATrackings => Set<SLATracking>();

    public DbSet<AnalystAction> AnalystActions => Set<AnalystAction>();
    public DbSet<InvestigationSession> InvestigationSessions => Set<InvestigationSession>();
    public DbSet<InvestigationTimeline> InvestigationTimelines => Set<InvestigationTimeline>();
    public DbSet<Evidence> Evidences => Set<Evidence>();
    public DbSet<DeviceTrust> DeviceTrusts => Set<DeviceTrust>();
    public DbSet<CustomerRiskHistory> CustomerRiskHistories => Set<CustomerRiskHistory>();

    public DbSet<WCAInteraction> WCAInteractions => Set<WCAInteraction>();
    public DbSet<PartnerCommunication> PartnerCommunications => Set<PartnerCommunication>();
    public DbSet<CommunicationTemplate> CommunicationTemplates => Set<CommunicationTemplate>();
    public DbSet<PartnerDirectory> PartnerDirectories => Set<PartnerDirectory>();

    public DbSet<FraudMetric> FraudMetrics => Set<FraudMetric>();
    public DbSet<AnalystMetric> AnalystMetrics => Set<AnalystMetric>();
    public DbSet<FraudTrend> FraudTrends => Set<FraudTrend>();
    public DbSet<DailyStatistic> DailyStatistics => Set<DailyStatistic>();

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordHistory> PasswordHistories => Set<PasswordHistory>();

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

        modelBuilder.Entity<FRMAlert>(e =>
        {
            e.HasKey(x => x.AlertID);
            e.HasIndex(x => x.AlertNumber).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountID);
            e.HasOne(x => x.Transaction).WithMany().HasForeignKey(x => x.TransactionID).IsRequired(false);
            e.HasOne(x => x.AssignedAnalyst).WithMany().HasForeignKey(x => x.AssignedAnalystID).IsRequired(false);
        });

        modelBuilder.Entity<AlertAssignment>(e =>
        {
            e.HasKey(x => x.AssignmentID);
            e.HasOne(x => x.Alert).WithMany(a => a.Assignments).HasForeignKey(x => x.AlertID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
            e.HasOne(x => x.AssignedByUser).WithMany().HasForeignKey(x => x.AssignedBy).IsRequired(false);
        });

        modelBuilder.Entity<AlertHistory>(e =>
        {
            e.HasKey(x => x.HistoryID);
            e.HasOne(x => x.Alert).WithMany(a => a.History).HasForeignKey(x => x.AlertID);
            e.HasOne(x => x.ActionByUser).WithMany().HasForeignKey(x => x.ActionBy).IsRequired(false);
        });

        modelBuilder.Entity<AlertComment>(e =>
        {
            e.HasKey(x => x.CommentID);
            e.HasOne(x => x.Alert).WithMany(a => a.Comments).HasForeignKey(x => x.AlertID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
        });

        modelBuilder.Entity<Device>(e =>
        {
            e.HasKey(x => x.DeviceID);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
        });

        modelBuilder.Entity<CustomerSession>(e =>
        {
            e.HasKey(x => x.SessionID);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Device).WithMany(d => d.Sessions).HasForeignKey(x => x.DeviceID).IsRequired(false);
        });

        modelBuilder.Entity<ATOAlert>(e =>
        {
            e.HasKey(x => x.ATOAlertID);
            e.HasIndex(x => x.ATOAlertNumber).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Session).WithMany().HasForeignKey(x => x.SessionID).IsRequired(false);
            e.HasOne(x => x.AssignedAnalyst).WithMany().HasForeignKey(x => x.AssignedAnalystID).IsRequired(false);
        });

        modelBuilder.Entity<SentinelIncident>(e =>
        {
            e.HasKey(x => x.IncidentID);
            e.HasIndex(x => x.IncidentNumber).IsUnique();
            e.HasOne(x => x.AssignedAnalyst).WithMany().HasForeignKey(x => x.AssignedAnalystID).IsRequired(false);
        });

        modelBuilder.Entity<SentinelAlert>(e =>
        {
            e.HasKey(x => x.AlertID);
            e.HasIndex(x => x.AlertNumber).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserID).IsRequired(false);
            e.HasOne(x => x.Device).WithMany().HasForeignKey(x => x.DeviceID).IsRequired(false);
            e.HasOne(x => x.AssignedAnalyst).WithMany().HasForeignKey(x => x.AssignedAnalystID).IsRequired(false);
            e.HasOne(x => x.Incident).WithMany(i => i.Alerts).HasForeignKey(x => x.IncidentID).IsRequired(false);
        });

        modelBuilder.Entity<ThreatIndicator>(e =>
        {
            e.HasKey(x => x.IndicatorID);
        });

        modelBuilder.Entity<SecurityEvent>(e =>
        {
            e.HasKey(x => x.EventID);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Device).WithMany().HasForeignKey(x => x.DeviceID).IsRequired(false);
        });

        modelBuilder.Entity<Case>(e =>
        {
            e.HasKey(x => x.CaseID);
            e.HasIndex(x => x.CaseNumber).IsUnique();
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.AssignedAnalyst).WithMany().HasForeignKey(x => x.AssignedAnalystID).IsRequired(false);
            e.HasOne(x => x.SLA).WithOne(s => s.Case).HasForeignKey<SLATracking>(s => s.CaseID);
        });

        modelBuilder.Entity<CaseAlert>(e =>
        {
            e.HasKey(x => x.CaseAlertID);
            e.HasOne(x => x.Case).WithMany(c => c.Alerts).HasForeignKey(x => x.CaseID);
        });

        modelBuilder.Entity<CaseTransaction>(e =>
        {
            e.HasKey(x => x.CaseTransactionID);
            e.HasOne(x => x.Case).WithMany(c => c.Transactions).HasForeignKey(x => x.CaseID);
            e.HasOne(x => x.Transaction).WithMany().HasForeignKey(x => x.TransactionID);
        });

        modelBuilder.Entity<CaseNote>(e =>
        {
            e.HasKey(x => x.NoteID);
            e.HasOne(x => x.Case).WithMany(c => c.Notes).HasForeignKey(x => x.CaseID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
        });

        modelBuilder.Entity<CaseTimeline>(e =>
        {
            e.HasKey(x => x.TimelineID);
            e.HasOne(x => x.Case).WithMany(c => c.Timelines).HasForeignKey(x => x.CaseID);
            e.HasOne(x => x.ActionByUser).WithMany().HasForeignKey(x => x.ActionBy).IsRequired(false);
        });

        modelBuilder.Entity<CaseAttachment>(e =>
        {
            e.HasKey(x => x.AttachmentID);
            e.HasOne(x => x.Case).WithMany(c => c.Attachments).HasForeignKey(x => x.CaseID);
            e.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedBy);
        });

        modelBuilder.Entity<CaseEscalation>(e =>
        {
            e.HasKey(x => x.EscalationID);
            e.HasOne(x => x.Case).WithMany(c => c.Escalations).HasForeignKey(x => x.CaseID);
            e.HasOne(x => x.EscalatedToUser).WithMany().HasForeignKey(x => x.EscalatedTo);
        });

        modelBuilder.Entity<SLATracking>(e =>
        {
            e.HasKey(x => x.SLAID);
            e.HasIndex(x => x.CaseID).IsUnique();
        });

        modelBuilder.Entity<AnalystAction>(e =>
        {
            e.HasKey(x => x.ActionID);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
            e.HasOne(x => x.Session).WithMany(s => s.Actions).HasForeignKey(x => x.SessionID).IsRequired(false);
        });

        modelBuilder.Entity<WCAInteraction>(e =>
        {
            e.HasKey(x => x.InteractionID);
            e.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
            e.HasOne(x => x.Case).WithMany().HasForeignKey(x => x.CaseID).IsRequired(false);
        });

        modelBuilder.Entity<PartnerCommunication>(e =>
        {
            e.HasKey(x => x.CommunicationID);
            e.HasOne(x => x.Partner).WithMany().HasForeignKey(x => x.PartnerID);
            e.HasOne(x => x.Case).WithMany().HasForeignKey(x => x.CaseID).IsRequired(false);
        });

        modelBuilder.Entity<CommunicationTemplate>(e =>
        {
            e.HasKey(x => x.TemplateID);
        });

        modelBuilder.Entity<PartnerDirectory>(e =>
        {
            e.HasKey(x => x.PartnerID);
        });

        modelBuilder.Entity<FraudMetric>(e =>
        {
            e.HasKey(x => x.MetricID);
            e.Property(x => x.FraudLossPrevented).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<AnalystMetric>(e =>
        {
            e.HasKey(x => x.AnalystMetricID);
            e.HasOne(x => x.Analyst).WithMany().HasForeignKey(x => x.AnalystID);
        });

        modelBuilder.Entity<FraudTrend>(e =>
        {
            e.HasKey(x => x.TrendID);
        });

        modelBuilder.Entity<DailyStatistic>(e =>
        {
            e.HasKey(x => x.StatID);
            e.Property(x => x.TotalVolume).HasColumnType("decimal(18,2)");
            e.Property(x => x.FraudVolume).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(x => x.RoleId);
            e.HasIndex(x => x.RoleName).IsUnique();
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.HasKey(x => x.PermissionId);
            e.HasIndex(x => x.PermissionName).IsUnique();
        });

        modelBuilder.Entity<RolePermission>(e =>
        {
            e.HasKey(x => x.RolePermissionId);
            e.HasOne(x => x.Role).WithMany(r => r.RolePermissions).HasForeignKey(x => x.RoleId);
            e.HasOne(x => x.Permission).WithMany(p => p.RolePermissions).HasForeignKey(x => x.PermissionId);
        });

        modelBuilder.Entity<UserSession>(e =>
        {
            e.HasKey(x => x.SessionId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.TokenId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<PasswordHistory>(e =>
        {
            e.HasKey(x => x.HistoryId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}
