namespace EnterpriseFraudRiskSystem.Models;

public class DailyStatistic
{
    public int StatID { get; set; }
    public DateTime StatDate { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal FraudVolume { get; set; }
    public int FraudCount { get; set; }
}
