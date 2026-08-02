using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class SentinelRepository : ISentinelRepository
{
    private readonly ApplicationDbContext _context;

    public SentinelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<SentinelAlert>> SearchSentinelAlertsAsync(string? severity, string? status, string? searchTerm, int page, int pageSize)
    {
        var query = _context.SentinelAlerts
            .Include(a => a.Customer)
            .Include(a => a.AssignedAnalyst)
            .Include(a => a.Incident)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity)) query = query.Where(a => a.Severity == severity);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(a => a.Status == status);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(a => a.AlertNumber.Contains(term) || a.AlertName.Contains(term) || a.IPAddress.Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(a => a.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<SentinelAlert> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<SentinelAlert?> GetAlertByIdAsync(int alertId)
    {
        return await _context.SentinelAlerts
            .Include(a => a.Customer)
            .Include(a => a.AssignedAnalyst)
            .Include(a => a.Incident)
            .FirstOrDefaultAsync(a => a.AlertID == alertId);
    }

    public async Task<PagedResultDto<SentinelIncident>> SearchIncidentsAsync(string? severity, string? status, int page, int pageSize)
    {
        var query = _context.SentinelIncidents
            .Include(i => i.AssignedAnalyst)
            .Include(i => i.Alerts)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(severity)) query = query.Where(i => i.Severity == severity);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(i => i.Status == status);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(i => i.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<SentinelIncident> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<SentinelIncident?> GetIncidentByIdDetailAsync(int incidentId)
    {
        return await _context.SentinelIncidents
            .Include(i => i.AssignedAnalyst)
            .Include(i => i.Alerts)
                .ThenInclude(a => a.Customer)
            .FirstOrDefaultAsync(i => i.IncidentID == incidentId);
    }

    public async Task<PagedResultDto<SecurityEvent>> SearchSecurityEventsAsync(int? customerId, string? eventType, int page, int pageSize)
    {
        var query = _context.SecurityEvents
            .Include(e => e.Customer)
            .AsQueryable();

        if (customerId.HasValue && customerId > 0) query = query.Where(e => e.CustomerID == customerId.Value);
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(e => e.EventType == eventType);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(e => e.EventTime).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<SecurityEvent> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PagedResultDto<ThreatIndicator>> SearchThreatIndicatorsAsync(string? threatLevel, string? source, int page, int pageSize)
    {
        var query = _context.ThreatIndicators.AsQueryable();

        if (!string.IsNullOrWhiteSpace(threatLevel)) query = query.Where(t => t.ThreatLevel == threatLevel);
        if (!string.IsNullOrWhiteSpace(source)) query = query.Where(t => t.Source == source);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(t => t.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<ThreatIndicator> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<int> RecordSecurityEventAsync(CreateSecurityEventDto dto)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_RecordSecurityEvent";
            command.Parameters.Add(new SqlParameter("@CustomerID", dto.CustomerID));
            command.Parameters.Add(new SqlParameter("@IPAddress", dto.IPAddress));
            command.Parameters.Add(new SqlParameter("@DeviceID", (object?)dto.DeviceID ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@EventType", dto.EventType));
            command.Parameters.Add(new SqlParameter("@Result", dto.Result));
            command.Parameters.Add(new SqlParameter("@Application", dto.Application));
            command.Parameters.Add(new SqlParameter("@OperatingSystem", dto.OperatingSystem));

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

    public async Task AssignIncidentAsync(int incidentId, int analystId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AssignIncident";
            command.Parameters.Add(new SqlParameter("@IncidentID", incidentId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task CloseIncidentAsync(int incidentId)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CloseIncident";
            command.Parameters.Add(new SqlParameter("@IncidentID", incidentId));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task<SentinelSummaryStatsDto> GetSummaryStatsAsync()
    {
        var openIncidents = await _context.SentinelIncidents.CountAsync(i => i.Status == "New" || i.Status == "Active");
        var criticalIncidents = await _context.SentinelIncidents.CountAsync(i => i.Severity == "Critical" && i.Status != "Closed");
        var highRiskDev = await _context.Devices.CountAsync(d => d.IsBlocked);
        var activeIndicators = await _context.ThreatIndicators.CountAsync();
        var today = DateTime.UtcNow.Date;
        var eventsToday = await _context.SecurityEvents.CountAsync(e => e.EventTime >= today);

        return new SentinelSummaryStatsDto
        {
            OpenIncidentsCount = openIncidents,
            CriticalIncidentsCount = criticalIncidents,
            HighRiskDevicesCount = highRiskDev,
            ActiveThreatIndicatorsCount = activeIndicators,
            SecurityEventsTodayCount = eventsToday
        };
    }
}
