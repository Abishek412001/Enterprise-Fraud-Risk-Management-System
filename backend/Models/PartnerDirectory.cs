namespace EnterpriseFraudRiskSystem.Models;

public class PartnerDirectory
{
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string Department { get; set; } = "Fraud Operations";
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? EscalationContact { get; set; }
}
