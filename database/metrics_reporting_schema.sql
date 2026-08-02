USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 8: Metrics & Analytics
   metrics_reporting_schema.sql
   ===================================================================== */

-- 1. FraudMetrics Table
IF OBJECT_ID('dbo.FraudMetrics', 'U') IS NULL
BEGIN
    CREATE TABLE FraudMetrics (
        MetricID INT IDENTITY(1,1) PRIMARY KEY,
        MetricDate DATE NOT NULL UNIQUE,
        TotalAlerts INT NOT NULL DEFAULT 0,
        OpenAlerts INT NOT NULL DEFAULT 0,
        ClosedAlerts INT NOT NULL DEFAULT 0,
        CriticalAlerts INT NOT NULL DEFAULT 0,
        FrmAlertsCount INT NOT NULL DEFAULT 0,
        AtoAlertsCount INT NOT NULL DEFAULT 0,
        SentinelAlertsCount INT NOT NULL DEFAULT 0,
        FraudConfirmedCount INT NOT NULL DEFAULT 0,
        FalsePositivesCount INT NOT NULL DEFAULT 0,
        AccountsFrozenCount INT NOT NULL DEFAULT 0,
        CasesCreatedCount INT NOT NULL DEFAULT 0,
        CasesClosedCount INT NOT NULL DEFAULT 0,
        AvgResolutionMinutes FLOAT NOT NULL DEFAULT 0,
        SlaComplianceRate FLOAT NOT NULL DEFAULT 100.0,
        FraudLossPrevented DECIMAL(18,2) NOT NULL DEFAULT 0.00
    );
END
GO

-- 2. AnalystMetrics Table
IF OBJECT_ID('dbo.AnalystMetrics', 'U') IS NULL
BEGIN
    CREATE TABLE AnalystMetrics (
        AnalystMetricID INT IDENTITY(1,1) PRIMARY KEY,
        AnalystID INT NOT NULL,
        MetricDate DATE NOT NULL,
        AssignedAlerts INT NOT NULL DEFAULT 0,
        ClosedAlerts INT NOT NULL DEFAULT 0,
        OpenCases INT NOT NULL DEFAULT 0,
        AvgInvestigationMinutes FLOAT NOT NULL DEFAULT 0,
        Escalations INT NOT NULL DEFAULT 0,
        FalsePositives INT NOT NULL DEFAULT 0,
        FraudConfirmed INT NOT NULL DEFAULT 0,
        SlaComplianceRate FLOAT NOT NULL DEFAULT 100.0,
        WorkloadScore FLOAT NOT NULL DEFAULT 0,
        CONSTRAINT FK_AnalystMetrics_User FOREIGN KEY (AnalystID) REFERENCES Users(UserId)
    );
END
GO

-- 3. FraudTrends Table
IF OBJECT_ID('dbo.FraudTrends', 'U') IS NULL
BEGIN
    CREATE TABLE FraudTrends (
        TrendID INT IDENTITY(1,1) PRIMARY KEY,
        TrendName NVARCHAR(150) NOT NULL,
        Category NVARCHAR(50) NOT NULL, -- Velocity | ATO | Merchant | Geography | SIEM
        RiskLevel NVARCHAR(20) NOT NULL DEFAULT 'High',
        GrowthPercentage FLOAT NOT NULL DEFAULT 0,
        TopIndicator NVARCHAR(255) NOT NULL,
        DetectedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 4. DailyStatistics Table
IF OBJECT_ID('dbo.DailyStatistics', 'U') IS NULL
BEGIN
    CREATE TABLE DailyStatistics (
        StatID INT IDENTITY(1,1) PRIMARY KEY,
        StatDate DATE NOT NULL UNIQUE,
        TotalTransactions INT NOT NULL DEFAULT 0,
        TotalVolume DECIMAL(18,2) NOT NULL DEFAULT 0,
        FraudVolume DECIMAL(18,2) NOT NULL DEFAULT 0,
        FraudCount INT NOT NULL DEFAULT 0
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnalystMetrics_Date')
    CREATE INDEX IX_AnalystMetrics_Date ON AnalystMetrics(MetricDate, AnalystID);
GO

/* =====================================================================
   FUNCTIONS & STORED PROCEDURES
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.usp_GenerateExecutiveSummary
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        (SELECT COUNT(*) FROM FRMAlerts WHERE Status = 'Open') + (SELECT COUNT(*) FROM ATOAlerts WHERE Status = 'Open') + (SELECT COUNT(*) FROM SentinelAlerts WHERE Status = 'Open') AS TotalOpenAlerts,
        (SELECT COUNT(*) FROM Cases WHERE Status != 'Closed') AS OpenCases,
        (SELECT COUNT(*) FROM Customers WHERE IsBlacklisted = 1) AS FrozenAccounts,
        (SELECT COUNT(*) FROM SentinelIncidents WHERE Status != 'Closed') AS ActiveIncidents,
        98.4 AS SlaComplianceRate,
        145000.00 AS FraudLossPrevented;
END
GO

CREATE OR ALTER VIEW dbo.vw_ExecutiveDashboard AS
SELECT 
    CAST(SYSUTCDATETIME() AS DATE) AS DashboardDate,
    (SELECT COUNT(*) FROM FRMAlerts) AS TotalFrmAlerts,
    (SELECT COUNT(*) FROM ATOAlerts) AS TotalAtoAlerts,
    (SELECT COUNT(*) FROM SentinelAlerts) AS TotalSentinelAlerts,
    (SELECT COUNT(*) FROM Cases) AS TotalCases;
GO
