namespace EnterpriseFraudRiskSystem.Models;

public class CaseAlert
{
    public int CaseAlertID { get; set; }
    public int CaseID { get; set; }
    public string AlertType { get; set; } = string.Empty; // FRM | ATO | Sentinel | Legacy
    public int AlertID { get; set; }
    public DateTime LinkedDate { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
}
