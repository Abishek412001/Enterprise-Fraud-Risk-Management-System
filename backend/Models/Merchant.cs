namespace EnterpriseFraudRiskSystem.Models;

public class Merchant
{
    public int MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public string MerchantCategory { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low";
    public bool IsBlacklisted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
}
