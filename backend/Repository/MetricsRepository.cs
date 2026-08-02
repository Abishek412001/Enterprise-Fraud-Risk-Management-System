using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class MetricsRepository : IMetricsRepository
{
    private readonly ApplicationDbContext _context;

    public MetricsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync()
    {
        var openFrm = await _context.FRMAlerts.CountAsync(a => a.Status == "Open");
        var openAto = await _context.ATOAlerts.CountAsync(a => a.Status == "Open");
        var openSen = await _context.SentinelAlerts.CountAsync(a => a.Status == "Open");
        var openCases = await _context.Cases.CountAsync(c => c.Status != "Closed");
        var frozenAccounts = await _context.Customers.CountAsync(c => c.IsBlacklisted);
        var activeIncidents = await _context.SentinelIncidents.CountAsync(i => i.Status != "Closed");

        var insights = new List<string>
        {
            "Fraud alerts increased by 18% compared to last week.",
            "Most high-risk transactions originated from Merchant E-Commerce Global.",
            "Account takeover alerts increased in the last 24 hours.",
            "Analyst John Analyst resolved the highest number of cases this week.",
            "Velocity fraud is the fastest growing fraud pattern."
        };

        return new ExecutiveDashboardDto
        {
            TotalOpenAlerts = openFrm + openAto + openSen,
            OpenCases = openCases,
            FrozenAccounts = frozenAccounts,
            ActiveIncidents = activeIncidents,
            SlaComplianceRate = 98.4,
            FraudLossPrevented = 145000.00m,
            RuleBasedInsights = insights
        };
    }

    public async Task<FraudReportDto> GetFraudReportAsync()
    {
        var totalTxns = await _context.Transactions.CountAsync();
        var totalAlerts = await _context.FRMAlerts.CountAsync() + await _context.ATOAlerts.CountAsync();
        var totalCases = await _context.Cases.CountAsync();

        return new FraudReportDto
        {
            ReportDate = DateTime.UtcNow,
            TotalTransactionsAnalyzed = totalTxns,
            TotalAlertsTriggered = totalAlerts,
            FraudCasesOpened = totalCases,
            TotalFraudLossPrevented = 145000.00m,
            FalsePositiveRate = 4.2,
            TopFraudMerchants = new List<string> { "CryptoExchange X", "LuxJewelry Inc", "FastPay Transfer" },
            TopFraudCountries = new List<string> { "USA", "UK", "Brazil" }
        };
    }

    public async Task<List<FraudTrend>> GetFraudTrendsAsync()
    {
        return await _context.FraudTrends.OrderByDescending(t => t.DetectedDate).ToListAsync();
    }

    public async Task<List<AnalystPerformanceDto>> GetAnalystPerformanceAsync()
    {
        var analysts = await _context.Users.Where(u => u.Role == "Fraud Analyst" || u.Role == "Admin").ToListAsync();
        var list = new List<AnalystPerformanceDto>();

        foreach (var a in analysts)
        {
            var assigned = await _context.Cases.CountAsync(c => c.AssignedAnalystID == a.UserId);
            var closed = await _context.Cases.CountAsync(c => c.AssignedAnalystID == a.UserId && c.Status == "Closed");

            list.Add(new AnalystPerformanceDto
            {
                AnalystID = a.UserId,
                AnalystName = a.Username,
                AssignedAlerts = assigned + 5,
                ClosedAlerts = closed + 4,
                OpenCases = assigned - closed,
                AvgInvestigationMinutes = 18.5,
                Escalations = 1,
                SlaComplianceRate = 97.5,
                WorkloadScore = 82.0
            });
        }

        return list;
    }
}
