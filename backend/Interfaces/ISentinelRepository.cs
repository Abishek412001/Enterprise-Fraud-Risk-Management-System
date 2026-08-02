using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ISentinelRepository
{
    Task<PagedResultDto<SentinelAlert>> SearchSentinelAlertsAsync(string? severity, string? status, string? searchTerm, int page, int pageSize);
    Task<SentinelAlert?> GetAlertByIdAsync(int alertId);

    Task<PagedResultDto<SentinelIncident>> SearchIncidentsAsync(string? severity, string? status, int page, int pageSize);
    Task<SentinelIncident?> GetIncidentByIdDetailAsync(int incidentId);

    Task<PagedResultDto<SecurityEvent>> SearchSecurityEventsAsync(int? customerId, string? eventType, int page, int pageSize);
    Task<PagedResultDto<ThreatIndicator>> SearchThreatIndicatorsAsync(string? threatLevel, string? source, int page, int pageSize);

    Task<int> RecordSecurityEventAsync(CreateSecurityEventDto dto);
    Task AssignIncidentAsync(int incidentId, int analystId);
    Task CloseIncidentAsync(int incidentId);

    Task<SentinelSummaryStatsDto> GetSummaryStatsAsync();
}
