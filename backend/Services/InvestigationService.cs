using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;

namespace EnterpriseFraudRiskSystem.Services;

public class InvestigationService : IInvestigationService
{
    private readonly IInvestigationRepository _repository;

    public InvestigationService(IInvestigationRepository repository)
    {
        _repository = repository;
    }

    public async Task<Customer360Dto?> GetCustomer360Async(int customerId)
    {
        return await _repository.GetCustomer360Async(customerId);
    }

    public async Task<List<CustomerRiskHistoryDto>> GetRiskHistoryAsync(int customerId)
    {
        var history = await _repository.GetRiskHistoryAsync(customerId);
        return history.Select(h => new CustomerRiskHistoryDto
        {
            HistoryID = h.HistoryID,
            OldRiskScore = h.OldRiskScore,
            NewRiskScore = h.NewRiskScore,
            ChangeReason = h.ChangeReason,
            Timestamp = h.Timestamp
        }).ToList();
    }

    public async Task<List<InvestigationTimelineDto>> GetTimelineAsync(int customerId)
    {
        var timeline = await _repository.GetTimelineAsync(customerId);
        return timeline.Select(t => new InvestigationTimelineDto
        {
            TimelineID = t.TimelineID,
            EventCategory = t.EventCategory,
            Title = t.Title,
            Description = t.Description,
            Timestamp = t.Timestamp,
            PerformedByName = t.PerformedByUser?.Username
        }).ToList();
    }

    public async Task<int> StartInvestigationAsync(int customerId, int analystId)
    {
        return await _repository.StartInvestigationAsync(customerId, analystId);
    }

    public async Task CloseInvestigationAsync(int sessionId, string summaryNotes)
    {
        await _repository.CloseInvestigationAsync(sessionId, summaryNotes);
    }

    public async Task FreezeCustomerAccountAsync(FreezeAccountDto dto)
    {
        await _repository.FreezeCustomerAccountAsync(dto.CustomerID, dto.AnalystID, dto.Reason);
    }

    public async Task UnfreezeCustomerAccountAsync(FreezeAccountDto dto)
    {
        await _repository.UnfreezeCustomerAccountAsync(dto.CustomerID, dto.AnalystID, dto.Reason);
    }

    public async Task SuspendCardAsync(SuspendCardDto dto)
    {
        await _repository.SuspendCardAsync(dto.CardID, dto.AnalystID, dto.Reason);
    }

    public async Task ActivateCardAsync(SuspendCardDto dto)
    {
        await _repository.ActivateCardAsync(dto.CardID, dto.AnalystID, dto.Reason);
    }

    public async Task BlockDeviceAsync(DeviceActionDto dto)
    {
        await _repository.BlockDeviceAsync(dto.DeviceID, dto.AnalystID, dto.Reason);
    }

    public async Task TrustDeviceAsync(DeviceActionDto dto)
    {
        await _repository.TrustDeviceAsync(dto.DeviceID, dto.AnalystID, dto.Reason);
    }

    public async Task RecordAnalystActionAsync(AnalystActionDto dto)
    {
        await _repository.RecordAnalystActionAsync(dto.CustomerID, dto.AnalystID, null, dto.ActionType, dto.Reason, dto.Details);
    }

    public async Task<InvestigationSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _repository.GetSummaryStatsAsync();
    }
}
