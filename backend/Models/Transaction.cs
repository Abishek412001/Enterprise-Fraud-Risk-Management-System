namespace EnterpriseFraudRiskSystem.Models;

public class Transaction
{
    public long TransactionId { get; set; }
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
    public string Status { get; set; } = "Approved";
    public DateTime TransactionAt { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
    public Card? Card { get; set; }
    public Merchant? Merchant { get; set; }
}
