using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IInvestigationRepository
{
    Task<Customer360Dto?> GetCustomer360Async(int customerId);
    Task<List<CustomerRiskHistory>> GetRiskHistoryAsync(int customerId);
    Task<List<InvestigationTimeline>> GetTimelineAsync(int customerId);

    Task<int> StartInvestigationAsync(int customerId, int analystId);
    Task CloseInvestigationAsync(int sessionId, string summaryNotes);

    Task FreezeCustomerAccountAsync(int customerId, int analystId, string reason);
    Task UnfreezeCustomerAccountAsync(int customerId, int analystId, string reason);
    Task SuspendCardAsync(int cardId, int analystId, string reason);
    Task ActivateCardAsync(int cardId, int analystId, string reason);
    Task BlockDeviceAsync(int deviceId, int analystId, string reason);
    Task TrustDeviceAsync(int deviceId, int analystId, string reason);

    Task RecordAnalystActionAsync(int customerId, int analystId, int? sessionId, string actionType, string reason, string? details);

    Task<InvestigationSummaryStatsDto> GetSummaryStatsAsync();
}
