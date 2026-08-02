using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IATOAlertRepository
{
    Task<PagedResultDto<ATOAlert>> SearchAtoAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize);

    Task<ATOAlert?> GetByIdDetailAsync(int atoAlertId);
    Task AssignAtoAlertAsync(int atoAlertId, int analystId);
    Task CloseAtoAlertAsync(int atoAlertId, string resolution, string resolutionNotes);

    Task<PagedResultDto<CustomerSession>> SearchSessionsAsync(int? customerId, string? status, int page, int pageSize);
    Task<PagedResultDto<Device>> SearchDevicesAsync(int? customerId, bool? isBlocked, int page, int pageSize);

    Task<int> RecordCustomerLoginAsync(RecordCustomerLoginDto dto);
    Task SetDeviceStatusAsync(int deviceId, bool isBlocked, bool isTrusted);
    Task<AtoSummaryStatsDto> GetSummaryStatsAsync();
}
