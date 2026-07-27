-- =====================================================================
-- File: database/views.sql
-- =====================================================================
USE FraudRiskDB;
GO

IF OBJECT_ID('dbo.vwHighRiskCustomers', 'V') IS NOT NULL DROP VIEW dbo.vwHighRiskCustomers;
GO
IF OBJECT_ID('dbo.vwDailyFraudSummary', 'V') IS NOT NULL DROP VIEW dbo.vwDailyFraudSummary;
GO

-- ---------------------------------------------------------------------
-- vwHighRiskCustomers: score >= 70 OR blacklisted, ranked by score.
-- Demonstrates LEFT JOIN, CASE, and a window function (RANK).
-- ---------------------------------------------------------------------
CREATE VIEW dbo.vwHighRiskCustomers
AS
SELECT
    c.CustomerID,
    c.FullName,
    c.HomeCountry,
    ISNULL(rs.Score, 0)                                   AS RiskScore,
    CASE WHEN bl.CustomerID IS NOT NULL THEN 1 ELSE 0 END AS IsBlacklisted,
    RANK() OVER (ORDER BY ISNULL(rs.Score, 0) DESC)       AS RiskRank
FROM dbo.Customers c
LEFT JOIN dbo.RiskScores rs ON rs.CustomerID = c.CustomerID
LEFT JOIN dbo.Blacklist bl  ON bl.CustomerID = c.CustomerID
WHERE ISNULL(rs.Score, 0) >= 70 OR bl.CustomerID IS NOT NULL;
GO

-- ---------------------------------------------------------------------
-- vwDailyFraudSummary: alerts grouped by calendar day and severity mix.
-- ---------------------------------------------------------------------
CREATE VIEW dbo.vwDailyFraudSummary
AS
SELECT
    CAST(fa.CreatedAt AS DATE)                                   AS AlertDate,
    COUNT(*)                                                     AS TotalAlerts,
    SUM(CASE WHEN fa.Severity = 'High'   THEN 1 ELSE 0 END)      AS HighSeverity,
    SUM(CASE WHEN fa.Severity = 'Medium' THEN 1 ELSE 0 END)      AS MediumSeverity,
    SUM(CASE WHEN fa.Severity = 'Low'    THEN 1 ELSE 0 END)      AS LowSeverity,
    SUM(CASE WHEN fa.Status = 'Open'     THEN 1 ELSE 0 END)      AS StillOpen
FROM dbo.FraudAlerts fa
GROUP BY CAST(fa.CreatedAt AS DATE);
GO

PRINT 'views.sql executed successfully.';
GO

-- =====================================================================
-- Output validation examples
-- =====================================================================
-- SELECT TOP 10 * FROM dbo.vwHighRiskCustomers ORDER BY RiskRank;
--   Expected: CustomerID 12 and 31 (seeded blacklisted) near the top,
--   plus anyone usp_CalculateRiskScore has pushed to >= 70.
--
-- SELECT TOP 10 * FROM dbo.vwDailyFraudSummary ORDER BY AlertDate DESC;
--   Expected columns: AlertDate | TotalAlerts | HighSeverity | MediumSeverity | LowSeverity | StillOpen
