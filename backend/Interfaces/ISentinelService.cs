using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ISentinelService
{
    Task<PagedResultDto<SentinelAlertResponseDto>> SearchSentinelAlertsAsync(string? severity, string? status, string? searchTerm, int page, int pageSize);
    Task<SentinelAlertResponseDto?> GetAlertByIdAsync(int alertId);

    Task<PagedResultDto<SentinelIncidentResponseDto>> SearchIncidentsAsync(string? severity, string? status, int page, int pageSize);
    Task<SentinelIncidentDetailResponseDto?> GetIncidentByIdDetailAsync(int incidentId);

    Task<PagedResultDto<SecurityEventDto>> SearchSecurityEventsAsync(int? customerId, string? eventType, int page, int pageSize);
    Task<PagedResultDto<ThreatIndicatorDto>> SearchThreatIndicatorsAsync(string? threatLevel, string? source, int page, int pageSize);

    Task<int> RecordSecurityEventAsync(CreateSecurityEventDto dto);
    Task AssignIncidentAsync(AssignIncidentDto dto);
    Task CloseIncidentAsync(CloseIncidentDto dto);

    Task<SentinelSummaryStatsDto> GetSummaryStatsAsync();
}
