using EnterpriseFraudRiskSystem.DTOs;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ICustomerService
{
    Task<PagedResultDto<CustomerResponseDto>> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<CustomerResponseDto?> GetByIdAsync(int customerId);
    Task<int> CreateAsync(CustomerCreateDto dto, int createdByUserId);
    Task UpdateAsync(CustomerUpdateDto dto);
    Task DeleteAsync(int customerId);
}
