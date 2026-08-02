USE EnterpriseFraudRiskDB;
GO

-- Monthly fraud trend
SELECT FORMAT(CreatedAt, 'yyyy-MM') AS Month, COUNT(*) AS AlertCount
FROM FraudAlerts
GROUP BY FORMAT(CreatedAt, 'yyyy-MM')
ORDER BY Month;
GO

-- Country-wise fraud distribution
SELECT t.Country, COUNT(*) AS FlaggedCount
FROM Transactions t
WHERE t.Status = 'Flagged'
GROUP BY t.Country
ORDER BY FlaggedCount DESC;
GO

-- Merchant-wise fraud distribution
SELECT m.MerchantName, COUNT(*) AS FlaggedCount
FROM Transactions t
INNER JOIN Merchants m ON m.MerchantId = t.MerchantId
WHERE t.Status = 'Flagged'
GROUP BY m.MerchantName
ORDER BY FlaggedCount DESC;
GO

-- Top 10 customers by transaction volume
SELECT TOP 10 c.CustomerId, c.FirstName, c.LastName, COUNT(t.TransactionId) AS TxCount, SUM(t.Amount) AS TotalAmount
FROM Customers c
INNER JOIN Accounts a ON a.CustomerId = c.CustomerId
INNER JOIN Transactions t ON t.AccountId = a.AccountId
GROUP BY c.CustomerId, c.FirstName, c.LastName
ORDER BY TotalAmount DESC;
GO

-- Daily fraud report (parametrized by @ReportDate)
DECLARE @ReportDate DATE = CAST(GETUTCDATE() AS DATE);
SELECT * FROM FraudAlerts WHERE CAST(CreatedAt AS DATE) = @ReportDate;
GO
