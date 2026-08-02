USE EnterpriseFraudRiskDB;
GO

CREATE OR ALTER VIEW vw_HighRiskCustomers AS
SELECT c.CustomerId, c.FirstName, c.LastName, c.Email, c.Country,
       r.Score, r.RiskLevel, r.LastCalculatedAt
FROM Customers c
INNER JOIN CustomerRiskScore r ON r.CustomerId = c.CustomerId
WHERE r.RiskLevel IN ('High','Critical');
GO

CREATE OR ALTER VIEW vw_FraudSummary AS
SELECT fa.AlertType, fa.Severity, fa.Status, COUNT(*) AS AlertCount,
       CAST(fa.CreatedAt AS DATE) AS AlertDate
FROM FraudAlerts fa
GROUP BY fa.AlertType, fa.Severity, fa.Status, CAST(fa.CreatedAt AS DATE);
GO

CREATE OR ALTER VIEW vw_DailyTransactions AS
SELECT CAST(t.TransactionAt AS DATE) AS TransactionDate,
       COUNT(*) AS TransactionCount,
       SUM(t.Amount) AS TotalAmount,
       SUM(CASE WHEN t.Status = 'Flagged' THEN 1 ELSE 0 END) AS FlaggedCount
FROM Transactions t
GROUP BY CAST(t.TransactionAt AS DATE);
GO

CREATE OR ALTER VIEW vw_MerchantRisk AS
SELECT m.MerchantId, m.MerchantName, m.MerchantCategory, m.Country, m.RiskLevel,
       COUNT(t.TransactionId) AS TransactionCount,
       SUM(CASE WHEN t.Status = 'Flagged' THEN 1 ELSE 0 END) AS FlaggedCount
FROM Merchants m
LEFT JOIN Transactions t ON t.MerchantId = m.MerchantId
GROUP BY m.MerchantId, m.MerchantName, m.MerchantCategory, m.Country, m.RiskLevel;
GO

CREATE OR ALTER VIEW vw_RecentTransactions AS
SELECT TOP 100 t.TransactionId, t.TransactionAt, t.Amount, t.Currency, t.Status,
       a.AccountNumber, c.FirstName, c.LastName, m.MerchantName
FROM Transactions t
INNER JOIN Accounts a ON a.AccountId = t.AccountId
INNER JOIN Customers c ON c.CustomerId = a.CustomerId
INNER JOIN Merchants m ON m.MerchantId = t.MerchantId
ORDER BY t.TransactionAt DESC;
GO
