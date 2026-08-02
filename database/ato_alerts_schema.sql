USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 3: ATO Management
   ato_alerts_schema.sql
   ===================================================================== */

-- 1. Devices Table
IF OBJECT_ID('dbo.Devices', 'U') IS NULL
BEGIN
    CREATE TABLE Devices (
        DeviceID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        DeviceFingerprint NVARCHAR(255) NOT NULL,
        DeviceName NVARCHAR(100) NOT NULL DEFAULT 'Unknown Device',
        Browser NVARCHAR(100) NOT NULL DEFAULT 'Unknown Browser',
        OperatingSystem NVARCHAR(100) NOT NULL DEFAULT 'Unknown OS',
        IPAddress NVARCHAR(45) NOT NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        City NVARCHAR(100) NULL,
        Latitude DECIMAL(9,6) NULL,
        Longitude DECIMAL(9,6) NULL,
        FirstSeen DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastSeen DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsTrusted BIT NOT NULL DEFAULT 0,
        IsBlocked BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Devices_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId)
    );
END
GO

-- 2. CustomerSessions Table
IF OBJECT_ID('dbo.CustomerSessions', 'U') IS NULL
BEGIN
    CREATE TABLE CustomerSessions (
        SessionID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        DeviceID INT NULL,
        IPAddress NVARCHAR(45) NOT NULL,
        LoginTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LogoutTime DATETIME2 NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        Browser NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        OperatingSystem NVARCHAR(100) NOT NULL DEFAULT 'Unknown',
        AuthenticationMethod NVARCHAR(50) NOT NULL DEFAULT 'Password', -- Password | MFA | OAuth
        LoginStatus NVARCHAR(20) NOT NULL DEFAULT 'Success',           -- Success | Failed | Blocked
        RiskScore INT NOT NULL DEFAULT 0,                             -- 0 to 100
        CONSTRAINT FK_CustomerSessions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_CustomerSessions_Devices FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID),
        CONSTRAINT CK_CustomerSessions_Status CHECK (LoginStatus IN ('Success','Failed','Blocked'))
    );
END
GO

-- 3. ATOAlerts Table
IF OBJECT_ID('dbo.ATOAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE ATOAlerts (
        ATOAlertID INT IDENTITY(1,1) PRIMARY KEY,
        ATOAlertNumber NVARCHAR(50) NOT NULL UNIQUE,
        CustomerID INT NOT NULL,
        SessionID INT NULL,
        AlertType NVARCHAR(100) NOT NULL,    -- NewDevice | ImpossibleTravel | TorVpnLogin | CredentialStuffing | FailedLoginAbuse
        Severity NVARCHAR(20) NOT NULL DEFAULT 'High',
        Priority NVARCHAR(20) NOT NULL DEFAULT 'High',
        RiskScore INT NOT NULL DEFAULT 70,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Open', -- Open | InProgress | Closed | FalsePositive
        AssignedAnalystID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ResolvedDate DATETIME2 NULL,
        Resolution NVARCHAR(100) NULL,
        ResolutionNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_ATOAlerts_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_ATOAlerts_Sessions FOREIGN KEY (SessionID) REFERENCES CustomerSessions(SessionID),
        CONSTRAINT FK_ATOAlerts_Analyst FOREIGN KEY (AssignedAnalystID) REFERENCES Users(UserId),
        CONSTRAINT CK_ATOAlerts_Status CHECK (Status IN ('Open','InProgress','Closed','FalsePositive'))
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Devices_Fingerprint')
    CREATE INDEX IX_Devices_Fingerprint ON Devices(DeviceFingerprint, CustomerID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CustomerSessions_CustomerID_LoginTime')
    CREATE INDEX IX_CustomerSessions_CustomerID_LoginTime ON CustomerSessions(CustomerID, LoginTime DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_ATOAlerts_Status_Priority')
    CREATE INDEX IX_ATOAlerts_Status_Priority ON ATOAlerts(Status, Priority);
GO

/* =====================================================================
   FUNCTIONS
   ===================================================================== */

-- Calculate Failed Login Count in last 24h for Customer
CREATE OR ALTER FUNCTION dbo.fn_FailedLoginCount (@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT = 0;
    SELECT @Count = COUNT(*)
    FROM CustomerSessions
    WHERE CustomerID = @CustomerID
      AND LoginStatus = 'Failed'
      AND LoginTime >= DATEADD(HOUR, -24, SYSUTCDATETIME());
    RETURN ISNULL(@Count, 0);
END
GO

-- Calculate Device Risk (+20 for new device, +50 if blocked, -20 if trusted)
CREATE OR ALTER FUNCTION dbo.fn_DeviceRisk (@DeviceID INT, @CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;
    IF (@DeviceID IS NULL OR @DeviceID = 0)
        RETURN 20; -- New / Unregistered Device

    DECLARE @IsTrusted BIT, @IsBlocked BIT;
    SELECT @IsTrusted = IsTrusted, @IsBlocked = IsBlocked FROM Devices WHERE DeviceID = @DeviceID;

    IF (@IsBlocked = 1) SET @Score = 80;
    ELSE IF (@IsTrusted = 1) SET @Score = -20;
    ELSE SET @Score = 10;

    RETURN @Score;
END
GO

-- Calculate Login Velocity
CREATE OR ALTER FUNCTION dbo.fn_LoginVelocity (@CustomerID INT)
RETURNS INT
AS
BEGIN
    DECLARE @Velocity INT = 0;
    SELECT @Velocity = COUNT(*)
    FROM CustomerSessions
    WHERE CustomerID = @CustomerID
      AND LoginTime >= DATEADD(MINUTE, -15, SYSUTCDATETIME());
    RETURN ISNULL(@Velocity, 0);
END
GO

-- Composite ATO Risk Score Calculator
CREATE OR ALTER FUNCTION dbo.fn_ATORiskScore (
    @CustomerID INT,
    @DeviceID INT,
    @Country NVARCHAR(100),
    @IsTorVpn BIT = 0
)
RETURNS INT
AS
BEGIN
    DECLARE @Score INT = 0;

    -- Device Risk
    SET @Score = @Score + dbo.fn_DeviceRisk(@DeviceID, @CustomerID);

    -- Failed Logins Risk
    DECLARE @FailedCount INT = dbo.fn_FailedLoginCount(@CustomerID);
    IF (@FailedCount >= 5) SET @Score = @Score + 40;
    ELSE IF (@FailedCount >= 3) SET @Score = @Score + 20;

    -- TOR / VPN Risk
    IF (@IsTorVpn = 1) SET @Score = @Score + 50;

    -- Impossible Travel / Unknown Country Check
    IF EXISTS (SELECT 1 FROM CustomerSessions WHERE CustomerID = @CustomerID AND Country <> @Country AND LoginTime >= DATEADD(HOUR, -2, SYSUTCDATETIME()))
        SET @Score = @Score + 40;

    IF (@Score > 100) SET @Score = 100;
    IF (@Score < 0) SET @Score = 0;

    RETURN @Score;
END
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

-- Register Device
CREATE OR ALTER PROCEDURE dbo.usp_RegisterDevice
    @CustomerID INT,
    @DeviceFingerprint NVARCHAR(255),
    @DeviceName NVARCHAR(100),
    @Browser NVARCHAR(100),
    @OperatingSystem NVARCHAR(100),
    @IPAddress NVARCHAR(45),
    @Country NVARCHAR(100),
    @City NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @DeviceID INT;

    SELECT @DeviceID = DeviceID FROM Devices 
    WHERE CustomerID = @CustomerID AND DeviceFingerprint = @DeviceFingerprint;

    IF (@DeviceID IS NULL)
    BEGIN
        INSERT INTO Devices (CustomerID, DeviceFingerprint, DeviceName, Browser, OperatingSystem, IPAddress, Country, City, FirstSeen, LastSeen)
        VALUES (@CustomerID, @DeviceFingerprint, @DeviceName, @Browser, @OperatingSystem, @IPAddress, @Country, @City, SYSUTCDATETIME(), SYSUTCDATETIME());
        
        SET @DeviceID = SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE Devices 
        SET LastSeen = SYSUTCDATETIME(), IPAddress = @IPAddress, Country = @Country, City = ISNULL(@City, City)
        WHERE DeviceID = @DeviceID;
    END

    SELECT @DeviceID AS DeviceID;
END
GO

-- Record Login Attempt
CREATE OR ALTER PROCEDURE dbo.usp_RecordLogin
    @CustomerID INT,
    @DeviceFingerprint NVARCHAR(255),
    @IPAddress NVARCHAR(45),
    @Country NVARCHAR(100),
    @Browser NVARCHAR(100),
    @OperatingSystem NVARCHAR(100),
    @LoginStatus NVARCHAR(20),
    @IsTorVpn BIT = 0
AS
BEGIN
    SET NOCOUNT ON;

    -- Register or get Device ID
    DECLARE @DeviceID INT;
    EXEC dbo.usp_RegisterDevice 
        @CustomerID = @CustomerID, 
        @DeviceFingerprint = @DeviceFingerprint, 
        @DeviceName = @OperatingSystem, 
        @Browser = @Browser, 
        @OperatingSystem = @OperatingSystem, 
        @IPAddress = @IPAddress, 
        @Country = @Country;
    
    SELECT @DeviceID = DeviceID FROM Devices WHERE CustomerID = @CustomerID AND DeviceFingerprint = @DeviceFingerprint;

    -- Calculate Session Risk
    DECLARE @RiskScore INT = dbo.fn_ATORiskScore(@CustomerID, @DeviceID, @Country, @IsTorVpn);

    INSERT INTO CustomerSessions (CustomerID, DeviceID, IPAddress, LoginTime, Country, Browser, OperatingSystem, LoginStatus, RiskScore)
    VALUES (@CustomerID, @DeviceID, @IPAddress, SYSUTCDATETIME(), @Country, @Browser, @OperatingSystem, @LoginStatus, @RiskScore);

    DECLARE @SessionID INT = SCOPE_IDENTITY();

    -- Create ATO Alert if Risk Score >= 65
    IF (@RiskScore >= 65 OR @LoginStatus = 'Failed' AND dbo.fn_FailedLoginCount(@CustomerID) >= 5)
    BEGIN
        DECLARE @AlertType NVARCHAR(100) = 
            CASE 
                WHEN @IsTorVpn = 1 THEN 'TOR/VPN Suspicious Login'
                WHEN dbo.fn_FailedLoginCount(@CustomerID) >= 5 THEN 'Credential Stuffing / Password Abuse'
                WHEN @RiskScore >= 80 THEN 'Impossible Travel / Critical Device Risk'
                ELSE 'High Risk Login Activity'
            END;

        EXEC dbo.usp_CreateATOAlert 
            @CustomerID = @CustomerID,
            @SessionID = @SessionID,
            @AlertType = @AlertType,
            @Severity = 'High',
            @RiskScore = @RiskScore;
    END

    SELECT @SessionID AS SessionID, @RiskScore AS RiskScore;
END
GO

-- Create ATO Alert
CREATE OR ALTER PROCEDURE dbo.usp_CreateATOAlert
    @CustomerID INT,
    @SessionID INT = NULL,
    @AlertType NVARCHAR(100),
    @Severity NVARCHAR(20) = 'High',
    @RiskScore INT = 70
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ATOAlertNumber NVARCHAR(50);
    SET @ATOAlertNumber = CONCAT('ATO-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR(4)), 4));

    DECLARE @Priority NVARCHAR(20) = CASE WHEN @RiskScore >= 80 THEN 'Critical' ELSE 'High' END;

    INSERT INTO ATOAlerts (ATOAlertNumber, CustomerID, SessionID, AlertType, Severity, Priority, RiskScore, Status, CreatedDate)
    VALUES (@ATOAlertNumber, @CustomerID, @SessionID, @AlertType, @Severity, @Priority, @RiskScore, 'Open', SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS ATOAlertID, @ATOAlertNumber AS ATOAlertNumber;
END
GO

-- Assign ATO Alert
CREATE OR ALTER PROCEDURE dbo.usp_AssignATOAlert
    @ATOAlertID INT,
    @AnalystID INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ATOAlerts
    SET AssignedAnalystID = @AnalystID, Status = 'InProgress'
    WHERE ATOAlertID = @ATOAlertID;
END
GO

-- Close ATO Alert
CREATE OR ALTER PROCEDURE dbo.usp_CloseATOAlert
    @ATOAlertID INT,
    @Resolution NVARCHAR(100),
    @ResolutionNotes NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ATOAlerts
    SET Status = 'Closed',
        Resolution = @Resolution,
        ResolutionNotes = @ResolutionNotes,
        ResolvedDate = SYSUTCDATETIME()
    WHERE ATOAlertID = @ATOAlertID;
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_OpenATOAlerts AS
SELECT a.ATOAlertID, a.ATOAlertNumber, a.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       a.SessionID, s.IPAddress, s.Country, s.Browser, s.OperatingSystem,
       a.AlertType, a.Severity, a.Priority, a.RiskScore, a.Status,
       a.AssignedAnalystID, u.Username AS AssignedAnalystName, a.CreatedDate
FROM ATOAlerts a
INNER JOIN Customers c ON c.CustomerId = a.CustomerID
LEFT JOIN CustomerSessions s ON s.SessionID = a.SessionID
LEFT JOIN Users u ON u.UserId = a.AssignedAnalystID
WHERE a.Status IN ('Open', 'InProgress');
GO

CREATE OR ALTER VIEW dbo.vw_HighRiskLogins AS
SELECT s.SessionID, s.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       s.IPAddress, s.Country, s.Browser, s.OperatingSystem, s.LoginTime, s.LoginStatus, s.RiskScore
FROM CustomerSessions s
INNER JOIN Customers c ON c.CustomerId = s.CustomerID
WHERE s.RiskScore >= 60;
GO

CREATE OR ALTER VIEW dbo.vw_SuspiciousDevices AS
SELECT d.DeviceID, d.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       d.DeviceFingerprint, d.DeviceName, d.Browser, d.OperatingSystem, d.IPAddress, d.Country,
       d.FirstSeen, d.LastSeen, d.IsTrusted, d.IsBlocked
FROM Devices d
INNER JOIN Customers c ON c.CustomerId = d.CustomerID
WHERE d.IsBlocked = 1 OR d.IsTrusted = 0;
GO
