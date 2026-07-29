namespace EnterpriseFraudRiskSystem.DTOs;

public class TransactionCreateDto
{
    public int AccountId { get; set; }
    public int? CardId { get; set; }
    public int MerchantId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Country { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Channel { get; set; } = "Online";
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
}

public class TransactionResponseDto
{
    public long TransactionId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Country { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime TransactionAt { get; set; }
}
