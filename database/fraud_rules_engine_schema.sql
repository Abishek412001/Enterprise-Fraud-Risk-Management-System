USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 12: Advanced Rules Engine
   fraud_rules_engine_schema.sql
   ===================================================================== */

-- 1. FraudRules Table
IF OBJECT_ID('dbo.FraudRules', 'U') IS NULL
BEGIN
    CREATE TABLE FraudRules (
        RuleID INT IDENTITY(1,1) PRIMARY KEY,
        RuleCode NVARCHAR(50) NOT NULL UNIQUE,
        RuleName NVARCHAR(150) NOT NULL,
        Category NVARCHAR(50) NOT NULL, -- Velocity | ATO | Merchant | Device | Location | Mule
        ConditionExpression NVARCHAR(1000) NOT NULL,
        RiskScoreWeight INT NOT NULL DEFAULT 20,
        ActionToTake NVARCHAR(50) NOT NULL DEFAULT 'CreateAlert', -- Block | CreateAlert | Flag | Review
        Priority INT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );

    INSERT INTO FraudRules (RuleCode, RuleName, Category, ConditionExpression, RiskScoreWeight, ActionToTake, Priority) VALUES
    ('RULE-101', 'High Velocity Spikes', 'Velocity', 'TransactionCount_1Hour > 5', 40, 'CreateAlert', 1),
    ('RULE-102', 'Impossible Travel Speed', 'Location', 'DistanceKm > 500 AND MinutesDiff < 30', 50, 'Block', 1),
    ('RULE-103', 'Unknown Hardware Device Fingerprint', 'Device', 'DeviceIsTrusted = 0 AND FailedAttempts > 3', 35, 'CreateAlert', 2),
    ('RULE-104', 'High Risk Country Transaction', 'Location', 'Country IN (''Nigeria'', ''North Korea'')', 30, 'Review', 3);
END
GO

-- 2. RuleExecutions Table
IF OBJECT_ID('dbo.RuleExecutions', 'U') IS NULL
BEGIN
    CREATE TABLE RuleExecutions (
        ExecutionID INT IDENTITY(1,1) PRIMARY KEY,
        RuleID INT NOT NULL,
        TransactionID INT NULL,
        CustomerID INT NOT NULL,
        IsTriggered BIT NOT NULL,
        ScoreImpact INT NOT NULL DEFAULT 0,
        ExecutionTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RuleExecutions_FraudRules FOREIGN KEY (RuleID) REFERENCES FraudRules(RuleID) ON DELETE CASCADE
    );
END
GO
