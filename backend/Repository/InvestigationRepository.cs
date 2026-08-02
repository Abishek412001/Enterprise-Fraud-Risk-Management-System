using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class InvestigationRepository : IInvestigationRepository
{
    private readonly ApplicationDbContext _context;

    public InvestigationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Customer360Dto?> GetCustomer360Async(int customerId)
    {
        var cust = await _context.Customers
            .Include(c => c.RiskScore)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);

        if (cust == null) return null;

        var cardsCount = await _context.Cards.CountAsync(card => card.Account != null && card.Account.CustomerId == customerId);
        var txnsCount = await _context.Transactions.CountAsync(t => t.Account != null && t.Account.CustomerId == customerId);
        var frmAlerts = await _context.FRMAlerts.CountAsync(a => a.CustomerID == customerId);
        var atoAlerts = await _context.ATOAlerts.CountAsync(a => a.CustomerID == customerId);
        var sentinelAlerts = await _context.SentinelAlerts.CountAsync(a => a.CustomerID == customerId);
        var cases = await _context.Cases.CountAsync(c => c.CustomerID == customerId && c.Status != "Closed");
        var devices = await _context.Devices.CountAsync(d => d.CustomerID == customerId);

        return new Customer360Dto
        {
            CustomerID = cust.CustomerId,
            FullName = $"{cust.FirstName} {cust.LastName}",
            Email = cust.Email,
            Phone = cust.Phone,
            KycStatus = cust.KycStatus,
            AmlRiskLevel = cust.AmlRiskLevel,
            IsFrozen = cust.IsBlacklisted,
            CustomerSince = cust.CreatedDate,
            CurrentRiskScore = cust.RiskScore?.RiskScore ?? 50,
            RiskCategory = cust.RiskScore?.RiskCategory ?? "Medium",
            TotalAccounts = cust.Accounts.Count,
            TotalCards = cardsCount,
            TotalTransactions = txnsCount,
            FrmAlertsCount = frmAlerts,
            AtoAlertsCount = atoAlerts,
            SentinelAlertsCount = sentinelAlerts,
            OpenCasesCount = cases,
            RegisteredDevicesCount = devices
        };
    }

    public async Task<List<CustomerRiskHistory>> GetRiskHistoryAsync(int customerId)
    {
        return await _context.CustomerRiskHistories
            .Where(r => r.CustomerID == customerId)
            .OrderByDescending(r => r.Timestamp)
            .ToListAsync();
    }

    public async Task<List<InvestigationTimeline>> GetTimelineAsync(int customerId)
    {
        return await _context.InvestigationTimelines
            .Include(t => t.PerformedByUser)
            .Where(t => t.CustomerID == customerId)
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync();
    }

    public async Task<int> StartInvestigationAsync(int customerId, int analystId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_StartInvestigation";
            command.Parameters.Add(new SqlParameter("@CustomerID", customerId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task CloseInvestigationAsync(int sessionId, string summaryNotes)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CloseInvestigation";
            command.Parameters.Add(new SqlParameter("@SessionID", sessionId));
            command.Parameters.Add(new SqlParameter("@SummaryNotes", summaryNotes));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task FreezeCustomerAccountAsync(int customerId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_FreezeCustomerAccount";
            command.Parameters.Add(new SqlParameter("@CustomerID", customerId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task UnfreezeCustomerAccountAsync(int customerId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_UnfreezeCustomerAccount";
            command.Parameters.Add(new SqlParameter("@CustomerID", customerId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task SuspendCardAsync(int cardId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_SuspendCard";
            command.Parameters.Add(new SqlParameter("@CardID", cardId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task ActivateCardAsync(int cardId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_ActivateCard";
            command.Parameters.Add(new SqlParameter("@CardID", cardId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task BlockDeviceAsync(int deviceId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_BlockDevice";
            command.Parameters.Add(new SqlParameter("@DeviceID", deviceId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task TrustDeviceAsync(int deviceId, int analystId, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_TrustDevice";
            command.Parameters.Add(new SqlParameter("@DeviceID", deviceId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task RecordAnalystActionAsync(int customerId, int analystId, int? sessionId, string actionType, string reason, string? details)
    {
        var action = new AnalystAction
        {
            CustomerID = customerId,
            AnalystID = analystId,
            SessionID = sessionId,
            ActionType = actionType,
            Reason = reason,
            Timestamp = DateTime.UtcNow,
            Details = details
        };
        _context.AnalystActions.Add(action);
        await _context.SaveChangesAsync();
    }

    public async Task<InvestigationSummaryStatsDto> GetSummaryStatsAsync()
    {
        var activeSessions = await _context.InvestigationSessions.CountAsync(s => s.Status == "Active");
        var frozenAccounts = await _context.Customers.CountAsync(c => c.IsBlacklisted);
        var suspendedCards = await _context.Cards.CountAsync(c => c.CardStatus == "Blocked");
        var blockedDevices = await _context.Devices.CountAsync(d => d.IsBlocked);
        var today = DateTime.UtcNow.Date;
        var todaySessions = await _context.InvestigationSessions.CountAsync(s => s.StartTime >= today);

        return new InvestigationSummaryStatsDto
        {
            CustomersUnderInvestigationCount = activeSessions,
            AccountsFrozenCount = frozenAccounts,
            CardsSuspendedCount = suspendedCards,
            DevicesBlockedCount = blockedDevices,
            InvestigationsTodayCount = todaySessions,
            AverageInvestigationMinutes = 18.5
        };
    }
}
