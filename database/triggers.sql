USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   trg_Transactions_FraudDetection
   Fires AFTER INSERT on Transactions. Evaluates each newly inserted row
   against six fraud rules and writes a FraudAlerts row for every rule
   that trips. Also flags the transaction itself and triggers a risk
   score recalculation for the owning customer.
   ===================================================================== */
CREATE OR ALTER TRIGGER trg_Transactions_FraudDetection
ON Transactions
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @TransactionId BIGINT, @AccountId INT, @CardId INT, @MerchantId INT,
            @Amount DECIMAL(18,2), @Country NVARCHAR(100), @TransactionAt DATETIME2,
            @CustomerId INT;

    DECLARE tx_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TransactionId, AccountId, CardId, MerchantId, Amount, Country, TransactionAt
        FROM inserted;

    OPEN tx_cursor;
    FETCH NEXT FROM tx_cursor INTO @TransactionId, @AccountId, @CardId, @MerchantId, @Amount, @Country, @TransactionAt;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @CustomerId = CustomerId FROM Accounts WHERE AccountId = @AccountId;

        -- Rule 1: High Value Transfer (>= 10,000)
        IF (@Amount >= 10000)
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'HighValue', 'High', 'Transaction amount exceeds high-value threshold of 10,000.');

        -- Rule 2: Velocity Fraud (> 5 transactions on the same account in the last 10 minutes)
        IF (SELECT COUNT(*) FROM Transactions
            WHERE AccountId = @AccountId
              AND TransactionAt >= DATEADD(MINUTE, -10, @TransactionAt)) > 5
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'Velocity', 'High', 'More than 5 transactions on this account within 10 minutes.');

        -- Rule 3: Duplicate Transaction (same account, merchant, amount within 2 minutes)
        IF (SELECT COUNT(*) FROM Transactions
            WHERE AccountId = @AccountId
              AND MerchantId = @MerchantId
              AND Amount = @Amount
              AND TransactionId <> @TransactionId
              AND TransactionAt >= DATEADD(MINUTE, -2, @TransactionAt)) > 0
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'Duplicate', 'Medium', 'Matching amount/merchant transaction on the same account within 2 minutes.');

        -- Rule 4: Foreign Transaction (transaction country differs from customer's home country)
        IF EXISTS (SELECT 1 FROM Customers WHERE CustomerId = @CustomerId AND Country <> @Country)
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'Foreign', 'Medium', 'Transaction country differs from customer home country.');

        -- Rule 5: Blacklisted Customer
        IF EXISTS (SELECT 1 FROM Customers WHERE CustomerId = @CustomerId AND IsBlacklisted = 1)
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'Blacklisted', 'Critical', 'Transaction placed by a blacklisted customer.');

        -- Rule 6: Blocked Card
        IF (@CardId IS NOT NULL AND EXISTS (SELECT 1 FROM Cards WHERE CardId = @CardId AND Status = 'Blocked'))
            INSERT INTO FraudAlerts (TransactionId, CustomerId, AlertType, Severity, Description)
            VALUES (@TransactionId, @CustomerId, 'BlockedCard', 'Critical', 'Transaction attempted on a blocked card.');

        -- Mark the transaction Flagged if any alert was raised for it
        IF EXISTS (SELECT 1 FROM FraudAlerts WHERE TransactionId = @TransactionId)
            UPDATE Transactions SET Status = 'Flagged' WHERE TransactionId = @TransactionId;

        -- Recalculate the customer's risk score after any new activity
        EXEC usp_UpdateRiskScore @CustomerId = @CustomerId;

        FETCH NEXT FROM tx_cursor INTO @TransactionId, @AccountId, @CardId, @MerchantId, @Amount, @Country, @TransactionAt;
    END

    CLOSE tx_cursor;
    DEALLOCATE tx_cursor;
END
GO

/* =====================================================================
   Audit triggers — one per audited table, all writing to AuditLog.
   ===================================================================== */
CREATE OR ALTER TRIGGER trg_Customers_Audit
ON Customers
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLog (EntityName, EntityId, Action, Details)
    SELECT 'Customers', CAST(CustomerId AS NVARCHAR(50)), 'INSERT', CONCAT('Customer created: ', FirstName, ' ', LastName)
    FROM inserted i WHERE NOT EXISTS (SELECT 1 FROM deleted d WHERE d.CustomerId = i.CustomerId);

    INSERT INTO AuditLog (EntityName, EntityId, Action, Details)
    SELECT 'Customers', CAST(i.CustomerId AS NVARCHAR(50)), 'UPDATE', 'Customer record updated'
    FROM inserted i INNER JOIN deleted d ON d.CustomerId = i.CustomerId;

    INSERT INTO AuditLog (EntityName, EntityId, Action, Details)
    SELECT 'Customers', CAST(CustomerId AS NVARCHAR(50)), 'DELETE', CONCAT('Customer removed: ', FirstName, ' ', LastName)
    FROM deleted d WHERE NOT EXISTS (SELECT 1 FROM inserted i WHERE i.CustomerId = d.CustomerId);
END
GO

CREATE OR ALTER TRIGGER trg_Transactions_Audit
ON Transactions
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLog (EntityName, EntityId, Action, Details)
    SELECT 'Transactions', CAST(TransactionId AS NVARCHAR(50)), 'INSERT', CONCAT('Transaction recorded: ', Amount, ' ', Currency)
    FROM inserted;
END
GO

CREATE OR ALTER TRIGGER trg_FraudAlerts_Audit
ON FraudAlerts
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLog (EntityName, EntityId, Action, Details)
    SELECT 'FraudAlerts', CAST(FraudAlertId AS NVARCHAR(50)),
           CASE WHEN EXISTS (SELECT 1 FROM deleted d WHERE d.FraudAlertId = i.FraudAlertId) THEN 'UPDATE' ELSE 'INSERT' END,
           CONCAT('Alert type: ', AlertType, ', Severity: ', Severity, ', Status: ', Status)
    FROM inserted i;
END
GO
