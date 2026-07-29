USE EnterpriseFraudRiskDB;
GO

/* Total number of transactions for a customer across all their accounts */
CREATE OR ALTER FUNCTION fn_TotalTransactions (@CustomerId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Total INT;
    SELECT @Total = COUNT(*)
    FROM Transactions t
    INNER JOIN Accounts a ON a.AccountId = t.AccountId
    WHERE a.CustomerId = @CustomerId;
    RETURN ISNULL(@Total, 0);
END
GO

/* Count of failed logins for a user in the last 24 hours */
CREATE OR ALTER FUNCTION fn_FailedLoginCount (@UserId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*)
    FROM LoginHistory
    WHERE UserId = @UserId
      AND IsSuccessful = 0
      AND AttemptedAt >= DATEADD(HOUR, -24, SYSUTCDATETIME());
    RETURN ISNULL(@Count, 0);
END
GO

/* Count of transactions over $5000 for a customer in the last 30 days */
CREATE OR ALTER FUNCTION fn_HighValueTransactionCount (@CustomerId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = COUNT(*)
    FROM Transactions t
    INNER JOIN Accounts a ON a.AccountId = t.AccountId
    WHERE a.CustomerId = @CustomerId
      AND t.Amount >= 5000
      AND t.TransactionAt >= DATEADD(DAY, -30, SYSUTCDATETIME());
    RETURN ISNULL(@Count, 0);
END
GO

/* Composite 0-100 risk score for a customer, blending velocity, value, and alert history */
CREATE OR ALTER FUNCTION fn_CustomerRiskScore (@CustomerId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;
    DECLARE @OpenAlerts INT;
    DECLARE @HighValueCount INT;
    DECLARE @IsBlacklisted BIT;
    DECLARE @TxCountLast24h INT;

    SELECT @IsBlacklisted = IsBlacklisted FROM Customers WHERE CustomerId = @CustomerId;

    SELECT @OpenAlerts = COUNT(*)
    FROM FraudAlerts
    WHERE CustomerId = @CustomerId AND Status IN ('Open','UnderReview');

    SET @HighValueCount = dbo.fn_HighValueTransactionCount(@CustomerId);

    SELECT @TxCountLast24h = COUNT(*)
    FROM Transactions t
    INNER JOIN Accounts a ON a.AccountId = t.AccountId
    WHERE a.CustomerId = @CustomerId
      AND t.TransactionAt >= DATEADD(HOUR, -24, SYSUTCDATETIME());

    SET @Score = (@OpenAlerts * 15) + (@HighValueCount * 5) + (CASE WHEN @TxCountLast24h > 10 THEN 20 ELSE 0 END);

    IF (@IsBlacklisted = 1) SET @Score = 100;
    IF (@Score > 100) SET @Score = 100;

    RETURN @Score;
END
GO
