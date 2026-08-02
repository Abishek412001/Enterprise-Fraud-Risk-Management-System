namespace EnterpriseFraudRiskSystem.Models;

public class AlertComment
{
    public int CommentID { get; set; }
    public int AlertID { get; set; }
    public int AnalystID { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public FRMAlert? Alert { get; set; }
    public User? Analyst { get; set; }
}
