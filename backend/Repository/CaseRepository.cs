using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class CaseRepository : ICaseRepository
{
    private readonly ApplicationDbContext _context;

    public CaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<Case>> SearchCasesAsync(string? priority, string? severity, string? status, int? analystId, string? searchTerm, int page, int pageSize)
    {
        var query = _context.Cases
            .Include(c => c.Customer)
            .Include(c => c.AssignedAnalyst)
            .Include(c => c.SLA)
            .Include(c => c.Alerts)
            .Include(c => c.Transactions)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(priority)) query = query.Where(c => c.Priority == priority);
        if (!string.IsNullOrWhiteSpace(severity)) query = query.Where(c => c.Severity == severity);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(c => c.Status == status);
        if (analystId.HasValue && analystId > 0) query = query.Where(c => c.AssignedAnalystID == analystId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(c => c.CaseNumber.Contains(term) || c.CaseTitle.Contains(term));
        }

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(c => c.CreatedDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<Case> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<Case?> GetCaseByIdDetailAsync(int caseId)
    {
        return await _context.Cases
            .Include(c => c.Customer)
            .Include(c => c.AssignedAnalyst)
            .Include(c => c.SLA)
            .Include(c => c.Alerts)
            .Include(c => c.Transactions)
                .ThenInclude(t => t.Transaction)
            .Include(c => c.Notes)
                .ThenInclude(n => n.Analyst)
            .Include(c => c.Timelines)
                .ThenInclude(t => t.ActionByUser)
            .Include(c => c.Attachments)
                .ThenInclude(a => a.UploadedByUser)
            .Include(c => c.Escalations)
                .ThenInclude(e => e.EscalatedToUser)
            .FirstOrDefaultAsync(c => c.CaseID == caseId);
    }

    public async Task<List<Case>> GetOpenCasesAsync()
    {
        return await _context.Cases
            .Include(c => c.Customer)
            .Include(c => c.AssignedAnalyst)
            .Include(c => c.SLA)
            .Where(c => c.Status == "Open" || c.Status == "InProgress" || c.Status == "Escalated")
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Case>> GetCriticalCasesAsync()
    {
        return await _context.Cases
            .Include(c => c.Customer)
            .Include(c => c.AssignedAnalyst)
            .Include(c => c.SLA)
            .Where(c => c.Priority == "Critical" && c.Status != "Closed")
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<List<Case>> GetCasesByAnalystAsync(int analystId)
    {
        return await _context.Cases
            .Include(c => c.Customer)
            .Include(c => c.SLA)
            .Where(c => c.AssignedAnalystID == analystId && c.Status != "Closed")
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<(int caseId, string caseNumber)> CreateCaseAsync(CreateCaseDto dto)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CreateCase";
            command.Parameters.Add(new SqlParameter("@CaseType", dto.CaseType));
            command.Parameters.Add(new SqlParameter("@CaseTitle", dto.CaseTitle));
            command.Parameters.Add(new SqlParameter("@CaseDescription", (object?)dto.CaseDescription ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@CustomerID", dto.CustomerID));
            command.Parameters.Add(new SqlParameter("@Priority", dto.Priority));
            command.Parameters.Add(new SqlParameter("@Severity", dto.Severity));
            command.Parameters.Add(new SqlParameter("@CreatedBy", (object?)dto.CreatedBy ?? DBNull.Value));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var caseId = reader.GetInt32(0);
                var caseNumber = reader.GetString(1);
                return (caseId, caseNumber);
            }
            return (0, string.Empty);
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task AssignCaseAsync(int caseId, int analystId, int? assignedBy)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AssignCase";
            command.Parameters.Add(new SqlParameter("@CaseID", caseId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@AssignedBy", (object?)assignedBy ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task EscalateCaseAsync(int caseId, int escalatedTo, string reason, int? actionBy)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_EscalateCase";
            command.Parameters.Add(new SqlParameter("@CaseID", caseId));
            command.Parameters.Add(new SqlParameter("@EscalatedTo", escalatedTo));
            command.Parameters.Add(new SqlParameter("@EscalationReason", reason));
            command.Parameters.Add(new SqlParameter("@ActionBy", (object?)actionBy ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task CloseCaseAsync(int caseId, string resolution, string rootCause, bool falsePositive, int? actionBy)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CloseCase";
            command.Parameters.Add(new SqlParameter("@CaseID", caseId));
            command.Parameters.Add(new SqlParameter("@Resolution", resolution));
            command.Parameters.Add(new SqlParameter("@RootCause", rootCause));
            command.Parameters.Add(new SqlParameter("@FalsePositive", falsePositive));
            command.Parameters.Add(new SqlParameter("@ActionBy", (object?)actionBy ?? DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task AddCaseNoteAsync(int caseId, int analystId, string noteType, string comment)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_AddCaseNote";
            command.Parameters.Add(new SqlParameter("@CaseID", caseId));
            command.Parameters.Add(new SqlParameter("@AnalystID", analystId));
            command.Parameters.Add(new SqlParameter("@NoteType", noteType));
            command.Parameters.Add(new SqlParameter("@Comment", comment));

            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task AddAttachmentAsync(int caseId, string fileName, string fileType, int uploadedBy)
    {
        var attachment = new CaseAttachment
        {
            CaseID = caseId,
            FileName = fileName,
            FileType = fileType,
            UploadedBy = uploadedBy,
            UploadDate = DateTime.UtcNow
        };
        _context.CaseAttachments.Add(attachment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCaseStatusAsync(int caseId, string status, int? actionBy)
    {
        var c = await _context.Cases.FindAsync(caseId);
        if (c != null)
        {
            c.Status = status;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CaseSummaryStatsDto> GetSummaryStatsAsync()
    {
        var openCount = await _context.Cases.CountAsync(c => c.Status != "Closed");
        var criticalCount = await _context.Cases.CountAsync(c => c.Priority == "Critical" && c.Status != "Closed");
        var breachesCount = await _context.SLATrackings.CountAsync(s => s.SLAStatus == "Breached");
        var escalatedCount = await _context.Cases.CountAsync(c => c.Status == "Escalated");
        var today = DateTime.UtcNow.Date;
        var closedToday = await _context.Cases.CountAsync(c => c.ClosedDate >= today);

        return new CaseSummaryStatsDto
        {
            OpenCasesCount = openCount,
            MyCasesCount = openCount,
            CriticalCasesCount = criticalCount,
            SlaBreachesCount = breachesCount,
            EscalatedCasesCount = escalatedCount,
            ClosedTodayCount = closedToday,
            AverageResolutionHours = 4.5
        };
    }
}
