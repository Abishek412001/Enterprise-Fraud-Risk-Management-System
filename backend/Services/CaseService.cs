using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class CaseService : ICaseService
{
    private readonly ICaseRepository _caseRepository;

    public CaseService(ICaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<PagedResultDto<CaseResponseDto>> SearchCasesAsync(string? priority, string? severity, string? status, int? analystId, string? searchTerm, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedResult = await _caseRepository.SearchCasesAsync(priority, severity, status, analystId, searchTerm, page, pageSize);

        return new PagedResultDto<CaseResponseDto>
        {
            Items = pagedResult.Items.Select(MapToResponseDto).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }

    public async Task<CaseDetailResponseDto?> GetCaseByIdDetailAsync(int caseId)
    {
        var c = await _caseRepository.GetCaseByIdDetailAsync(caseId);
        if (c == null) return null;

        var dto = new CaseDetailResponseDto
        {
            CaseID = c.CaseID,
            CaseNumber = c.CaseNumber,
            CaseType = c.CaseType,
            CaseTitle = c.CaseTitle,
            CaseDescription = c.CaseDescription,
            CustomerID = c.CustomerID,
            CustomerName = c.Customer != null ? $"{c.Customer.FirstName} {c.Customer.LastName}" : string.Empty,
            CustomerEmail = c.Customer?.Email ?? string.Empty,
            CustomerPhone = c.Customer?.Phone ?? string.Empty,
            Priority = c.Priority,
            Severity = c.Severity,
            Status = c.Status,
            AssignedAnalystID = c.AssignedAnalystID,
            AssignedAnalystName = c.AssignedAnalyst?.Username,
            CreatedDate = c.CreatedDate,
            DueDate = c.DueDate,
            ResolvedDate = c.ResolvedDate,
            ClosedDate = c.ClosedDate,
            RootCause = c.RootCause,
            Resolution = c.Resolution,
            FalsePositive = c.FalsePositive,
            SLAStatus = c.SLA?.SLAStatus ?? "OnTrack",
            AlertsCount = c.Alerts.Count,
            TransactionsCount = c.Transactions.Count,
            AgeHours = (int)(DateTime.UtcNow - c.CreatedDate).TotalHours,
            LinkedAlerts = c.Alerts.Select(a => new CaseAlertDto { CaseAlertID = a.CaseAlertID, AlertType = a.AlertType, AlertID = a.AlertID, LinkedDate = a.LinkedDate }).ToList(),
            LinkedTransactions = c.Transactions.Select(t => new CaseTransactionDto { CaseTransactionID = t.CaseTransactionID, TransactionID = t.TransactionID, Amount = t.Transaction?.Amount ?? 0, Status = t.Transaction?.Status ?? string.Empty, LinkedDate = t.LinkedDate }).ToList(),
            Notes = c.Notes.Select(n => new CaseNoteDto { NoteID = n.NoteID, AnalystID = n.AnalystID, AnalystName = n.Analyst?.Username ?? "Analyst", NoteType = n.NoteType, Comment = n.Comment, CreatedDate = n.CreatedDate }).ToList(),
            Timeline = c.Timelines.Select(t => new CaseTimelineDto { TimelineID = t.TimelineID, Action = t.Action, ActionBy = t.ActionBy, ActionByName = t.ActionByUser?.Username, Timestamp = t.Timestamp, Details = t.Details }).ToList(),
            Attachments = c.Attachments.Select(a => new CaseAttachmentDto { AttachmentID = a.AttachmentID, FileName = a.FileName, FileType = a.FileType, UploadedBy = a.UploadedBy, UploadedByName = a.UploadedByUser?.Username ?? "Analyst", UploadDate = a.UploadDate }).ToList(),
            Escalations = c.Escalations.Select(e => new CaseEscalationDto { EscalationID = e.EscalationID, EscalatedTo = e.EscalatedTo, EscalatedToName = e.EscalatedToUser?.Username ?? "Supervisor", EscalationReason = e.EscalationReason, EscalationDate = e.EscalationDate }).ToList()
        };

        return dto;
    }

    public async Task<List<CaseResponseDto>> GetOpenCasesAsync()
    {
        var cases = await _caseRepository.GetOpenCasesAsync();
        return cases.Select(MapToResponseDto).ToList();
    }

    public async Task<List<CaseResponseDto>> GetCriticalCasesAsync()
    {
        var cases = await _caseRepository.GetCriticalCasesAsync();
        return cases.Select(MapToResponseDto).ToList();
    }

    public async Task<List<CaseResponseDto>> GetCasesByAnalystAsync(int analystId)
    {
        var cases = await _caseRepository.GetCasesByAnalystAsync(analystId);
        return cases.Select(MapToResponseDto).ToList();
    }

    public async Task<CaseResponseDto> CreateCaseAsync(CreateCaseDto dto)
    {
        var (caseId, caseNumber) = await _caseRepository.CreateCaseAsync(dto);
        var created = await GetCaseByIdDetailAsync(caseId);
        return created ?? new CaseResponseDto { CaseID = caseId, CaseNumber = caseNumber, CaseTitle = dto.CaseTitle };
    }

    public async Task AssignCaseAsync(AssignCaseDto dto)
    {
        await _caseRepository.AssignCaseAsync(dto.CaseID, dto.AnalystID, dto.AssignedBy);
    }

    public async Task EscalateCaseAsync(EscalateCaseDto dto)
    {
        await _caseRepository.EscalateCaseAsync(dto.CaseID, dto.EscalatedTo, dto.EscalationReason, dto.ActionBy);
    }

    public async Task CloseCaseAsync(CloseCaseDto dto)
    {
        await _caseRepository.CloseCaseAsync(dto.CaseID, dto.Resolution, dto.RootCause, dto.FalsePositive, dto.ActionBy);
    }

    public async Task AddCaseNoteAsync(AddCaseNoteDto dto)
    {
        await _caseRepository.AddCaseNoteAsync(dto.CaseID, dto.AnalystID, dto.NoteType, dto.Comment);
    }

    public async Task AddAttachmentAsync(AddCaseAttachmentDto dto)
    {
        await _caseRepository.AddAttachmentAsync(dto.CaseID, dto.FileName, dto.FileType, dto.UploadedBy);
    }

    public async Task UpdateCaseStatusAsync(UpdateCaseStatusDto dto)
    {
        await _caseRepository.UpdateCaseStatusAsync(dto.CaseID, dto.Status, dto.ActionBy);
    }

    public async Task<CaseSummaryStatsDto> GetSummaryStatsAsync()
    {
        return await _caseRepository.GetSummaryStatsAsync();
    }

    private static CaseResponseDto MapToResponseDto(Case c) => new()
    {
        CaseID = c.CaseID,
        CaseNumber = c.CaseNumber,
        CaseType = c.CaseType,
        CaseTitle = c.CaseTitle,
        CaseDescription = c.CaseDescription,
        CustomerID = c.CustomerID,
        CustomerName = c.Customer != null ? $"{c.Customer.FirstName} {c.Customer.LastName}" : string.Empty,
        Priority = c.Priority,
        Severity = c.Severity,
        Status = c.Status,
        AssignedAnalystID = c.AssignedAnalystID,
        AssignedAnalystName = c.AssignedAnalyst?.Username,
        CreatedDate = c.CreatedDate,
        DueDate = c.DueDate,
        SLAStatus = c.SLA?.SLAStatus ?? "OnTrack",
        AlertsCount = c.Alerts.Count,
        TransactionsCount = c.Transactions.Count,
        AgeHours = (int)(DateTime.UtcNow - c.CreatedDate).TotalHours
    };
}
