USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Automated SQL Integration Tests
   test_scripts.sql
   ===================================================================== */

PRINT 'Starting EFRS SQL Verification Suite...';

-- 1. Test FRM Alerts Creation Stored Procedure
BEGIN TRANSACTION;
    DECLARE @AlertID INT;
    EXEC dbo.usp_CreateFRMAlert 
        @CustomerID = 1, 
        @AccountID = 1, 
        @TransactionID = 1, 
        @AlertType = 'High Velocity Spikes', 
        @AlertCategory = 'Velocity', 
        @Severity = 'Critical', 
        @RiskScore = 95, 
        @RuleTriggered = 'RULE-101 Velocity Limit Exceeded', 
        @NewAlertID = @AlertID OUTPUT;

    IF @AlertID IS NOT NULL
        PRINT '[PASS] usp_CreateFRMAlert created alert ID: ' + CAST(@AlertID AS NVARCHAR);
    ELSE
        PRINT '[FAIL] usp_CreateFRMAlert failed.';
ROLLBACK TRANSACTION;

-- 2. Test SLA Calculation Function
DECLARE @SlaStatus NVARCHAR(50) = dbo.fn_SLAStatus(SYSUTCDATETIME(), DATEADD(hour, 2, SYSUTCDATETIME()), 'Open');
IF @SlaStatus = 'OnTrack'
    PRINT '[PASS] fn_SLAStatus returns OnTrack correctly.';
ELSE
    PRINT '[FAIL] fn_SLAStatus expected OnTrack, got: ' + ISNULL(@SlaStatus, 'NULL');

PRINT 'SQL Verification Suite Completed.';
GO
