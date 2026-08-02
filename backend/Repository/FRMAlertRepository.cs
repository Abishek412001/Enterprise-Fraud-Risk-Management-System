using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class FRMAlertRepository : IFRMAlertRepository
{
    private readonly ApplicationDbContext _context;

    public FRMAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<FRMAlert>> SearchAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize)
    {
        var query = _context.FRMAlerts
            .Include(a => a.Customer)
            .Include(a => a.Account)
            .Include(a => a.AssignedAnalyst)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(a => a.Priority == priority);

        if (!string.IsNullOrWhiteSpace(severity))
            query = query.Where(a => a.Severity == severity);

        if (analystId.HasValue && analystId > 0)
            query = query.Where(a => a.AssignedAnalystID == analystId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(a =>
                a.AlertNumber.Contains(term) ||
                a.AlertType.Contains(term) ||
                a.Customer!.FirstName.Contains(term) ||
                a.Customer!.LastName.Contains(term) ||
                a.Account!.AccountNumber.Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<FRMAlert>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<FRMAlert?> GetByIdDetailAsync(int alertId)
    {
        return await _context.FRMAlerts
            .Include(a => a.Customer)
                .ThenInclude(c => c!.RiskScore)
            .Include(a => a.Account)
                .ThenInclude(acc => acc!.Cards)
            .Include(a => a.Transaction)
            .Include(a => a.AssignedAnalyst)
            .Include(a => a.History)
                .ThenInclude(h => h.ActionByUser)
            .Include(a => a.Comments)
                .ThenInclude(c => c.Analyst)
            .FirstOrDefaultAsync(a => a.AlertID == alertId);
    }

    public async Task<int> CreateAlertAsync(FRMAlert alert, string? notes)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CreateFRMAlert";
            command.Parameters.Add(new SqlParameter("@CustomerID", alert.CustomerID));
            command.Parameters.Add(new SqlParameter("@AccountID", alert.AccountID));
            command.Parameters.Add(new SqlParameter("@TransactionID", (object?)alert.TransactionID ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@AlertType", alert.AlertType));
            command.Parameters.Add(new SqlParameter("@AlertCategory", alert.AlertCategory));
            command.Parameters.Add(new SqlParameter("@Severity", alert.Severity));
            command.Parameters.Add(new SqlParameter("@RiskScore", alert.RiskScore));
            command.Parameters.Add(new SqlParameter("@ResolutionNotes", (object?)notes ?? DBNull.Value));

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

    public async Task AssignAlertAsync(int alertId, int analystId, int? assignedBy)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AssignAlert";
            command.Parameters.Add(new SqlParameter("@AlertID", alertId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@AssignedBy", (object?)assignedBy ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task UpdateStatusAsync(int alertId, string newStatus, int? actionBy, string? comments)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_UpdateAlertStatus";
            command.Parameters.Add(new SqlParameter("@AlertID", alertId));
            command.Parameters.Add(new SqlParameter("@NewStatus", newStatus));
            command.Parameters.Add(new SqlParameter("@ActionBy", (object?)actionBy ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Comments", (object?)comments ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task EscalateAlertAsync(int alertId, int? actionBy, string reason)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_EscalateAlert";
            command.Parameters.Add(new SqlParameter("@AlertID", alertId));
            command.Parameters.Add(new SqlParameter("@ActionBy", (object?)actionBy ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Reason", reason));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task CloseAlertAsync(int alertId, string resolution, string resolutionNotes, int? actionBy)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CloseAlert";
            command.Parameters.Add(new SqlParameter("@AlertID", alertId));
            command.Parameters.Add(new SqlParameter("@Resolution", resolution));
            command.Parameters.Add(new SqlParameter("@ResolutionNotes", resolutionNotes));
            command.Parameters.Add(new SqlParameter("@ActionBy", (object?)actionBy ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task AddCommentAsync(int alertId, int analystId, string comment)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AddAlertComment";
            command.Parameters.Add(new SqlParameter("@AlertID", alertId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@Comment", comment));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task DeleteAlertAsync(int alertId)
    {
        var alert = await _context.FRMAlerts.FindAsync(alertId);
        if (alert != null)
        {
            _context.FRMAlerts.Remove(alert);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<FrmAlertSummaryStatsDto> GetSummaryStatsAsync()
    {
        var total = await _context.FRMAlerts.CountAsync();
        var open = await _context.FRMAlerts.CountAsync(a => a.Status == "Open" || a.Status == "InProgress");
        var critical = await _context.FRMAlerts.CountAsync(a => a.Priority == "Critical" || a.Severity == "Critical");
        var assigned = await _context.FRMAlerts.CountAsync(a => a.AssignedAnalystID.HasValue && (a.Status == "Open" || a.Status == "InProgress"));
        var closed = await _context.FRMAlerts.CountAsync(a => a.Status == "Closed" || a.Status == "FalsePositive");

        var closedAlertsList = await _context.FRMAlerts
            .Where(a => a.ClosedDate.HasValue)
            .Select(a => EF.Functions.DateDiffHour(a.CreatedDate, a.ClosedDate!.Value))
            .ToListAsync();

        double avgAgeHours = closedAlertsList.Any() ? closedAlertsList.Average() : 0.0;

        return new FrmAlertSummaryStatsDto
        {
            TotalAlerts = total,
            OpenAlerts = open,
            CriticalAlerts = critical,
            AssignedAlerts = assigned,
            ClosedAlerts = closed,
            AverageAlertAgeHours = Math.Round(avgAgeHours, 1)
        };
    }
}
