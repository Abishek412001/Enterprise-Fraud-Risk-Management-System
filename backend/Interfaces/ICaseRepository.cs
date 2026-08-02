using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ICaseRepository
{
    Task<PagedResultDto<Case>> SearchCasesAsync(string? priority, string? severity, string? status, int? analystId, string? searchTerm, int page, int pageSize);
    Task<Case?> GetCaseByIdDetailAsync(int caseId);
    Task<List<Case>> GetOpenCasesAsync();
    Task<List<Case>> GetCriticalCasesAsync();
    Task<List<Case>> GetCasesByAnalystAsync(int analystId);

    Task<(int caseId, string caseNumber)> CreateCaseAsync(CreateCaseDto dto);
    Task AssignCaseAsync(int caseId, int analystId, int? assignedBy);
    Task EscalateCaseAsync(int caseId, int escalatedTo, string reason, int? actionBy);
    Task CloseCaseAsync(int caseId, string resolution, string rootCause, bool falsePositive, int? actionBy);
    Task AddCaseNoteAsync(int caseId, int analystId, string noteType, string comment);
    Task AddAttachmentAsync(int caseId, string fileName, string fileType, int uploadedBy);
    Task UpdateCaseStatusAsync(int caseId, string status, int? actionBy);

    Task<CaseSummaryStatsDto> GetSummaryStatsAsync();
}
