namespace EnterpriseFraudRiskSystem.DTOs;

public class CustomerCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string NationalIdNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string Country { get; set; } = string.Empty;
}

public class CustomerUpdateDto : CustomerCreateDto
{
    public int CustomerId { get; set; }
}

public class CustomerResponseDto
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public bool IsBlacklisted { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public DateTime CreatedAt { get; set; }
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
