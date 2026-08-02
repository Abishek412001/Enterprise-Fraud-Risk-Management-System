namespace EnterpriseFraudRiskSystem.Models;

public class FraudAlert
{
    public int FraudAlertId { get; set; }
    public long TransactionId { get; set; }
    public int CustomerId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium";
    public string? Description { get; set; }
    public string Status { get; set; } = "Open";
    public int? ReviewedByUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public Transaction? Transaction { get; set; }
    public Customer? Customer { get; set; }
}
