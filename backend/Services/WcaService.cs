using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class WcaService : IWcaService
{
    private readonly IWcaRepository _wcaRepository;

    public WcaService(IWcaRepository wcaRepository)
    {
        _wcaRepository = wcaRepository;
    }

    public async Task<PagedResultDto<WcaInteractionDto>> SearchWcaInteractionsAsync(int? customerId, int? caseId, string? actionType, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _wcaRepository.SearchWcaInteractionsAsync(customerId, caseId, actionType, page, pageSize);

        return new PagedResultDto<WcaInteractionDto>
        {
            Items = pagedResult.Items.Select(MapToWcaDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<WcaInteractionDto?> GetInteractionByIdAsync(int interactionId)
    {
        var item = await _wcaRepository.GetInteractionByIdAsync(interactionId);
        return item == null ? null : MapToWcaDto(item);
    }

    public async Task<PagedResultDto<PartnerCommunicationDto>> SearchPartnerCommunicationsAsync(int? caseId, int? partnerId, string? status, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _wcaRepository.SearchPartnerCommunicationsAsync(caseId, partnerId, status, page, pageSize);

        return new PagedResultDto<PartnerCommunicationDto>
        {
            Items = pagedResult.Items.Select(MapToPartnerCommsDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<PartnerCommunicationDto?> GetCommunicationByIdAsync(int communicationId)
    {
        var item = await _wcaRepository.GetCommunicationByIdAsync(communicationId);
        return item == null ? null : MapToPartnerCommsDto(item);
    }

    public async Task<List<CommunicationTemplateDto>> GetActiveTemplatesAsync()
    {
        var templates = await _wcaRepository.GetActiveTemplatesAsync();
        return templates.Select(t => new CommunicationTemplateDto
        {
            TemplateID = t.TemplateID,
            TemplateName = t.TemplateName,
            Category = t.Category,
            Subject = t.Subject,
            MessageBody = t.MessageBody,
            IsActive = t.IsActive
        }).ToList();
    }

    public async Task<List<PartnerDirectoryDto>> GetPartnerDirectoryAsync()
    {
        var partners = await _wcaRepository.GetPartnerDirectoryAsync();
        return partners.Select(p => new PartnerDirectoryDto
        {
            PartnerID = p.PartnerID,
            PartnerName = p.PartnerName,
            Department = p.Department,
            Email = p.Email,
            Phone = p.Phone,
            EscalationContact = p.EscalationContact
        }).ToList();
    }

    public async Task<int> RecordWcaInteractionAsync(RecordWcaInteractionDto dto)
    {
        return await _wcaRepository.RecordWcaInteractionAsync(dto);
    }

    public async Task<int> SendPartnerCommunicationAsync(SendCommunicationDto dto)
    {
        return await _wcaRepository.SendPartnerCommunicationAsync(dto);
    }

    public async Task<WcaSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _wcaRepository.GetSummaryStatsAsync();
    }

    private static WcaInteractionDto MapToWcaDto(WCAInteraction w) => new()
    {
        InteractionID = w.InteractionID,
        CaseID = w.CaseID,
        AlertID = w.AlertID,
        CustomerID = w.CustomerID,
        CustomerName = w.Customer != null ? $"{w.Customer.FirstName} {w.Customer.LastName}" : string.Empty,
        AnalystID = w.AnalystID,
        AnalystName = w.Analyst?.Username ?? "Analyst",
        ActionType = w.ActionType,
        ActionCategory = w.ActionCategory,
        ActionDescription = w.ActionDescription,
        Comments = w.Comments,
        StatusBefore = w.StatusBefore,
        StatusAfter = w.StatusAfter,
        Timestamp = w.Timestamp
    };

    private static PartnerCommunicationDto MapToPartnerCommsDto(PartnerCommunication p) => new()
    {
        CommunicationID = p.CommunicationID,
        CaseID = p.CaseID,
        PartnerID = p.PartnerID,
        PartnerName = p.PartnerName,
        CommunicationType = p.CommunicationType,
        Direction = p.Direction,
        Channel = p.Channel,
        Subject = p.Subject,
        Message = p.Message,
        Status = p.Status,
        SentDate = p.SentDate,
        ReceivedDate = p.ReceivedDate
    };
}
