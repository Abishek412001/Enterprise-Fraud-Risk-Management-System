namespace EnterpriseFraudRiskSystem.Models;

public class LoginHistory
{
    public long LoginHistoryId { get; set; }
    public int UserId { get; set; }
    public string? IpAddress { get; set; }
    public bool IsSuccessful { get; set; }
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
}
