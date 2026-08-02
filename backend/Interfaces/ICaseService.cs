using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ICaseService
{
    Task<PagedResultDto<CaseResponseDto>> SearchCasesAsync(string? priority, string? severity, string? status, int? analystId, string? searchTerm, int page, int pageSize);
    Task<CaseDetailResponseDto?> GetCaseByIdDetailAsync(int caseId);
    Task<List<CaseResponseDto>> GetOpenCasesAsync();
    Task<List<CaseResponseDto>> GetCriticalCasesAsync();
    Task<List<CaseResponseDto>> GetCasesByAnalystAsync(int analystId);

    Task<CaseResponseDto> CreateCaseAsync(CreateCaseDto dto);
    Task AssignCaseAsync(AssignCaseDto dto);
    Task EscalateCaseAsync(EscalateCaseDto dto);
    Task CloseCaseAsync(CloseCaseDto dto);
    Task AddCaseNoteAsync(AddCaseNoteDto dto);
    Task AddAttachmentAsync(AddCaseAttachmentDto dto);
    Task UpdateCaseStatusAsync(UpdateCaseStatusDto dto);

    Task<CaseSummaryStatsDto> GetSummaryStatsAsync();
}
