using EnterpriseFraudRiskSystem.DTOs;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<PagedResultDto<CustomerResponseDto>> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var (items, totalCount) = await _customerRepository.SearchAsync(searchTerm, page, pageSize);

        return new PagedResultDto<CustomerResponseDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CustomerResponseDto?> GetByIdAsync(int customerId)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId);
        return customer is null ? null : MapToDto(customer);
    }

    public async Task<int> CreateAsync(CustomerCreateDto dto, int createdByUserId)
    {
        var customer = new Customer
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            NationalIdNumber = dto.NationalIdNumber,
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country
        };

        return await _customerRepository.CreateAsync(customer, createdByUserId);
    }

    public async Task UpdateAsync(CustomerUpdateDto dto)
    {
        var customer = new Customer
        {
            CustomerId = dto.CustomerId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            NationalIdNumber = dto.NationalIdNumber,
            DateOfBirth = dto.DateOfBirth,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country
        };

        await _customerRepository.UpdateAsync(customer);
    }

    public async Task DeleteAsync(int customerId)
    {
        await _customerRepository.DeleteAsync(customerId);
    }

    private static CustomerResponseDto MapToDto(Customer c) => new()
    {
        CustomerId = c.CustomerId,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Email = c.Email,
        Phone = c.Phone,
        Country = c.Country,
        IsBlacklisted = c.IsBlacklisted,
        RiskScore = c.RiskScore?.Score ?? 0,
        RiskLevel = c.RiskScore?.RiskLevel ?? "Low",
        CreatedAt = c.CreatedAt
    };
}
