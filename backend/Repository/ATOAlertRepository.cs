using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class ATOAlertRepository : IATOAlertRepository
{
    private readonly ApplicationDbContext _context;

    public ATOAlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<ATOAlert>> SearchAtoAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize)
    {
        var query = _context.ATOAlerts
            .Include(a => a.Customer)
            .Include(a => a.Session)
                .ThenInclude(s => s!.Device)
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
                a.ATOAlertNumber.Contains(term) ||
                a.AlertType.Contains(term) ||
                a.Customer!.FirstName.Contains(term) ||
                a.Customer!.LastName.Contains(term) ||
                (a.Session != null && a.Session.IPAddress.Contains(term)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<ATOAlert>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ATOAlert?> GetByIdDetailAsync(int atoAlertId)
    {
        return await _context.ATOAlerts
            .Include(a => a.Customer)
            .Include(a => a.Session)
                .ThenInclude(s => s!.Device)
            .Include(a => a.AssignedAnalyst)
            .FirstOrDefaultAsync(a => a.ATOAlertID == atoAlertId);
    }

    public async Task AssignAtoAlertAsync(int atoAlertId, int analystId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AssignATOAlert";
            command.Parameters.Add(new SqlParameter("@ATOAlertID", atoAlertId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task CloseAtoAlertAsync(int atoAlertId, string resolution, string resolutionNotes)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CloseATOAlert";
            command.Parameters.Add(new SqlParameter("@ATOAlertID", atoAlertId));
            command.Parameters.Add(new SqlParameter("@Resolution", resolution));
            command.Parameters.Add(new SqlParameter("@ResolutionNotes", resolutionNotes));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task<PagedResultDto<CustomerSession>> SearchSessionsAsync(int? customerId, string? status, int page, int pageSize)
    {
        var query = _context.CustomerSessions
            .Include(s => s.Customer)
            .Include(s => s.Device)
            .AsQueryable();

        if (customerId.HasValue && customerId > 0)
            query = query.Where(s => s.CustomerID == customerId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(s => s.LoginStatus == status);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(s => s.LoginTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<CustomerSession>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedResultDto<Device>> SearchDevicesAsync(int? customerId, bool? isBlocked, int page, int pageSize)
    {
        var query = _context.Devices
            .Include(d => d.Customer)
            .AsQueryable();

        if (customerId.HasValue && customerId > 0)
            query = query.Where(d => d.CustomerID == customerId.Value);

        if (isBlocked.HasValue)
            query = query.Where(d => d.IsBlocked == isBlocked.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(d => d.LastSeen)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<Device>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<int> RecordCustomerLoginAsync(RecordCustomerLoginDto dto)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_RecordLogin";
            command.Parameters.Add(new SqlParameter("@CustomerID", dto.CustomerID));
            command.Parameters.Add(new SqlParameter("@DeviceFingerprint", dto.DeviceFingerprint));
            command.Parameters.Add(new SqlParameter("@IPAddress", dto.IPAddress));
            command.Parameters.Add(new SqlParameter("@Country", dto.Country));
            command.Parameters.Add(new SqlParameter("@Browser", dto.Browser));
            command.Parameters.Add(new SqlParameter("@OperatingSystem", dto.OperatingSystem));
            command.Parameters.Add(new SqlParameter("@LoginStatus", dto.LoginStatus));
            command.Parameters.Add(new SqlParameter("@IsTorVpn", dto.IsTorVpn));

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

    public async Task SetDeviceStatusAsync(int deviceId, bool isBlocked, bool isTrusted)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device != null)
        {
            device.IsBlocked = isBlocked;
            device.IsTrusted = isTrusted;
            device.LastSeen = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<AtoSummaryStatsDto> GetSummaryStatsAsync()
    {
        var total = await _context.ATOAlerts.CountAsync();
        var open = await _context.ATOAlerts.CountAsync(a => a.Status == "Open" || a.Status == "InProgress");
        var today = DateTime.UtcNow.Date;
        var highRiskToday = await _context.CustomerSessions.CountAsync(s => s.LoginTime >= today && s.RiskScore >= 60);
        var failedToday = await _context.CustomerSessions.CountAsync(s => s.LoginTime >= today && s.LoginStatus == "Failed");
        var suspiciousDev = await _context.Devices.CountAsync(d => d.IsBlocked || !d.IsTrusted);

        return new AtoSummaryStatsDto
        {
            TotalAtoAlerts = total,
            OpenAtoAlerts = open,
            HighRiskLoginsToday = highRiskToday,
            FailedLoginsToday = failedToday,
            SuspiciousDevicesCount = suspiciousDev
        };
    }
}
