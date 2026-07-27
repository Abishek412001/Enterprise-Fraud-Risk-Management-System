-- =====================================================================
-- File: database/procedures.sql
-- Core stored procedures. usp_RecordTransaction is where the fraud
-- rules live: blacklist check, high-value check, velocity check,
-- and foreign-country check. Every write path uses TRY/CATCH with
-- explicit COMMIT/ROLLBACK.
-- =====================================================================
USE FraudRiskDB;
GO

IF OBJECT_ID('dbo.usp_CreateCustomer', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_CreateCustomer;
GO
IF OBJECT_ID('dbo.usp_RecordTransaction', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_RecordTransaction;
GO
IF OBJECT_ID('dbo.usp_CalculateRiskScore', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_CalculateRiskScore;
GO
IF OBJECT_ID('dbo.usp_BlockCard', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_BlockCard;
GO
IF OBJECT_ID('dbo.usp_ResolveFraudCase', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_ResolveFraudCase;
GO
IF OBJECT_ID('dbo.usp_DailyFraudSummary', 'P') IS NOT NULL DROP PROCEDURE dbo.usp_DailyFraudSummary;
GO

-- ---------------------------------------------------------------------
-- usp_CreateCustomer
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_CreateCustomer
    @FullName    NVARCHAR(100),
    @DateOfBirth DATE,
    @Email       NVARCHAR(150),
    @Phone       NVARCHAR(20),
    @HomeCountry CHAR(2) = 'US'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.Customers WHERE Email = @Email)
    BEGIN
        RAISERROR('A customer with this email already exists.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO dbo.Customers (FullName, DateOfBirth, Email, Phone, HomeCountry)
        VALUES (@FullName, @DateOfBirth, @Email, @Phone, @HomeCountry);

        DECLARE @NewCustomerID INT = SCOPE_IDENTITY();

        INSERT INTO dbo.RiskScores (CustomerID, Score) VALUES (@NewCustomerID, 0);

        COMMIT TRANSACTION;
        SELECT @NewCustomerID AS NewCustomerID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- usp_RecordTransaction
-- Fraud rules applied inline, each raising a FraudAlerts row:
--   1. Blacklisted customer            -> High severity, txn Flagged
--   2. High-value transaction (>5000)  -> Medium/High severity
--   3. Velocity (>=5 txns / 10 min)    -> High severity, txn Flagged
--   4. Foreign country (<> HomeCountry)-> Medium severity
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_RecordTransaction
    @AccountID          INT,
    @CardID             INT = NULL,
    @MerchantID          INT,
    @Amount             DECIMAL(18,2),
    @TransactionCountry CHAR(2) = 'US',
    @Channel            NVARCHAR(20) = 'POS'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Accounts WHERE AccountID = @AccountID AND Status = 'Active')
    BEGIN
        RAISERROR('Account does not exist or is not Active.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @CustomerID INT = (SELECT CustomerID FROM dbo.Accounts WHERE AccountID = @AccountID);
        DECLARE @HomeCountry CHAR(2) = (SELECT HomeCountry FROM dbo.Customers WHERE CustomerID = @CustomerID);
        DECLARE @IsBlacklisted BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.Blacklist WHERE CustomerID = @CustomerID) THEN 1 ELSE 0 END;
        DECLARE @FinalStatus NVARCHAR(20) = 'Approved';

        INSERT INTO dbo.Transactions (AccountID, CardID, MerchantID, Amount, TransactionCountry, Channel, Status)
        VALUES (@AccountID, @CardID, @MerchantID, @Amount, @TransactionCountry, @Channel, 'Approved');

        DECLARE @TransactionID BIGINT = SCOPE_IDENTITY();

        -- Rule 1: Blacklist
        IF @IsBlacklisted = 1
        BEGIN
            INSERT INTO dbo.FraudAlerts (TransactionID, CustomerID, AlertType, Severity)
            VALUES (@TransactionID, @CustomerID, 'BlacklistedCustomer', 'High');
            SET @FinalStatus = 'Flagged';
        END

        -- Rule 2: High-value transaction
        IF @Amount > 5000
        BEGIN
            INSERT INTO dbo.FraudAlerts (TransactionID, CustomerID, AlertType, Severity)
            VALUES (@TransactionID, @CustomerID, 'HighValueTransaction',
                    CASE WHEN @Amount > 9000 THEN 'High' ELSE 'Medium' END);
            IF @Amount > 9000 SET @FinalStatus = 'Flagged';
        END

        -- Rule 3: Velocity — 5+ transactions on this account in the last 10 minutes
        IF dbo.fnVelocityCount(@AccountID, 10) >= 5
        BEGIN
            INSERT INTO dbo.FraudAlerts (TransactionID, CustomerID, AlertType, Severity)
            VALUES (@TransactionID, @CustomerID, 'VelocityFraud', 'High');
            SET @FinalStatus = 'Flagged';
        END

        -- Rule 4: Foreign-country transaction relative to home country
        IF @TransactionCountry <> @HomeCountry
        BEGIN
            INSERT INTO dbo.FraudAlerts (TransactionID, CustomerID, AlertType, Severity)
            VALUES (@TransactionID, @CustomerID, 'ForeignCountryTransaction', 'Medium');
        END

        IF @FinalStatus <> 'Approved'
        BEGIN
            UPDATE dbo.Transactions SET Status = @FinalStatus WHERE TransactionID = @TransactionID;
        END

        COMMIT TRANSACTION;
        SELECT @TransactionID AS TransactionID, @FinalStatus AS FinalStatus;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- usp_CalculateRiskScore
-- Weighted score from open alerts (last 90 days) + blacklist flag,
-- upserted via MERGE. Clipped to 0-100 by the RiskScores CHECK constraint.
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_CalculateRiskScore
    @CustomerID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DECLARE @HighAlerts   INT = (SELECT COUNT(*) FROM dbo.FraudAlerts WHERE CustomerID = @CustomerID AND Severity = 'High'   AND CreatedAt >= DATEADD(DAY, -90, SYSUTCDATETIME()));
        DECLARE @MedAlerts    INT = (SELECT COUNT(*) FROM dbo.FraudAlerts WHERE CustomerID = @CustomerID AND Severity = 'Medium' AND CreatedAt >= DATEADD(DAY, -90, SYSUTCDATETIME()));
        DECLARE @LowAlerts    INT = (SELECT COUNT(*) FROM dbo.FraudAlerts WHERE CustomerID = @CustomerID AND Severity = 'Low'    AND CreatedAt >= DATEADD(DAY, -90, SYSUTCDATETIME()));
        DECLARE @IsBlacklisted BIT = CASE WHEN EXISTS (SELECT 1 FROM dbo.Blacklist WHERE CustomerID = @CustomerID) THEN 1 ELSE 0 END;

        DECLARE @Score INT = (@HighAlerts * 20) + (@MedAlerts * 8) + (@LowAlerts * 3) + (@IsBlacklisted * 40);
        IF @Score > 100 SET @Score = 100;

        MERGE dbo.RiskScores AS target
        USING (SELECT @CustomerID AS CustomerID, @Score AS Score) AS src
        ON target.CustomerID = src.CustomerID
        WHEN MATCHED THEN
            UPDATE SET Score = src.Score, LastUpdated = SYSUTCDATETIME()
        WHEN NOT MATCHED THEN
            INSERT (CustomerID, Score, LastUpdated) VALUES (src.CustomerID, src.Score, SYSUTCDATETIME());

        COMMIT TRANSACTION;
        SELECT @CustomerID AS CustomerID, @Score AS NewScore;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- usp_BlockCard
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_BlockCard
    @CardID INT,
    @Reason NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Cards WHERE CardID = @CardID)
    BEGIN
        RAISERROR('CardID not found.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Cards SET Status = 'Blocked' WHERE CardID = @CardID AND Status <> 'Blocked';

        INSERT INTO dbo.AuditLog (TableName, Operation, RecordID, Details)
        VALUES ('Cards', 'BLOCK', @CardID, @Reason);

        COMMIT TRANSACTION;
        SELECT @CardID AS CardID, 'Blocked' AS NewStatus;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- usp_ResolveFraudCase
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_ResolveFraudCase
    @AlertID     BIGINT,
    @Resolution  NVARCHAR(20)  -- 'Resolved' or 'FalsePositive'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @Resolution NOT IN ('Resolved','FalsePositive')
    BEGIN
        RAISERROR('Resolution must be Resolved or FalsePositive.', 16, 1);
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.FraudAlerts WHERE AlertID = @AlertID)
    BEGIN
        RAISERROR('AlertID not found.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.FraudAlerts SET Status = @Resolution WHERE AlertID = @AlertID;

        INSERT INTO dbo.AuditLog (TableName, Operation, RecordID, Details)
        VALUES ('FraudAlerts', 'RESOLVE', @AlertID, @Resolution);

        COMMIT TRANSACTION;
        SELECT @AlertID AS AlertID, @Resolution AS NewStatus;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- ---------------------------------------------------------------------
-- usp_DailyFraudSummary
-- ---------------------------------------------------------------------
CREATE PROCEDURE dbo.usp_DailyFraudSummary
    @Date DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @Date IS NULL SET @Date = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        @Date AS ReportDate,
        (SELECT COUNT(*) FROM dbo.FraudAlerts WHERE CAST(CreatedAt AS DATE) = @Date) AS DailyFraudCount,
        (SELECT COUNT(*) FROM dbo.RiskScores WHERE Score >= 70) AS HighRiskCustomers,
        (SELECT COUNT(*) FROM dbo.Cards WHERE Status = 'Blocked') AS BlockedCards;
END
GO

PRINT 'procedures.sql executed successfully.';
GO

-- =====================================================================
-- Output validation examples (run after seed_data.sql + views.sql)
-- =====================================================================
-- EXEC dbo.usp_RecordTransaction @AccountID = 5, @MerchantID = 1, @Amount = 45.00, @TransactionCountry = 'US', @Channel = 'Online';
--   Expected: 1 row inserted into Transactions, likely FinalStatus = 'Flagged'
--   (AccountID 5 already has 8 recent txns from the seeded velocity burst)
--   plus a new row in FraudAlerts with AlertType = 'VelocityFraud'.
--
-- EXEC dbo.usp_CalculateRiskScore @CustomerID = 12;
--   Expected: CustomerID 12 is seeded blacklisted -> NewScore >= 40
--
-- EXEC dbo.usp_BlockCard @CardID = 3, @Reason = 'Suspected card testing';
--   Expected: 1 row updated, AuditLog gets a new 'BLOCK' entry
--
-- EXEC dbo.usp_DailyFraudSummary;
--   Expected columns: ReportDate | DailyFraudCount | HighRiskCustomers | BlockedCards
--   (exact counts vary run-to-run because seed data uses randomized dates)
