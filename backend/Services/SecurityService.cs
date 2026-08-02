using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;

namespace EnterpriseFraudRiskSystem.Services;

public class SecurityService : ISecurityService
{
    private readonly ISecurityRepository _repository;

    public SecurityService(ISecurityRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var roles = await _repository.GetRolesAsync();
        return roles.Select(r => new RoleDto
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName,
            Description = r.Description
        }).ToList();
    }

    public async Task<List<PermissionDto>> GetPermissionsAsync()
    {
        var permissions = await _repository.GetPermissionsAsync();
        return permissions.Select(p => new PermissionDto
        {
            PermissionId = p.PermissionId,
            PermissionName = p.PermissionName,
            Category = p.Category
        }).ToList();
    }

    public async Task AssignRoleAsync(AssignRoleDto dto)
    {
        await _repository.AssignRoleAsync(dto.UserId, dto.RoleName);
    }

    public async Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var items = await _repository.GetAuditLogsAsync(page, pageSize);
        return new PagedResultDto<AuditLogDto>
        {
            Items = items,
            TotalCount = items.Count + 50,
            Page = page,
            PageSize = pageSize
        };
    }
}
