namespace EnterpriseFraudRiskSystem.Models;

public class FraudRule
{
    public int RuleID { get; set; }
    public string RuleCode { get; set; } = string.Empty;
    public string RuleName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ConditionExpression { get; set; } = string.Empty;
    public int RiskScoreWeight { get; set; } = 20;
    public string ActionToTake { get; set; } = "CreateAlert";
    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
