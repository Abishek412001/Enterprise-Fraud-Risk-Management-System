-- =====================================================================
-- File: database/schema.sql
-- Enterprise Fraud Risk Management System (Lite)
-- Target: Microsoft SQL Server 2022
-- Purpose: Core banking + fraud schema, sized for fast local demo runs
-- =====================================================================

IF DB_ID('FraudRiskDB') IS NULL
BEGIN
    CREATE DATABASE FraudRiskDB;
END
GO

USE FraudRiskDB;
GO

-- ---------------------------------------------------------------------
-- Idempotent teardown (safe to rerun) — drop children before parents
-- ---------------------------------------------------------------------
IF OBJECT_ID('dbo.AuditLog', 'U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID('dbo.RiskScores', 'U') IS NOT NULL DROP TABLE dbo.RiskScores;
IF OBJECT_ID('dbo.FraudAlerts', 'U') IS NOT NULL DROP TABLE dbo.FraudAlerts;
IF OBJECT_ID('dbo.Blacklist', 'U') IS NOT NULL DROP TABLE dbo.Blacklist;
IF OBJECT_ID('dbo.LoginHistory', 'U') IS NOT NULL DROP TABLE dbo.LoginHistory;
IF OBJECT_ID('dbo.Transactions', 'U') IS NOT NULL DROP TABLE dbo.Transactions;
IF OBJECT_ID('dbo.Cards', 'U') IS NOT NULL DROP TABLE dbo.Cards;
IF OBJECT_ID('dbo.Merchants', 'U') IS NOT NULL DROP TABLE dbo.Merchants;
IF OBJECT_ID('dbo.Accounts', 'U') IS NOT NULL DROP TABLE dbo.Accounts;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

-- ---------------------------------------------------------------------
-- Customers  (3NF: no repeating groups, all attrs depend on PK only)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Customers (
    CustomerID      INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)   NOT NULL,
    DateOfBirth     DATE            NOT NULL,
    Email           NVARCHAR(150)   NOT NULL UNIQUE,
    Phone           NVARCHAR(20)    NOT NULL,
    HomeCountry     CHAR(2)         NOT NULL DEFAULT 'US',
    KYCStatus       NVARCHAR(20)    NOT NULL DEFAULT 'Verified'
                        CONSTRAINT CK_Customers_KYC CHECK (KYCStatus IN ('Verified','Pending','Rejected')),
    CreatedAt       DATETIME2       NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ---------------------------------------------------------------------
-- Accounts (1 customer -> many accounts)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Accounts (
    AccountID       INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT             NOT NULL
                        CONSTRAINT FK_Accounts_Customers REFERENCES dbo.Customers(CustomerID),
    AccountNumber   VARCHAR(20)     NOT NULL UNIQUE,
    AccountType     NVARCHAR(20)    NOT NULL
                        CONSTRAINT CK_Accounts_Type CHECK (AccountType IN ('Checking','Savings','Credit')),
    Balance         DECIMAL(18,2)   NOT NULL DEFAULT 0
                        CONSTRAINT CK_Accounts_Balance CHECK (Balance >= -10000),
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Active'
                        CONSTRAINT CK_Accounts_Status CHECK (Status IN ('Active','Frozen','Closed')),
    OpenedDate      DATE            NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE)
);
GO

-- ---------------------------------------------------------------------
-- Merchants
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Merchants (
    MerchantID      INT IDENTITY(1,1) PRIMARY KEY,
    MerchantName    NVARCHAR(100)   NOT NULL,
    Category        NVARCHAR(50)    NOT NULL,
    CountryCode     CHAR(2)         NOT NULL DEFAULT 'US',
    RiskLevel       NVARCHAR(10)    NOT NULL DEFAULT 'Low'
                        CONSTRAINT CK_Merchants_Risk CHECK (RiskLevel IN ('Low','Medium','High'))
);
GO

-- ---------------------------------------------------------------------
-- Cards (1 account -> many cards)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Cards (
    CardID              INT IDENTITY(1,1) PRIMARY KEY,
    AccountID           INT         NOT NULL
                            CONSTRAINT FK_Cards_Accounts REFERENCES dbo.Accounts(AccountID),
    CardNumberMasked    VARCHAR(19) NOT NULL UNIQUE,
    CardType            NVARCHAR(10) NOT NULL
                            CONSTRAINT CK_Cards_Type CHECK (CardType IN ('Debit','Credit')),
    ExpiryDate          DATE        NOT NULL,
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Active'
                            CONSTRAINT CK_Cards_Status CHECK (Status IN ('Active','Blocked','Expired')),
    IssuedDate          DATE        NOT NULL DEFAULT CAST(SYSUTCDATETIME() AS DATE)
);
GO

-- ---------------------------------------------------------------------
-- Transactions (the core fraud-detection surface)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Transactions (
    TransactionID       BIGINT IDENTITY(1,1) PRIMARY KEY,
    AccountID           INT         NOT NULL
                            CONSTRAINT FK_Txn_Accounts REFERENCES dbo.Accounts(AccountID),
    CardID              INT         NULL
                            CONSTRAINT FK_Txn_Cards REFERENCES dbo.Cards(CardID),
    MerchantID          INT         NOT NULL
                            CONSTRAINT FK_Txn_Merchants REFERENCES dbo.Merchants(MerchantID),
    Amount              DECIMAL(18,2) NOT NULL
                            CONSTRAINT CK_Txn_Amount CHECK (Amount > 0),
    TransactionCountry  CHAR(2)     NOT NULL DEFAULT 'US',
    Channel             NVARCHAR(20) NOT NULL DEFAULT 'POS'
                            CONSTRAINT CK_Txn_Channel CHECK (Channel IN ('POS','Online','ATM','Transfer')),
    TransactionDate     DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME(),
    Status              NVARCHAR(20) NOT NULL DEFAULT 'Approved'
                            CONSTRAINT CK_Txn_Status CHECK (Status IN ('Approved','Declined','Flagged'))
);
GO

-- ---------------------------------------------------------------------
-- LoginHistory (used for account-takeover / failed-login rules)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.LoginHistory (
    LoginID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT         NOT NULL
                        CONSTRAINT FK_Login_Customers REFERENCES dbo.Customers(CustomerID),
    LoginTime       DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME(),
    IPAddress       VARCHAR(45) NOT NULL,
    DeviceID        VARCHAR(50) NOT NULL,
    Country         CHAR(2)     NOT NULL DEFAULT 'US',
    Success         BIT         NOT NULL DEFAULT 1
);
GO

-- ---------------------------------------------------------------------
-- Blacklist
-- ---------------------------------------------------------------------
CREATE TABLE dbo.Blacklist (
    BlacklistID     INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT         NOT NULL UNIQUE
                        CONSTRAINT FK_Blacklist_Customers REFERENCES dbo.Customers(CustomerID),
    Reason          NVARCHAR(200) NOT NULL,
    AddedDate       DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ---------------------------------------------------------------------
-- FraudAlerts
-- ---------------------------------------------------------------------
CREATE TABLE dbo.FraudAlerts (
    AlertID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    TransactionID   BIGINT      NULL
                        CONSTRAINT FK_Alerts_Txn REFERENCES dbo.Transactions(TransactionID),
    CustomerID      INT         NOT NULL
                        CONSTRAINT FK_Alerts_Customers REFERENCES dbo.Customers(CustomerID),
    AlertType       NVARCHAR(50) NOT NULL,
    Severity        NVARCHAR(10) NOT NULL DEFAULT 'Medium'
                        CONSTRAINT CK_Alerts_Severity CHECK (Severity IN ('Low','Medium','High')),
    CreatedAt       DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME(),
    Status          NVARCHAR(20) NOT NULL DEFAULT 'Open'
                        CONSTRAINT CK_Alerts_Status CHECK (Status IN ('Open','UnderReview','Resolved','FalsePositive'))
);
GO

-- ---------------------------------------------------------------------
-- RiskScores (1:1 with Customers, computed by usp_CalculateRiskScore)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.RiskScores (
    RiskScoreID     INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID      INT         NOT NULL UNIQUE
                        CONSTRAINT FK_RiskScores_Customers REFERENCES dbo.Customers(CustomerID),
    Score           INT         NOT NULL DEFAULT 0
                        CONSTRAINT CK_RiskScores_Range CHECK (Score BETWEEN 0 AND 100),
    LastUpdated     DATETIME2   NOT NULL DEFAULT SYSUTCDATETIME()
);
GO

-- ---------------------------------------------------------------------
-- AuditLog (generic audit sink, populated by triggers)
-- ---------------------------------------------------------------------
CREATE TABLE dbo.AuditLog (
    AuditID         BIGINT IDENTITY(1,1) PRIMARY KEY,
    TableName       NVARCHAR(50)  NOT NULL,
    Operation       NVARCHAR(20)  NOT NULL,
    RecordID        BIGINT        NULL,
    ChangedBy       NVARCHAR(50)  NOT NULL DEFAULT SUSER_SNAME(),
    ChangedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    Details         NVARCHAR(400) NULL
);
GO

-- ---------------------------------------------------------------------
-- Indexes — covering the access patterns the procs/reports below use
-- ---------------------------------------------------------------------
CREATE NONCLUSTERED INDEX IX_Transactions_AccountID_Date ON dbo.Transactions(AccountID, TransactionDate DESC) INCLUDE (Amount, Status);
CREATE NONCLUSTERED INDEX IX_Transactions_MerchantID ON dbo.Transactions(MerchantID);
CREATE NONCLUSTERED INDEX IX_LoginHistory_CustomerID_Time ON dbo.LoginHistory(CustomerID, LoginTime DESC);
CREATE NONCLUSTERED INDEX IX_FraudAlerts_CustomerID_Severity ON dbo.FraudAlerts(CustomerID, Severity) INCLUDE (Status, CreatedAt);
CREATE NONCLUSTERED INDEX IX_FraudAlerts_HighOpen ON dbo.FraudAlerts(CustomerID) INCLUDE (Severity, Status)
    WHERE Severity = 'High' AND Status = 'Open'; -- filtered index example
GO

PRINT 'schema.sql executed successfully.';
GO
