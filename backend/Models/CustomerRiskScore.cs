namespace EnterpriseFraudRiskSystem.Models;

public class CustomerRiskScore
{
    public int RiskScoreId { get; set; }
    public int CustomerId { get; set; }
    public int Score { get; set; }
    public string RiskLevel { get; set; } = "Low";
    public DateTime LastCalculatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
}
