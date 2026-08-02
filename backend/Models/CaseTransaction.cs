namespace EnterpriseFraudRiskSystem.Models;

public class CaseTransaction
{
    public int CaseTransactionID { get; set; }
    public int CaseID { get; set; }
    public long TransactionID { get; set; }
    public DateTime LinkedDate { get; set; } = DateTime.UtcNow;

    public Case? Case { get; set; }
    public Transaction? Transaction { get; set; }
}
