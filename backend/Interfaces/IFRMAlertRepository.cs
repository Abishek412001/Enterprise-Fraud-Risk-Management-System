using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IFRMAlertRepository
{
    Task<PagedResultDto<FRMAlert>> SearchAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize);

    Task<FRMAlert?> GetByIdDetailAsync(int alertId);
    Task<int> CreateAlertAsync(FRMAlert alert, string? notes);
    Task AssignAlertAsync(int alertId, int analystId, int? assignedBy);
    Task UpdateStatusAsync(int alertId, string newStatus, int? actionBy, string? comments);
    Task EscalateAlertAsync(int alertId, int? actionBy, string reason);
    Task CloseAlertAsync(int alertId, string resolution, string resolutionNotes, int? actionBy);
    Task AddCommentAsync(int alertId, int analystId, string comment);
    Task DeleteAlertAsync(int alertId);
    Task<FrmAlertSummaryStatsDto> GetSummaryStatsAsync();
}
