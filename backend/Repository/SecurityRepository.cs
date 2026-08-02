using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class SecurityRepository : ISecurityRepository
{
    private readonly ApplicationDbContext _context;

    public SecurityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsAsync()
    {
        return await _context.Permissions.ToListAsync();
    }

    public async Task AssignRoleAsync(int userId, string roleName)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Role = roleName;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize)
    {
        var logs = await _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return logs.Select(a => new AuditLogDto
        {
            AuditId = a.AuditId,
            UserId = a.UserId,
            Username = a.User?.Username,
            Action = a.Action,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            Details = a.Details,
            IpAddress = a.IpAddress,
            Timestamp = a.Timestamp
        }).ToList();
    }
}
