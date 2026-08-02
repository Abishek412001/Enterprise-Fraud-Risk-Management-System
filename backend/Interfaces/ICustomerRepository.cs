using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ICustomerRepository
{
    Task<(List<Customer> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize);
    Task<Customer?> GetByIdAsync(int customerId);
    Task<int> CreateAsync(Customer customer, int createdByUserId);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(int customerId);
}
