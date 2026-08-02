using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IATOAlertService
{
    Task<PagedResultDto<AtoAlertResponseDto>> SearchAtoAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize);

    Task<AtoAlertDetailResponseDto?> GetByIdDetailAsync(int atoAlertId);
    Task AssignAtoAlertAsync(AssignAtoAlertDto dto);
    Task CloseAtoAlertAsync(CloseAtoAlertDto dto);

    Task<PagedResultDto<CustomerSessionDto>> SearchSessionsAsync(int? customerId, string? status, int page, int pageSize);
    Task<PagedResultDto<DeviceDto>> SearchDevicesAsync(int? customerId, bool? isBlocked, int page, int pageSize);

    Task<int> RecordCustomerLoginAsync(RecordCustomerLoginDto dto);
    Task SetDeviceStatusAsync(int deviceId, bool isBlocked, bool isTrusted);
    Task<AtoSummaryStatsDto> GetSummaryStatsAsync();
}
