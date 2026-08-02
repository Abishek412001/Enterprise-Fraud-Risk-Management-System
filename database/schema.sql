/* =====================================================================
   Enterprise Fraud Risk Management System
   schema.sql - Core normalized schema
   Run this FIRST, before indexes/functions/procedures/triggers/views.
   ===================================================================== */

IF DB_ID('EnterpriseFraudRiskDB') IS NULL
BEGIN
    CREATE DATABASE EnterpriseFraudRiskDB;
END
GO

USE EnterpriseFraudRiskDB;
GO

/* ---------------------------------------------------------------------
   1. Users  (application login accounts — Admin / FraudAnalyst)
   --------------------------------------------------------------------- */
CREATE TABLE Users (
    UserId          INT IDENTITY(1,1) PRIMARY KEY,
    Username        NVARCHAR(50)  NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    PasswordHash    NVARCHAR(255) NOT NULL,   -- BCrypt hash, never plaintext
    Role            NVARCHAR(20)  NOT NULL,   -- 'Admin' | 'FraudAnalyst'
    IsActive        BIT           NOT NULL DEFAULT 1,
    FailedLoginCount INT          NOT NULL DEFAULT 0,
    LastLoginAt     DATETIME2     NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Username UNIQUE (Username),
    CONSTRAINT UQ_Users_Email UNIQUE (Email),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Admin','FraudAnalyst'))
);
GO

/* ---------------------------------------------------------------------
   2. Customers
   --------------------------------------------------------------------- */
CREATE TABLE Customers (
    CustomerId      INT IDENTITY(1,1) PRIMARY KEY,
    FirstName       NVARCHAR(50)  NOT NULL,
    LastName        NVARCHAR(50)  NOT NULL,
    Email           NVARCHAR(150) NOT NULL,
    Phone           NVARCHAR(20)  NOT NULL,
    NationalIdNumber NVARCHAR(50) NOT NULL,
    DateOfBirth     DATE          NOT NULL,
    Address         NVARCHAR(255) NULL,
    City            NVARCHAR(100) NULL,
    Country         NVARCHAR(100) NOT NULL,
    IsBlacklisted   BIT           NOT NULL DEFAULT 0,
    CreatedByUserId INT           NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Customers_Email UNIQUE (Email),
    CONSTRAINT UQ_Customers_NationalId UNIQUE (NationalIdNumber),
    CONSTRAINT FK_Customers_Users FOREIGN KEY (CreatedByUserId) REFERENCES Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   3. Accounts
   --------------------------------------------------------------------- */
CREATE TABLE Accounts (
    AccountId       INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT           NOT NULL,
    AccountNumber   NVARCHAR(34)  NOT NULL,   -- IBAN-length friendly
    AccountType     NVARCHAR(20)  NOT NULL,   -- Savings | Checking | Business
    Currency        CHAR(3)       NOT NULL DEFAULT 'USD',
    Balance         DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Active', -- Active | Frozen | Closed
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Accounts_Number UNIQUE (AccountNumber),
    CONSTRAINT FK_Accounts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT CK_Accounts_Type CHECK (AccountType IN ('Savings','Checking','Business')),
    CONSTRAINT CK_Accounts_Status CHECK (Status IN ('Active','Frozen','Closed')),
    CONSTRAINT CK_Accounts_Balance CHECK (Balance >= 0)
);
GO

/* ---------------------------------------------------------------------
   4. Cards
   --------------------------------------------------------------------- */
CREATE TABLE Cards (
    CardId          INT IDENTITY(1,1) PRIMARY KEY,
    AccountId       INT           NOT NULL,
    CardNumberMasked NVARCHAR(25) NOT NULL,   -- e.g. 4111********1234
    CardNumberHash  NVARCHAR(255) NOT NULL,   -- hashed full PAN, never stored plaintext
    CardType        NVARCHAR(20)  NOT NULL,   -- Debit | Credit
    ExpiryDate      DATE          NOT NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Active', -- Active | Blocked | Replaced | Expired
    IssuedAt        DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Cards_HashNumber UNIQUE (CardNumberHash),
    CONSTRAINT FK_Cards_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT CK_Cards_Type CHECK (CardType IN ('Debit','Credit')),
    CONSTRAINT CK_Cards_Status CHECK (Status IN ('Active','Blocked','Replaced','Expired'))
);
GO

/* ---------------------------------------------------------------------
   5. Merchants
   --------------------------------------------------------------------- */
CREATE TABLE Merchants (
    MerchantId      INT IDENTITY(1,1) PRIMARY KEY,
    MerchantName    NVARCHAR(150) NOT NULL,
    MerchantCategory NVARCHAR(50) NOT NULL,   -- e.g. Electronics, Travel, Grocery
    Country         NVARCHAR(100) NOT NULL,
    RiskLevel       NVARCHAR(20)  NOT NULL DEFAULT 'Low', -- Low | Medium | High
    IsBlacklisted   BIT           NOT NULL DEFAULT 0,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt      DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT CK_Merchants_Risk CHECK (RiskLevel IN ('Low','Medium','High'))
);
GO

/* ---------------------------------------------------------------------
   6. Transactions
   --------------------------------------------------------------------- */
CREATE TABLE Transactions (
    TransactionId   BIGINT IDENTITY(1,1) PRIMARY KEY,
    AccountId       INT           NOT NULL,
    CardId          INT           NULL,
    MerchantId      INT           NOT NULL,
    Amount          DECIMAL(18,2) NOT NULL,
    Currency        CHAR(3)       NOT NULL DEFAULT 'USD',
    Country         NVARCHAR(100) NOT NULL,
    IpAddress       NVARCHAR(45)  NULL,
    Channel         NVARCHAR(20)  NOT NULL,  -- POS | Online | ATM | Mobile
    GpsLatitude     DECIMAL(9,6)  NULL,
    GpsLongitude    DECIMAL(9,6)  NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Approved', -- Approved | Declined | Flagged
    TransactionAt   DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Transactions_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId),
    CONSTRAINT FK_Transactions_Cards FOREIGN KEY (CardId) REFERENCES Cards(CardId),
    CONSTRAINT FK_Transactions_Merchants FOREIGN KEY (MerchantId) REFERENCES Merchants(MerchantId),
    CONSTRAINT CK_Transactions_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Transactions_Channel CHECK (Channel IN ('POS','Online','ATM','Mobile')),
    CONSTRAINT CK_Transactions_Status CHECK (Status IN ('Approved','Declined','Flagged'))
);
GO

/* ---------------------------------------------------------------------
   7. FraudAlerts
   --------------------------------------------------------------------- */
CREATE TABLE FraudAlerts (
    FraudAlertId    INT IDENTITY(1,1) PRIMARY KEY,
    TransactionId   BIGINT        NOT NULL,
    CustomerId      INT           NOT NULL,
    AlertType       NVARCHAR(50)  NOT NULL,  -- Velocity | HighValue | Duplicate | Foreign | Blacklisted | BlockedCard
    Severity        NVARCHAR(20)  NOT NULL DEFAULT 'Medium', -- Low | Medium | High | Critical
    Description     NVARCHAR(500) NULL,
    Status          NVARCHAR(20)  NOT NULL DEFAULT 'Open', -- Open | UnderReview | Resolved | FalsePositive
    ReviewedByUserId INT          NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    ResolvedAt      DATETIME2     NULL,
    CONSTRAINT FK_FraudAlerts_Transactions FOREIGN KEY (TransactionId) REFERENCES Transactions(TransactionId),
    CONSTRAINT FK_FraudAlerts_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_FraudAlerts_Users FOREIGN KEY (ReviewedByUserId) REFERENCES Users(UserId),
    CONSTRAINT CK_FraudAlerts_Severity CHECK (Severity IN ('Low','Medium','High','Critical')),
    CONSTRAINT CK_FraudAlerts_Status CHECK (Status IN ('Open','UnderReview','Resolved','FalsePositive'))
);
GO

/* ---------------------------------------------------------------------
   8. CustomerRiskScore
   --------------------------------------------------------------------- */
CREATE TABLE CustomerRiskScore (
    RiskScoreId     INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId      INT           NOT NULL,
    Score           INT           NOT NULL DEFAULT 0,  -- 0-100
    RiskLevel       NVARCHAR(20)  NOT NULL DEFAULT 'Low', -- Low | Medium | High | Critical
    LastCalculatedAt DATETIME2    NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_RiskScore_Customer UNIQUE (CustomerId),
    CONSTRAINT FK_RiskScore_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT CK_RiskScore_Score CHECK (Score BETWEEN 0 AND 100),
    CONSTRAINT CK_RiskScore_Level CHECK (RiskLevel IN ('Low','Medium','High','Critical'))
);
GO

/* ---------------------------------------------------------------------
   9. LoginHistory
   --------------------------------------------------------------------- */
CREATE TABLE LoginHistory (
    LoginHistoryId  BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT           NOT NULL,
    IpAddress       NVARCHAR(45)  NULL,
    IsSuccessful    BIT           NOT NULL,
    AttemptedAt     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_LoginHistory_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

/* ---------------------------------------------------------------------
   10. AuditLog
   --------------------------------------------------------------------- */
CREATE TABLE AuditLog (
    AuditLogId      BIGINT IDENTITY(1,1) PRIMARY KEY,
    EntityName      NVARCHAR(50)  NOT NULL,   -- e.g. 'Transactions', 'Customers'
    EntityId        NVARCHAR(50)  NOT NULL,
    Action          NVARCHAR(20)  NOT NULL,   -- INSERT | UPDATE | DELETE
    PerformedByUserId INT         NULL,
    Details         NVARCHAR(1000) NULL,
    CreatedAt       DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_AuditLog_Users FOREIGN KEY (PerformedByUserId) REFERENCES Users(UserId),
    CONSTRAINT CK_AuditLog_Action CHECK (Action IN ('INSERT','UPDATE','DELETE'))
);
GO
