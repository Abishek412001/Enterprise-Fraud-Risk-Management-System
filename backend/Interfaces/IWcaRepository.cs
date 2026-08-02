using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IWcaRepository
{
    Task<PagedResultDto<WCAInteraction>> SearchWcaInteractionsAsync(int? customerId, int? caseId, string? actionType, int page, int pageSize);
    Task<WCAInteraction?> GetInteractionByIdAsync(int interactionId);

    Task<PagedResultDto<PartnerCommunication>> SearchPartnerCommunicationsAsync(int? caseId, int? partnerId, string? status, int page, int pageSize);
    Task<PartnerCommunication?> GetCommunicationByIdAsync(int communicationId);

    Task<List<CommunicationTemplate>> GetActiveTemplatesAsync();
    Task<List<PartnerDirectory>> GetPartnerDirectoryAsync();

    Task<int> RecordWcaInteractionAsync(RecordWcaInteractionDto dto);
    Task<int> SendPartnerCommunicationAsync(SendCommunicationDto dto);

    Task<WcaSummaryStatsDto> GetSummaryStatsAsync();
}
