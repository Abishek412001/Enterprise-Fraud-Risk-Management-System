using System.Data;
using EnterpriseFraudRiskSystem.Data;
using EnterpriseFraudRiskSystem.Interfaces;
using EnterpriseFraudRiskSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseFraudRiskSystem.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public CustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Customer> Items, int TotalCount)> SearchAsync(string? searchTerm, int page, int pageSize)
    {
        var query = _context.Customers.Include(c => c.RiskScore).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                c.Email.ToLower().Contains(term) ||
                c.NationalIdNumber.Contains(term));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Customer?> GetByIdAsync(int customerId)
    {
        return await _context.Customers
            .Include(c => c.RiskScore)
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
    }

    public async Task<int> CreateAsync(Customer customer, int createdByUserId)
    {
        var connection = (SqlConnection)_context.Database.GetDbConnection();
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed) await connection.OpenAsync();

        try
        {
            using var command = connection.CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "usp_CreateCustomer";
            command.Parameters.Add(new SqlParameter("@FirstName", customer.FirstName));
            command.Parameters.Add(new SqlParameter("@LastName", customer.LastName));
            command.Parameters.Add(new SqlParameter("@Email", customer.Email));
            command.Parameters.Add(new SqlParameter("@Phone", customer.Phone));
            command.Parameters.Add(new SqlParameter("@NationalIdNumber", customer.NationalIdNumber));
            command.Parameters.Add(new SqlParameter("@DateOfBirth", customer.DateOfBirth));
            command.Parameters.Add(new SqlParameter("@Address", (object?)customer.Address ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@City", (object?)customer.City ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Country", customer.Country));
            command.Parameters.Add(new SqlParameter("@CreatedByUserId", createdByUserId));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        finally
        {
            if (wasClosed) await connection.CloseAsync();
        }
    }

    public async Task UpdateAsync(Customer customer)
    {
        var existing = await _context.Customers.FindAsync(customer.CustomerId);
        if (existing is null) return;

        existing.FirstName = customer.FirstName;
        existing.LastName = customer.LastName;
        existing.Email = customer.Email;
        existing.Phone = customer.Phone;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.Country = customer.Country;
        existing.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int customerId)
    {
        var existing = await _context.Customers.FindAsync(customerId);
        if (existing is null) return;

        _context.Customers.Remove(existing);
        await _context.SaveChangesAsync();
    }
}
