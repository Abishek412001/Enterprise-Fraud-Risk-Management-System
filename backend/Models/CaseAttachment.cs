namespace EnterpriseFraudRiskSystem.Models;

public class CaseAttachment
{
    public int AttachmentID { get; set; }
    public int CaseID { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int UploadedBy { get; set; }
    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
    public User? UploadedByUser { get; set; }
}
