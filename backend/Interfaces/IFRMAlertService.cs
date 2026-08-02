using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IFRMAlertService
{
    Task<PagedResultDto<FrmAlertResponseDto>> SearchAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize);

    Task<FrmAlertDetailResponseDto?> GetByIdDetailAsync(int alertId);
    Task<FrmAlertResponseDto> CreateAlertAsync(CreateFrmAlertDto dto);
    Task AssignAlertAsync(AssignFrmAlertDto dto, int assignedByUserId);
    Task UpdateStatusAsync(UpdateFrmAlertStatusDto dto, int actionByUserId);
    Task EscalateAlertAsync(EscalateFrmAlertDto dto, int actionByUserId);
    Task CloseAlertAsync(CloseFrmAlertDto dto, int actionByUserId);
    Task AddCommentAsync(AddFrmAlertCommentDto dto, int analystUserId);
    Task DeleteAlertAsync(int alertId);
    Task<FrmAlertSummaryStatsDto> GetSummaryStatsAsync();
}
