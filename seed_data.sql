-- =====================================================================
-- File: database/seed_data.sql
-- Populates: 50 customers, ~75 accounts, ~75 cards, 10 merchants,
--            ~3,000 transactions (incl. a deliberate velocity-fraud
--            burst + high-value + foreign-country outliers),
--            ~300 login records (incl. a failed-login burst), blacklist.
-- Idempotent: clears and reseeds identities each run.
-- =====================================================================

USE FraudRiskDB;
GO
SET NOCOUNT ON;
GO

DELETE FROM dbo.AuditLog;
DELETE FROM dbo.FraudAlerts;
DELETE FROM dbo.RiskScores;
DELETE FROM dbo.Blacklist;
DELETE FROM dbo.LoginHistory;
DELETE FROM dbo.Transactions;
DELETE FROM dbo.Cards;
DELETE FROM dbo.Accounts;
DELETE FROM dbo.Merchants;
DELETE FROM dbo.Customers;

DBCC CHECKIDENT ('dbo.Customers', RESEED, 0);
DBCC CHECKIDENT ('dbo.Accounts', RESEED, 0);
DBCC CHECKIDENT ('dbo.Cards', RESEED, 0);
DBCC CHECKIDENT ('dbo.Merchants', RESEED, 0);
DBCC CHECKIDENT ('dbo.Transactions', RESEED, 0);
DBCC CHECKIDENT ('dbo.LoginHistory', RESEED, 0);
DBCC CHECKIDENT ('dbo.FraudAlerts', RESEED, 0);
DBCC CHECKIDENT ('dbo.RiskScores', RESEED, 0);
DBCC CHECKIDENT ('dbo.AuditLog', RESEED, 0);
GO

-- ---------------------------------------------------------------------
-- 1. Merchants (MerchantID 1-10; note IDs 4,6,9 are High risk on purpose)
-- ---------------------------------------------------------------------
INSERT INTO dbo.Merchants (MerchantName, Category, CountryCode, RiskLevel) VALUES
('Amazon Retail',            'E-commerce',       'US', 'Low'),      -- 1
('BestBuy Electronics',      'Electronics',      'US', 'Low'),      -- 2
('QuickCash ATM Network',    'Cash Withdrawal',  'US', 'Medium'),   -- 3
('Global Wire Transfer Co',  'Money Transfer',   'GB', 'High'),     -- 4
('Luxury Watches Intl',      'Luxury Goods',     'CH', 'High'),     -- 5
('CryptoXchange',            'Cryptocurrency',   'SC', 'High'),     -- 6
('Local Grocery Mart',       'Groceries',        'US', 'Low'),      -- 7
('FastFuel Gas Station',     'Fuel',             'US', 'Low'),      -- 8
('OffshoreBet Gaming',       'Gambling',         'MT', 'High'),     -- 9
('CityTransit Pass',         'Transportation',   'US', 'Low');      -- 10
GO

-- ---------------------------------------------------------------------
-- 2. Customers (50)
-- ---------------------------------------------------------------------
DECLARE @firstNames TABLE (Name NVARCHAR(50));
INSERT INTO @firstNames VALUES ('James'),('Mary'),('Robert'),('Patricia'),('John'),('Linda'),('Michael'),('Barbara'),('David'),('Elizabeth');
DECLARE @lastNames TABLE (Name NVARCHAR(50));
INSERT INTO @lastNames VALUES ('Smith'),('Johnson'),('Williams'),('Brown'),('Jones'),('Garcia'),('Miller'),('Davis'),('Rodriguez'),('Martinez');

DECLARE @i INT = 1;
WHILE @i <= 50
BEGIN
    DECLARE @fn NVARCHAR(50) = (SELECT TOP 1 Name FROM @firstNames ORDER BY NEWID());
    DECLARE @ln NVARCHAR(50) = (SELECT TOP 1 Name FROM @lastNames ORDER BY NEWID());
    INSERT INTO dbo.Customers (FullName, DateOfBirth, Email, Phone, HomeCountry, KYCStatus)
    VALUES (
        @fn + ' ' + @ln,
        DATEADD(YEAR, -1 * (20 + (@i % 40)), CAST(SYSUTCDATETIME() AS DATE)),
        LOWER(@fn) + '.' + LOWER(@ln) + CAST(@i AS VARCHAR(10)) + '@example.com',
        '+1-555-' + RIGHT('0000' + CAST(1000 + @i AS VARCHAR(10)), 4),
        'US',
        'Verified'
    );
    SET @i += 1;
END
GO

-- ---------------------------------------------------------------------
-- 3. Accounts (1 per customer, plus a 2nd for even-numbered customers)
-- ---------------------------------------------------------------------
DECLARE @c INT = 1;
WHILE @c <= 50
BEGIN
    INSERT INTO dbo.Accounts (CustomerID, AccountNumber, AccountType, Balance, Status)
    VALUES (
        @c,
        'ACCT' + RIGHT('000000' + CAST(@c AS VARCHAR(10)), 6),
        CASE WHEN @c % 3 = 0 THEN 'Savings' WHEN @c % 3 = 1 THEN 'Checking' ELSE 'Credit' END,
        500 + (@c * 37) % 5000,
        'Active'
    );

    IF @c % 2 = 0
    BEGIN
        INSERT INTO dbo.Accounts (CustomerID, AccountNumber, AccountType, Balance, Status)
        VALUES (
            @c,
            'ACCT' + RIGHT('000000' + CAST(@c AS VARCHAR(10)), 6) + 'B',
            'Savings',
            1000 + (@c * 19) % 3000,
            'Active'
        );
    END
    SET @c += 1;
END
GO

-- ---------------------------------------------------------------------
-- 4. Cards (one per account)
-- ---------------------------------------------------------------------
DECLARE @a INT;
DECLARE acct_cursor CURSOR LOCAL FAST_FORWARD FOR SELECT AccountID FROM dbo.Accounts;
OPEN acct_cursor;
FETCH NEXT FROM acct_cursor INTO @a;
WHILE @@FETCH_STATUS = 0
BEGIN
    INSERT INTO dbo.Cards (AccountID, CardNumberMasked, CardType, ExpiryDate, Status)
    VALUES (
        @a,
        '4111-XXXX-XXXX-' + RIGHT('0000' + CAST(@a AS VARCHAR(10)), 4),
        CASE WHEN @a % 2 = 0 THEN 'Debit' ELSE 'Credit' END,
        DATEADD(YEAR, 3, CAST(SYSUTCDATETIME() AS DATE)),
        'Active'
    );
    FETCH NEXT FROM acct_cursor INTO @a;
END
CLOSE acct_cursor;
DEALLOCATE acct_cursor;
GO

-- ---------------------------------------------------------------------
-- 5. Transactions (~3,000 baseline, spread over the last 90 days)
--    ~0.5% deliberately high-value (>$6,000) and ~0.7% deliberately
--    foreign-country (RU) so the fraud rules in procedures.sql have
--    real signal to catch when you run usp_RecordTransaction/reports.
-- ---------------------------------------------------------------------
DECLARE @maxAccountID INT = (SELECT MAX(AccountID) FROM dbo.Accounts);
DECLARE @maxMerchantID INT = (SELECT MAX(MerchantID) FROM dbo.Merchants);
DECLARE @t INT = 1;

WHILE @t <= 3000
BEGIN
    DECLARE @accID INT = 1 + (ABS(CHECKSUM(NEWID())) % @maxAccountID);
    DECLARE @merchID INT = 1 + (ABS(CHECKSUM(NEWID())) % @maxMerchantID);
    DECLARE @cardID INT = (SELECT TOP 1 CardID FROM dbo.Cards WHERE AccountID = @accID ORDER BY NEWID());
    DECLARE @amt DECIMAL(18,2) = CASE WHEN @t % 200 = 0 THEN 6000 + (ABS(CHECKSUM(NEWID())) % 4000)
                                       ELSE 10 + (ABS(CHECKSUM(NEWID())) % 490) END;
    DECLARE @txnCountry CHAR(2) = CASE WHEN @t % 140 = 0 THEN 'RU' ELSE 'US' END;
    DECLARE @daysBack INT = ABS(CHECKSUM(NEWID())) % 90;
    DECLARE @txnDate DATETIME2 = DATEADD(MINUTE, -1 * (ABS(CHECKSUM(NEWID())) % 1440), DATEADD(DAY, -1 * @daysBack, SYSUTCDATETIME()));
    DECLARE @channel NVARCHAR(20) = CASE WHEN @merchID = 3 THEN 'ATM' WHEN @merchID IN (4,6) THEN 'Transfer' ELSE 'POS' END;

    INSERT INTO dbo.Transactions (AccountID, CardID, MerchantID, Amount, TransactionCountry, Channel, TransactionDate, Status)
    VALUES (@accID, @cardID, @merchID, @amt, @txnCountry, @channel, @txnDate, 'Approved');

    SET @t += 1;
END
GO

-- Deliberate velocity-fraud burst: AccountID 5, 8 transactions inside
-- a 7-minute window, right now, so usp_RecordTransaction's velocity
-- check has something live to trip during a demo.
DECLARE @burstCard INT = (SELECT TOP 1 CardID FROM dbo.Cards WHERE AccountID = 5);
DECLARE @burst INT = 0;
WHILE @burst < 8
BEGIN
    INSERT INTO dbo.Transactions (AccountID, CardID, MerchantID, Amount, TransactionCountry, Channel, TransactionDate, Status)
    VALUES (5, @burstCard, 1, 45.00, 'US', 'Online', DATEADD(MINUTE, -1 * @burst, SYSUTCDATETIME()), 'Approved');
    SET @burst += 1;
END
GO

-- ---------------------------------------------------------------------
-- 6. LoginHistory (~300 successful, plus a deliberate failed-login
--    burst for CustomerID 7 to demo account-takeover detection)
-- ---------------------------------------------------------------------
DECLARE @maxCustomerID INT = (SELECT MAX(CustomerID) FROM dbo.Customers);
DECLARE @l INT = 1;
WHILE @l <= 300
BEGIN
    DECLARE @custID INT = 1 + (ABS(CHECKSUM(NEWID())) % @maxCustomerID);
    INSERT INTO dbo.LoginHistory (CustomerID, LoginTime, IPAddress, DeviceID, Country, Success)
    VALUES (
        @custID,
        DATEADD(MINUTE, -1 * (ABS(CHECKSUM(NEWID())) % 129600), SYSUTCDATETIME()), -- up to 90 days back
        CONCAT(ABS(CHECKSUM(NEWID())) % 255, '.', ABS(CHECKSUM(NEWID())) % 255, '.0.1'),
        'DEV-' + CAST(1000 + (ABS(CHECKSUM(NEWID())) % 400) AS VARCHAR(10)),
        'US',
        1
    );
    SET @l += 1;
END
GO

DECLARE @f INT = 0;
WHILE @f < 6
BEGIN
    INSERT INTO dbo.LoginHistory (CustomerID, LoginTime, IPAddress, DeviceID, Country, Success)
    VALUES (7, DATEADD(MINUTE, -1 * @f, SYSUTCDATETIME()), '203.0.113.55', 'DEV-UNKNOWN', 'NG', 0);
    SET @f += 1;
END
GO

-- ---------------------------------------------------------------------
-- 7. Blacklist (2 customers, for the blacklist fraud rule demo)
-- ---------------------------------------------------------------------
INSERT INTO dbo.Blacklist (CustomerID, Reason) VALUES
(12, 'Confirmed identity-theft victimizer, reported by partner bank'),
(31, 'Repeated chargeback abuse across multiple accounts');
GO

-- ---------------------------------------------------------------------
-- 8. Baseline RiskScores row per customer (usp_CalculateRiskScore updates these)
-- ---------------------------------------------------------------------
INSERT INTO dbo.RiskScores (CustomerID, Score)
SELECT CustomerID, 0 FROM dbo.Customers;
GO

PRINT 'seed_data.sql executed successfully.';
GO
