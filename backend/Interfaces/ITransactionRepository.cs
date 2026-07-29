using EnterpriseFraudRiskSystem.Models;

namespace EnterpriseFraudRiskSystem.Interfaces;

public interface ITransactionRepository
{
    Task<long> RecordTransactionAsync(Transaction transaction);
    Task<List<Transaction>> GetRecentAsync(int count);
    Task<List<FraudAlert>> GetAlertsForTransactionAsync(long transactionId);
}
