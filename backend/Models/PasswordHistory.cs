namespace EnterpriseFraudRiskSystem.Models;

public class PasswordHistory
{
    public int HistoryId { get; set; }
    public int UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime ChangedDate { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
