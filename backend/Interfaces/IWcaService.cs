using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface IWcaService
{
    Task<PagedResultDto<WcaInteractionDto>> SearchWcaInteractionsAsync(int? customerId, int? caseId, string? actionType, int page, int pageSize);
    Task<WcaInteractionDto?> GetInteractionByIdAsync(int interactionId);

    Task<PagedResultDto<PartnerCommunicationDto>> SearchPartnerCommunicationsAsync(int? caseId, int? partnerId, string? status, int page, int pageSize);
    Task<PartnerCommunicationDto?> GetCommunicationByIdAsync(int communicationId);

    Task<List<CommunicationTemplateDto>> GetActiveTemplatesAsync();
    Task<List<PartnerDirectoryDto>> GetPartnerDirectoryAsync();

    Task<int> RecordWcaInteractionAsync(RecordWcaInteractionDto dto);
    Task<int> SendPartnerCommunicationAsync(SendCommunicationDto dto);

    Task<WcaSummaryStatsDto> GetSummaryStatsAsync();
}
