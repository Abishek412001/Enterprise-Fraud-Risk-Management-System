namespace EnterpriseFraudRiskSystem.Models;

public class CommunicationTemplate
{
    public int TemplateID { get; set; }
    public string TemplateName { get; set; } = string.Empty;
    public string Category { get; set; } = "AccountStatus";
    public string Subject { get; set; } = string.Empty;
    public string MessageBody { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
