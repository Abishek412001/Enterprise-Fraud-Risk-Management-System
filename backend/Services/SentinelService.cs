using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class SentinelService : ISentinelService
{
    private readonly ISentinelRepository _sentinelRepository;

    public SentinelService(ISentinelRepository sentinelRepository)
    {
        _sentinelRepository = sentinelRepository;
    }

    public async Task<PagedResultDto<SentinelAlertResponseDto>> SearchSentinelAlertsAsync(string? severity, string? status, string? searchTerm, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _sentinelRepository.SearchSentinelAlertsAsync(severity, status, searchTerm, page, pageSize);

        return new PagedResultDto<SentinelAlertResponseDto>
        {
            Items = pagedResult.Items.Select(MapToAlertDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<SentinelAlertResponseDto?> GetAlertByIdAsync(int alertId)
    {
        var alert = await _sentinelRepository.GetAlertByIdAsync(alertId);
        return alert == null ? null : MapToAlertDto(alert);
    }

    public async Task<PagedResultDto<SentinelIncidentResponseDto>> SearchIncidentsAsync(string? severity, string? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _sentinelRepository.SearchIncidentsAsync(severity, status, page, pageSize);

        return new PagedResultDto<SentinelIncidentResponseDto>
        {
            Items = pagedResult.Items.Select(MapToIncidentDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<SentinelIncidentDetailResponseDto?> GetIncidentByIdDetailAsync(int incidentId)
    {
        var incident = await _sentinelRepository.GetIncidentByIdDetailAsync(incidentId);
        if (incident == null) return null;

        var firstAlert = incident.Alerts.FirstOrDefault();
        var customerId = firstAlert?.CustomerID ?? 1;

        var secEvents = await _sentinelRepository.SearchSecurityEventsAsync(customerId, null, 1, 10);
        var threatInd = await _sentinelRepository.SearchThreatIndicatorsAsync(null, null, 1, 10);

        var dto = new SentinelIncidentDetailResponseDto
        {
            IncidentID = incident.IncidentID,
            IncidentNumber = incident.IncidentNumber,
            Title = incident.Title,
            Description = incident.Description,
            Severity = incident.Severity,
            Status = incident.Status,
            AssignedAnalystID = incident.AssignedAnalystID,
            AssignedAnalystName = incident.AssignedAnalyst?.Username,
            CreatedDate = incident.CreatedDate,
            CorrelatedAlertsCount = incident.Alerts.Count,
            CustomerEmail = firstAlert?.Customer?.Email ?? "N/A",
            CustomerPhone = firstAlert?.Customer?.Phone ?? "N/A",
            CorrelatedAlerts = incident.Alerts.Select(MapToAlertDto).ToList(),
            SecurityEvents = secEvents.Items.Select(MapToEventDto).ToList(),
            MatchedThreatIndicators = threatInd.Items.Select(MapToThreatDto).ToList()
        };

        return dto;
    }

    public async Task<PagedResultDto<SecurityEventDto>> SearchSecurityEventsAsync(int? customerId, string? eventType, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _sentinelRepository.SearchSecurityEventsAsync(customerId, eventType, page, pageSize);

        return new PagedResultDto<SecurityEventDto>
        {
            Items = pagedResult.Items.Select(MapToEventDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<PagedResultDto<ThreatIndicatorDto>> SearchThreatIndicatorsAsync(string? threatLevel, string? source, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _sentinelRepository.SearchThreatIndicatorsAsync(threatLevel, source, page, pageSize);

        return new PagedResultDto<ThreatIndicatorDto>
        {
            Items = pagedResult.Items.Select(MapToThreatDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<int> RecordSecurityEventAsync(CreateSecurityEventDto dto)
    {
        return await _sentinelRepository.RecordSecurityEventAsync(dto);
    }

    public async Task AssignIncidentAsync(AssignIncidentDto dto)
    {
        await _sentinelRepository.AssignIncidentAsync(dto.IncidentID, dto.AnalystID);
    }

    public async Task CloseIncidentAsync(CloseIncidentDto dto)
    {
        await _sentinelRepository.CloseIncidentAsync(dto.IncidentID);
    }

    public async Task<SentinelSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _sentinelRepository.GetSummaryStatsAsync();
    }

    private static SentinelAlertResponseDto MapToAlertDto(SentinelAlert a) => new()
    {
        AlertID = a.AlertID,
        AlertNumber = a.AlertNumber,
        AlertName = a.AlertName,
        AlertCategory = a.AlertCategory,
        AlertSource = a.AlertSource,
        AlertRule = a.AlertRule,
        CustomerID = a.CustomerID,
        CustomerName = a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : string.Empty,
        IPAddress = a.IPAddress,
        Country = a.Country,
        Severity = a.Severity,
        Priority = a.Priority,
        RiskScore = a.RiskScore,
        Status = a.Status,
        AssignedAnalystID = a.AssignedAnalystID,
        AssignedAnalystName = a.AssignedAnalyst?.Username,
        IncidentID = a.IncidentID,
        CreatedDate = a.CreatedDate
    };

    private static SentinelIncidentResponseDto MapToIncidentDto(SentinelIncident i) => new()
    {
        IncidentID = i.IncidentID,
        IncidentNumber = i.IncidentNumber,
        Title = i.Title,
        Description = i.Description,
        Severity = i.Severity,
        Status = i.Status,
        AssignedAnalystID = i.AssignedAnalystID,
        AssignedAnalystName = i.AssignedAnalyst?.Username,
        CreatedDate = i.CreatedDate,
        CorrelatedAlertsCount = i.Alerts.Count
    };

    private static SecurityEventDto MapToEventDto(SecurityEvent e) => new()
    {
        EventID = e.EventID,
        CustomerID = e.CustomerID,
        CustomerName = e.Customer != null ? $"{e.Customer.FirstName} {e.Customer.LastName}" : string.Empty,
        IPAddress = e.IPAddress,
        EventType = e.EventType,
        EventTime = e.EventTime,
        Result = e.Result,
        Application = e.Application,
        OperatingSystem = e.OperatingSystem
    };

    private static ThreatIndicatorDto MapToThreatDto(ThreatIndicator t) => new()
    {
        IndicatorID = t.IndicatorID,
        IndicatorType = t.IndicatorType,
        IndicatorValue = t.IndicatorValue,
        ThreatLevel = t.ThreatLevel,
        Source = t.Source,
        CreatedDate = t.CreatedDate
    };
}
