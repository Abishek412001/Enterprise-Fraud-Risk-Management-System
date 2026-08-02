using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class FRMAlertService : IFRMAlertService
{
    private readonly IFRMAlertRepository _frmAlertRepository;

    public FRMAlertService(IFRMAlertRepository frmAlertRepository)
    {
        _frmAlertRepository = frmAlertRepository;
    }

    public async Task<PagedResultDto<FrmAlertResponseDto>> SearchAlertsAsync(
        string? status,
        string? priority,
        string? severity,
        int? analystId,
        string? searchTerm,
        int page,
        int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _frmAlertRepository.SearchAlertsAsync(status, priority, severity, analystId, searchTerm, page, pageSize);

        return new PagedResultDto<FrmAlertResponseDto>
        {
            Items = pagedResult.Items.Select(MapToSummaryDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<FrmAlertDetailResponseDto?> GetByIdDetailAsync(int alertId)
    {
        var alert = await _frmAlertRepository.GetByIdDetailAsync(alertId);
        if (alert == null) return null;

        var dto = new FrmAlertDetailResponseDto
        {
            AlertID = alert.AlertID,
            AlertNumber = alert.AlertNumber,
            CustomerID = alert.CustomerID,
            CustomerName = alert.Customer != null ? $"{alert.Customer.FirstName} {alert.Customer.LastName}" : string.Empty,
            CustomerEmail = alert.Customer?.Email ?? string.Empty,
            CustomerPhone = alert.Customer?.Phone ?? string.Empty,
            NationalIdNumber = alert.Customer?.NationalIdNumber ?? string.Empty,
            AccountID = alert.AccountID,
            AccountNumber = alert.Account?.AccountNumber ?? string.Empty,
            AccountBalance = alert.Account?.Balance ?? 0m,
            AccountStatus = alert.Account?.Status ?? string.Empty,
            TransactionID = alert.TransactionID,
            AlertType = alert.AlertType,
            AlertCategory = alert.AlertCategory,
            Priority = alert.Priority,
            Severity = alert.Severity,
            Status = alert.Status,
            RiskScore = alert.RiskScore,
            AssignedAnalystID = alert.AssignedAnalystID,
            AssignedAnalystName = alert.AssignedAnalyst?.Username,
            CreatedDate = alert.CreatedDate,
            LastUpdated = alert.LastUpdated,
            ClosedDate = alert.ClosedDate,
            Resolution = alert.Resolution,
            ResolutionNotes = alert.ResolutionNotes,
            Cards = alert.Account?.Cards.Select(c => new CardSummaryDto
            {
                CardId = c.CardId,
                CardNumberMasked = c.CardNumberMasked,
                CardType = c.CardType,
                Status = c.Status,
                ExpiryDate = c.ExpiryDate
            }).ToList() ?? new List<CardSummaryDto>(),
            History = alert.History.Select(h => new AlertHistoryDto
            {
                HistoryID = h.HistoryID,
                Action = h.Action,
                OldStatus = h.OldStatus,
                NewStatus = h.NewStatus,
                ActionByUsername = h.ActionByUser?.Username ?? "System",
                Comments = h.Comments,
                Timestamp = h.Timestamp
            }).OrderByDescending(h => h.Timestamp).ToList(),
            Comments = alert.Comments.Select(c => new AlertCommentDto
            {
                CommentID = c.CommentID,
                AnalystUsername = c.Analyst?.Username ?? "Analyst",
                Comment = c.Comment,
                Timestamp = c.Timestamp
            }).OrderByDescending(c => c.Timestamp).ToList()
        };

        return dto;
    }

    public async Task<FrmAlertResponseDto> CreateAlertAsync(CreateFrmAlertDto dto)
    {
        var alert = new FRMAlert
        {
            CustomerID = dto.CustomerID,
            AccountID = dto.AccountID,
            TransactionID = dto.TransactionID,
            AlertType = dto.AlertType,
            AlertCategory = dto.AlertCategory,
            Severity = dto.Severity,
            RiskScore = dto.RiskScore
        };

        var newId = await _frmAlertRepository.CreateAlertAsync(alert, dto.ResolutionNotes);
        var created = await _frmAlertRepository.GetByIdDetailAsync(newId);

        return MapToSummaryDto(created ?? alert);
    }

    public async Task AssignAlertAsync(AssignFrmAlertDto dto, int assignedByUserId)
    {
        await _frmAlertRepository.AssignAlertAsync(dto.AlertID, dto.AnalystID, assignedByUserId);
    }

    public async Task UpdateStatusAsync(UpdateFrmAlertStatusDto dto, int actionByUserId)
    {
        await _frmAlertRepository.UpdateStatusAsync(dto.AlertID, dto.NewStatus, actionByUserId, dto.Comments);
    }

    public async Task EscalateAlertAsync(EscalateFrmAlertDto dto, int actionByUserId)
    {
        await _frmAlertRepository.EscalateAlertAsync(dto.AlertID, actionByUserId, dto.Reason);
    }

    public async Task CloseAlertAsync(CloseFrmAlertDto dto, int actionByUserId)
    {
        await _frmAlertRepository.CloseAlertAsync(dto.AlertID, dto.Resolution, dto.ResolutionNotes, actionByUserId);
    }

    public async Task AddCommentAsync(AddFrmAlertCommentDto dto, int analystUserId)
    {
        await _frmAlertRepository.AddCommentAsync(dto.AlertID, analystUserId, dto.Comment);
    }

    public async Task DeleteAlertAsync(int alertId)
    {
        await _frmAlertRepository.DeleteAlertAsync(alertId);
    }

    public async Task<FrmAlertSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _frmAlertRepository.GetSummaryStatsAsync();
    }

    private static FrmAlertResponseDto MapToSummaryDto(FRMAlert a) => new()
    {
        AlertID = a.AlertID,
        AlertNumber = a.AlertNumber,
        CustomerID = a.CustomerID,
        CustomerName = a.Customer != null ? $"{a.Customer.FirstName} {a.Customer.LastName}" : string.Empty,
        AccountID = a.AccountID,
        AccountNumber = a.Account?.AccountNumber ?? string.Empty,
        TransactionID = a.TransactionID,
        AlertType = a.AlertType,
        AlertCategory = a.AlertCategory,
        Priority = a.Priority,
        Severity = a.Severity,
        Status = a.Status,
        RiskScore = a.RiskScore,
        AssignedAnalystID = a.AssignedAnalystID,
        AssignedAnalystName = a.AssignedAnalyst?.Username,
        CreatedDate = a.CreatedDate,
        LastUpdated = a.LastUpdated,
        ClosedDate = a.ClosedDate,
        Resolution = a.Resolution
    };
}
