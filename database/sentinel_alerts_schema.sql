USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 4: Microsoft Sentinel SIEM
   sentinel_alerts_schema.sql
   ===================================================================== */

-- 1. SentinelIncidents Table
IF OBJECT_ID('dbo.SentinelIncidents', 'U') IS NULL
BEGIN
    CREATE TABLE SentinelIncidents (
        IncidentID INT IDENTITY(1,1) PRIMARY KEY,
        IncidentNumber NVARCHAR(50) NOT NULL UNIQUE,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Severity NVARCHAR(20) NOT NULL DEFAULT 'High',
        Status NVARCHAR(20) NOT NULL DEFAULT 'New', -- New | Active | Closed | FalsePositive
        AssignedAnalystID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ResolvedDate DATETIME2 NULL,
        CONSTRAINT FK_SentinelIncidents_Analyst FOREIGN KEY (AssignedAnalystID) REFERENCES Users(UserId)
    );
END
GO

-- 2. SentinelAlerts Table
IF OBJECT_ID('dbo.SentinelAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE SentinelAlerts (
        AlertID INT IDENTITY(1,1) PRIMARY KEY,
        AlertNumber NVARCHAR(50) NOT NULL UNIQUE,
        AlertName NVARCHAR(150) NOT NULL,
        AlertCategory NVARCHAR(100) NOT NULL,  -- IdentitySecurity | EndpointThreat | NetworkAnomalies | PrivilegeEscalation
        AlertSource NVARCHAR(100) NOT NULL DEFAULT 'Microsoft Sentinel', -- Sentinel | Defender | AzureAD | VirusTotal | AbuseIPDB
        AlertRule NVARCHAR(150) NOT NULL,
        CustomerID INT NOT NULL,
        UserID INT NULL,
        IPAddress NVARCHAR(45) NOT NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        DeviceID INT NULL,
        Severity NVARCHAR(20) NOT NULL DEFAULT 'High',
        Priority NVARCHAR(20) NOT NULL DEFAULT 'High',
        RiskScore INT NOT NULL DEFAULT 75,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Open',
        AssignedAnalystID INT NULL,
        IncidentID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ClosedDate DATETIME2 NULL,
        Resolution NVARCHAR(100) NULL,
        ResolutionNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_SentinelAlerts_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_SentinelAlerts_Users FOREIGN KEY (UserID) REFERENCES Users(UserId),
        CONSTRAINT FK_SentinelAlerts_Devices FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID),
        CONSTRAINT FK_SentinelAlerts_Analyst FOREIGN KEY (AssignedAnalystID) REFERENCES Users(UserId),
        CONSTRAINT FK_SentinelAlerts_Incidents FOREIGN KEY (IncidentID) REFERENCES SentinelIncidents(IncidentID)
    );
END
GO

-- 3. ThreatIndicators Table (Threat Intelligence Simulation)
IF OBJECT_ID('dbo.ThreatIndicators', 'U') IS NULL
BEGIN
    CREATE TABLE ThreatIndicators (
        IndicatorID INT IDENTITY(1,1) PRIMARY KEY,
        IndicatorType NVARCHAR(50) NOT NULL,  -- IPAddress | MaliciousDevice | MaliciousDomain | DarkWebEmail
        IndicatorValue NVARCHAR(255) NOT NULL,
        ThreatLevel NVARCHAR(20) NOT NULL DEFAULT 'High',
        Source NVARCHAR(100) NOT NULL DEFAULT 'AbuseIPDB', -- VirusTotal | AbuseIPDB | AzureAD | DarkWeb
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
    );
END
GO

-- 4. SecurityEvents Table
IF OBJECT_ID('dbo.SecurityEvents', 'U') IS NULL
BEGIN
    CREATE TABLE SecurityEvents (
        EventID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        IPAddress NVARCHAR(45) NOT NULL,
        DeviceID INT NULL,
        EventType NVARCHAR(100) NOT NULL, -- BruteForceLogin | PowerShellExecution | PrivilegeEscalation | RansomwareActivity | MfaFailure
        EventTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Result NVARCHAR(50) NOT NULL DEFAULT 'Success',
        Application NVARCHAR(100) NOT NULL DEFAULT 'EFRS Portal',
        OperatingSystem NVARCHAR(100) NOT NULL DEFAULT 'Windows 11',
        CONSTRAINT FK_SecurityEvents_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_SecurityEvents_Devices FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID)
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SentinelAlerts_Status_Severity')
    CREATE INDEX IX_SentinelAlerts_Status_Severity ON SentinelAlerts(Status, Severity);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SecurityEvents_CustomerID_EventTime')
    CREATE INDEX IX_SecurityEvents_CustomerID_EventTime ON SecurityEvents(CustomerID, EventTime DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ThreatIndicators_Value')
    CREATE INDEX IX_ThreatIndicators_Value ON ThreatIndicators(IndicatorValue);
GO

/* =====================================================================
   FUNCTIONS
   ===================================================================== */

CREATE OR ALTER FUNCTION dbo.fn_ThreatIndicatorScore (@IPAddress NVARCHAR(45))
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;
    IF EXISTS (SELECT 1 FROM ThreatIndicators WHERE IndicatorValue = @IPAddress AND IndicatorType = 'IPAddress')
        SET @Score = 50;
    RETURN @Score;
END
GO

CREATE OR ALTER FUNCTION dbo.fn_SecurityRiskScore (
    @CustomerID INT,
    @IPAddress NVARCHAR(45),
    @DeviceID INT
)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;

    -- Threat Intel Score
    SET @Score = @Score + dbo.fn_ThreatIndicatorScore(@IPAddress);

    -- Security Event count in last 24h
    DECLARE @EventCount INT = 0;
    SELECT @EventCount = COUNT(*) FROM SecurityEvents WHERE CustomerID = @CustomerID AND EventTime >= DATEADD(HOUR, -24, SYSUTCDATETIME());

    IF (@EventCount >= 10) SET @Score = @Score + 40;
    ELSE IF (@EventCount >= 5) SET @Score = @Score + 20;

    IF (@Score > 100) SET @Score = 100;
    RETURN @Score;
END
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

-- Create Incident
CREATE OR ALTER PROCEDURE dbo.usp_CreateIncident
    @Title NVARCHAR(200),
    @Description NVARCHAR(1000),
    @Severity NVARCHAR(20) = 'High'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @IncidentNumber NVARCHAR(50);
    SET @IncidentNumber = CONCAT('INC-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR(4)), 4));

    INSERT INTO SentinelIncidents (IncidentNumber, Title, Description, Severity, Status, CreatedDate)
    VALUES (@IncidentNumber, @Title, @Description, @Severity, 'New', SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS IncidentID, @IncidentNumber AS IncidentNumber;
END
GO

-- Create Sentinel Alert
CREATE OR ALTER PROCEDURE dbo.usp_CreateSentinelAlert
    @AlertName NVARCHAR(150),
    @AlertCategory NVARCHAR(100),
    @AlertSource NVARCHAR(100),
    @AlertRule NVARCHAR(150),
    @CustomerID INT,
    @UserID INT = NULL,
    @IPAddress NVARCHAR(45),
    @Country NVARCHAR(100),
    @DeviceID INT = NULL,
    @Severity NVARCHAR(20) = 'High',
    @RiskScore INT = 75
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AlertNumber NVARCHAR(50);
    SET @AlertNumber = CONCAT('SEN-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR(4)), 4));

    -- Create correlated incident if severity is Critical or RiskScore >= 85
    DECLARE @IncidentID INT = NULL;
    IF (@Severity = 'Critical' OR @RiskScore >= 85)
    BEGIN
        EXEC dbo.usp_CreateIncident 
            @Title = @AlertName,
            @Description = @AlertRule,
            @Severity = @Severity;
            
        SELECT TOP 1 @IncidentID = IncidentID FROM SentinelIncidents ORDER BY IncidentID DESC;
    END

    INSERT INTO SentinelAlerts (AlertNumber, AlertName, AlertCategory, AlertSource, AlertRule, CustomerID, UserID, IPAddress, Country, DeviceID, Severity, Priority, RiskScore, Status, IncidentID, CreatedDate)
    VALUES (@AlertNumber, @AlertName, @AlertCategory, @AlertSource, @AlertRule, @CustomerID, @UserID, @IPAddress, @Country, @DeviceID, @Severity, CASE WHEN @RiskScore >= 85 THEN 'Critical' ELSE 'High' END, @RiskScore, 'Open', @IncidentID, SYSUTCDATETIME());

    -- Auto Freeze Customer Account if Critical Severity
    IF (@Severity = 'Critical')
    BEGIN
        UPDATE Customers SET IsBlacklisted = 1 WHERE CustomerId = @CustomerID;
    END

    SELECT SCOPE_IDENTITY() AS AlertID, @AlertNumber AS AlertNumber, @IncidentID AS IncidentID;
END
GO

-- Assign Incident
CREATE OR ALTER PROCEDURE dbo.usp_AssignIncident
    @IncidentID INT,
    @AnalystID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SentinelIncidents
    SET AssignedAnalystID = @AnalystID, Status = 'Active'
    WHERE IncidentID = @IncidentID;

    UPDATE SentinelAlerts
    SET AssignedAnalystID = @AnalystID, Status = 'InProgress'
    WHERE IncidentID = @IncidentID;
END
GO

-- Close Incident
CREATE OR ALTER PROCEDURE dbo.usp_CloseIncident
    @IncidentID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE SentinelIncidents
    SET Status = 'Closed', ResolvedDate = SYSUTCDATETIME()
    WHERE IncidentID = @IncidentID;

    UPDATE SentinelAlerts
    SET Status = 'Closed', ClosedDate = SYSUTCDATETIME()
    WHERE IncidentID = @IncidentID;
END
GO

-- Record Security Event
CREATE OR ALTER PROCEDURE dbo.usp_RecordSecurityEvent
    @CustomerID INT,
    @IPAddress NVARCHAR(45),
    @DeviceID INT = NULL,
    @EventType NVARCHAR(100),
    @Result NVARCHAR(50) = 'Success',
    @Application NVARCHAR(100) = 'EFRS Portal',
    @OperatingSystem NVARCHAR(100) = 'Windows 11'
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SecurityEvents (CustomerID, IPAddress, DeviceID, EventType, EventTime, Result, Application, OperatingSystem)
    VALUES (@CustomerID, @IPAddress, @DeviceID, @EventType, SYSUTCDATETIME(), @Result, @Application, @OperatingSystem);

    DECLARE @EventID INT = SCOPE_IDENTITY();

    -- Create Sentinel Alert for high risk events
    IF (@EventType IN ('BruteForceLogin', 'RansomwareActivity', 'PrivilegeEscalation', 'PowerShellExecution'))
    BEGIN
        EXEC dbo.usp_CreateSentinelAlert
            @AlertName = @EventType,
            @AlertCategory = 'SIEM Security Anomaly',
            @AlertSource = 'Microsoft Sentinel Correlation Engine',
            @AlertRule = @EventType,
            @CustomerID = @CustomerID,
            @IPAddress = @IPAddress,
            @Country = 'USA',
            @DeviceID = @DeviceID,
            @Severity = 'Critical',
            @RiskScore = 90;
    END

    SELECT @EventID AS EventID;
END
GO

-- Record Threat Indicator
CREATE OR ALTER PROCEDURE dbo.usp_RecordThreatIndicator
    @IndicatorType NVARCHAR(50),
    @IndicatorValue NVARCHAR(255),
    @ThreatLevel NVARCHAR(20) = 'High',
    @Source NVARCHAR(100) = 'AbuseIPDB'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ThreatIndicators (IndicatorType, IndicatorValue, ThreatLevel, Source, CreatedDate)
    VALUES (@IndicatorType, @IndicatorValue, @ThreatLevel, @Source, SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS IndicatorID;
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_OpenSentinelAlerts AS
SELECT a.AlertID, a.AlertNumber, a.AlertName, a.AlertCategory, a.AlertSource, a.AlertRule,
       a.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       a.IPAddress, a.Country, a.Severity, a.Priority, a.RiskScore, a.Status,
       a.AssignedAnalystID, u.Username AS AssignedAnalystName, a.IncidentID, a.CreatedDate
FROM SentinelAlerts a
INNER JOIN Customers c ON c.CustomerId = a.CustomerID
LEFT JOIN Users u ON u.UserId = a.AssignedAnalystID
WHERE a.Status IN ('Open', 'InProgress');
GO

CREATE OR ALTER VIEW dbo.vw_CriticalSecurityIncidents AS
SELECT i.IncidentID, i.IncidentNumber, i.Title, i.Description, i.Severity, i.Status,
       i.AssignedAnalystID, u.Username AS AssignedAnalystName, i.CreatedDate
FROM SentinelIncidents i
LEFT JOIN Users u ON u.UserId = i.AssignedAnalystID
WHERE i.Severity = 'Critical' AND i.Status <> 'Closed';
GO

CREATE OR ALTER VIEW dbo.vw_SecurityEvents AS
SELECT e.EventID, e.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       e.IPAddress, e.DeviceID, e.EventType, e.EventTime, e.Result, e.Application, e.OperatingSystem
FROM SecurityEvents e
INNER JOIN Customers c ON c.CustomerId = e.CustomerID;
GO

CREATE OR ALTER VIEW dbo.vw_ThreatIndicators AS
SELECT IndicatorID, IndicatorType, IndicatorValue, ThreatLevel, Source, CreatedDate
FROM ThreatIndicators;
GO
