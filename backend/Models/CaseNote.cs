namespace EnterpriseFraudRiskSystem.Models;

public class CaseNote
{
    public int NoteID { get; set; }
    public int CaseID { get; set; }
    public int AnalystID { get; set; }
    public string NoteType { get; set; } = "InvestigationNote";
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
    public User? Analyst { get; set; }
}
