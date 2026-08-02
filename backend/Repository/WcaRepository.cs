using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class WcaRepository : IWcaRepository
{
    private readonly ApplicationDbContext _context;

    public WcaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<WCAInteraction>> SearchWcaInteractionsAsync(int? customerId, int? caseId, string? actionType, int page, int pageSize)
    {
        var query = _context.WCAInteractions
            .Include(w => w.Customer)
            .Include(w => w.Analyst)
            .AsQueryable();

        if (customerId.HasValue && customerId > 0) query = query.Where(w => w.CustomerID == customerId.Value);
        if (caseId.HasValue && caseId > 0) query = query.Where(w => w.CaseID == caseId.Value);
        if (!string.IsNullOrWhiteSpace(actionType)) query = query.Where(w => w.ActionType == actionType);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(w => w.Timestamp).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<WCAInteraction> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<WCAInteraction?> GetInteractionByIdAsync(int interactionId)
    {
        return await _context.WCAInteractions
            .Include(w => w.Customer)
            .Include(w => w.Analyst)
            .FirstOrDefaultAsync(w => w.InteractionID == interactionId);
    }

    public async Task<PagedResultDto<PartnerCommunication>> SearchPartnerCommunicationsAsync(int? caseId, int? partnerId, string? status, int page, int pageSize)
    {
        var query = _context.PartnerCommunications
            .Include(p => p.Partner)
            .AsQueryable();

        if (caseId.HasValue && caseId > 0) query = query.Where(p => p.CaseID == caseId.Value);
        if (partnerId.HasValue && partnerId > 0) query = query.Where(p => p.PartnerID == partnerId.Value);
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(p => p.Status == status);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(p => p.SentDate).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResultDto<PartnerCommunication> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }

    public async Task<PartnerCommunication?> GetCommunicationByIdAsync(int communicationId)
    {
        return await _context.PartnerCommunications
            .Include(p => p.Partner)
            .FirstOrDefaultAsync(p => p.CommunicationID == communicationId);
    }

    public async Task<List<CommunicationTemplate>> GetActiveTemplatesAsync()
    {
        return await _context.CommunicationTemplates
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public async Task<List<PartnerDirectory>> GetPartnerDirectoryAsync()
    {
        return await _context.PartnerDirectories.ToListAsync();
    }

    public async Task<int> RecordWcaInteractionAsync(RecordWcaInteractionDto dto)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_RecordWCAInteraction";
            command.Parameters.Add(new SqlParameter("@CaseID", (object?)dto.CaseID ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@AlertID", (object?)dto.AlertID ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@CustomerID", dto.CustomerID));
            command.Parameters.Add(new SqlParameter("@AnalystID", dto.AnalystID));
            command.Parameters.Add(new SqlParameter("@ActionType", dto.ActionType));
            command.Parameters.Add(new SqlParameter("@ActionCategory", dto.ActionCategory));
            command.Parameters.Add(new SqlParameter("@ActionDescription", dto.ActionDescription));
            command.Parameters.Add(new SqlParameter("@Comments", (object?)dto.Comments ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@StatusBefore", (object?)dto.StatusBefore ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@StatusAfter", (object?)dto.StatusAfter ?? DBNull.Value));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task<int> SendPartnerCommunicationAsync(SendCommunicationDto dto)
    {
        var connection = _context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CreatePartnerCommunication";
            command.Parameters.Add(new SqlParameter("@CaseID", (object?)dto.CaseID ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@PartnerID", dto.PartnerID));
            command.Parameters.Add(new SqlParameter("@PartnerName", dto.PartnerName));
            command.Parameters.Add(new SqlParameter("@CommunicationType", dto.CommunicationType));
            command.Parameters.Add(new SqlParameter("@Channel", dto.Channel));
            command.Parameters.Add(new SqlParameter("@Subject", dto.Subject));
            command.Parameters.Add(new SqlParameter("@Message", dto.Message));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return reader.GetInt32(0);
            }
            return 0;
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task<WcaSummaryStatsDto> GetSummaryStatsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var actionsToday = await _context.WCAInteractions.CountAsync(w => w.Timestamp >= today);
        var pendingPartner = await _context.PartnerCommunications.CountAsync(p => p.Status == "PendingResponse");
        var sentCount = await _context.PartnerCommunications.CountAsync(p => p.Direction == "Outbound");
        var receivedCount = await _context.PartnerCommunications.CountAsync(p => p.Direction == "Inbound");

        return new WcaSummaryStatsDto
        {
            TodayWcaActionsCount = actionsToday,
            PendingPartnerResponsesCount = pendingPartner,
            CommunicationsSentCount = sentCount,
            CommunicationsReceivedCount = receivedCount,
            AveragePartnerResponseHours = 12.4
        };
    }
}
