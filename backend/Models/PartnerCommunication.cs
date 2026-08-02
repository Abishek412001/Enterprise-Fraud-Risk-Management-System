namespace EnterpriseFraudRiskSystem.Models;

public class PartnerCommunication
{
    public int CommunicationID { get; set; }
    public int? CaseID { get; set; }
    public int PartnerID { get; set; }
    public string PartnerName { get; set; } = string.Empty;
    public string CommunicationType { get; set; } = "InformationRequest";
    public string Direction { get; set; } = "Outbound";
    public string Channel { get; set; } = "Email";
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = "Sent";
    public DateTime SentDate { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedDate { get; set; }

    public Case? Case { get; set; }
    public PartnerDirectory? Partner { get; set; }
}
