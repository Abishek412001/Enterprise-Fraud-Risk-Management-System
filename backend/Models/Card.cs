namespace EnterpriseFraudRiskSystem.Models;

public class Card
{
    public int CardId { get; set; }
    public int AccountId { get; set; }
    public string CardNumberMasked { get; set; } = string.Empty;
    public string CardNumberHash { get; set; } = string.Empty;
    public string CardType { get; set; } = "Debit";
    public DateTime ExpiryDate { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public Account? Account { get; set; }
}
