using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ISecurityRepository
{
    Task<List<Role>> GetRolesAsync();
    Task<List<Permission>> GetPermissionsAsync();
    Task AssignRoleAsync(int userId, string roleName);
    Task<List<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize);
}
