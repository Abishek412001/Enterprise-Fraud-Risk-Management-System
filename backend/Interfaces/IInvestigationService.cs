using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IInvestigationService
{
    Task<Customer360Dto?> GetCustomer360Async(int customerId);
    Task<List<CustomerRiskHistoryDto>> GetRiskHistoryAsync(int customerId);
    Task<List<InvestigationTimelineDto>> GetTimelineAsync(int customerId);

    Task<int> StartInvestigationAsync(int customerId, int analystId);
    Task CloseInvestigationAsync(int sessionId, string summaryNotes);

    Task FreezeCustomerAccountAsync(FreezeAccountDto dto);
    Task UnfreezeCustomerAccountAsync(FreezeAccountDto dto);
    Task SuspendCardAsync(SuspendCardDto dto);
    Task ActivateCardAsync(SuspendCardDto dto);
    Task BlockDeviceAsync(DeviceActionDto dto);
    Task TrustDeviceAsync(DeviceActionDto dto);

    Task RecordAnalystActionAsync(AnalystActionDto dto);

    Task<InvestigationSummaryStatsDto> GetSummaryStatsAsync();
}

public class CustomerRiskHistoryDto
{
    public int HistoryID { get; set; }
    public int OldRiskScore { get; set; }
    public int NewRiskScore { get; set; }
    public string ChangeReason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class InvestigationTimelineDto
{
    public int TimelineID { get; set; }
    public string EventCategory { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string? PerformedByName { get; set; }
}
