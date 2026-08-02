namespace EnterpriseFraudRiskSystem.Models;

public class Evidence
{
    public int EvidenceID { get; set; }
    public int CustomerID { get; set; }
    public int? SessionID { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? FileLocation { get; set; }
    public int UploadedBy { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public InvestigationSession? Session { get; set; }
    public User? UploadedByUser { get; set; }
}
