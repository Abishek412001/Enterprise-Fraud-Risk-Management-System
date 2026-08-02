USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 13: AI Analytics Engine
   ai_analytics_schema.sql
   ===================================================================== */

-- 1. CustomerClusters Table
IF OBJECT_ID('dbo.CustomerClusters', 'U') IS NULL
BEGIN
    CREATE TABLE CustomerClusters (
        ClusterID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        ClusterName NVARCHAR(50) NOT NULL, -- LowRiskStandard | HighVelocityShopper | SuspectedMuleAccount | HighRiskInternational
        RiskScore INT NOT NULL DEFAULT 50,
        EvaluatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CustomerClusters_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId) ON DELETE CASCADE
    );
END
GO

-- 2. AnomalyLogs Table
IF OBJECT_ID('dbo.AnomalyLogs', 'U') IS NULL
BEGIN
    CREATE TABLE AnomalyLogs (
        AnomalyID INT IDENTITY(1,1) PRIMARY KEY,
        EntityType NVARCHAR(50) NOT NULL, -- Transaction | LoginSession | AccountBalance
        EntityID NVARCHAR(50) NOT NULL,
        AnomalyType NVARCHAR(100) NOT NULL, -- StatisticalOutlier | UncharacteristicMerchant | SuddenGeographicJump
        ConfidenceScore FLOAT NOT NULL DEFAULT 0.85,
        DetectedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO
