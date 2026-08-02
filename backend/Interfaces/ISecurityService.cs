using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ISecurityService
{
    Task<List<RoleDto>> GetRolesAsync();
    Task<List<PermissionDto>> GetPermissionsAsync();
    Task AssignRoleAsync(AssignRoleDto dto);
    Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize);
}
