namespace EnterpriseFraudRiskSystem.Models;

public class Account
{
    public int AccountId { get; set; }
    public int CustomerId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountType { get; set; } = "Savings";
    public string Currency { get; set; } = "USD";
    public decimal Balance { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
