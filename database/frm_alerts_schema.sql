USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 2: FRM Alert Management
   frm_alerts_schema.sql
   ===================================================================== */

-- 1. FRMAlerts Table
IF OBJECT_ID('dbo.FRMAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE FRMAlerts (
        AlertID INT IDENTITY(1,1) PRIMARY KEY,
        AlertNumber NVARCHAR(50) NOT NULL UNIQUE,
        CustomerID INT NOT NULL,
        AccountID INT NOT NULL,
        TransactionID BIGINT NULL,
        AlertType NVARCHAR(50) NOT NULL,       -- Velocity | HighValue | AccountTakeover | DeviceRisk | ForeignTx
        AlertCategory NVARCHAR(50) NOT NULL,   -- CardFraud | AccountFraud | IdentityFraud | Operational
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low | Medium | High | Critical
        Severity NVARCHAR(20) NOT NULL DEFAULT 'Medium', -- Low | Medium | High | Critical
        Status NVARCHAR(20) NOT NULL DEFAULT 'Open',     -- Open | InProgress | Escalated | Closed | FalsePositive
        RiskScore INT NOT NULL DEFAULT 50,               -- 0 to 100
        AssignedAnalystID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastUpdated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ClosedDate DATETIME2 NULL,
        Resolution NVARCHAR(100) NULL,
        ResolutionNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_FRMAlerts_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_FRMAlerts_Accounts FOREIGN KEY (AccountID) REFERENCES Accounts(AccountId),
        CONSTRAINT FK_FRMAlerts_Transactions FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionId),
        CONSTRAINT FK_FRMAlerts_Analyst FOREIGN KEY (AssignedAnalystID) REFERENCES Users(UserId),
        CONSTRAINT CK_FRMAlerts_Priority CHECK (Priority IN ('Low','Medium','High','Critical')),
        CONSTRAINT CK_FRMAlerts_Severity CHECK (Severity IN ('Low','Medium','High','Critical')),
        CONSTRAINT CK_FRMAlerts_Status CHECK (Status IN ('Open','InProgress','Escalated','Closed','FalsePositive')),
        CONSTRAINT CK_FRMAlerts_RiskScore CHECK (RiskScore BETWEEN 0 AND 100)
    );
END
GO

-- 2. AlertAssignments Table
IF OBJECT_ID('dbo.AlertAssignments', 'U') IS NULL
BEGIN
    CREATE TABLE AlertAssignments (
        AssignmentID INT IDENTITY(1,1) PRIMARY KEY,
        AlertID INT NOT NULL,
        AnalystID INT NOT NULL,
        AssignedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        AssignedBy INT NULL,
        CONSTRAINT FK_AlertAssignments_Alert FOREIGN KEY (AlertID) REFERENCES FRMAlerts(AlertID) ON DELETE CASCADE,
        CONSTRAINT FK_AlertAssignments_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId),
        CONSTRAINT FK_AlertAssignments_AssignedBy FOREIGN KEY (AssignedBy) REFERENCES Users(UserId)
    );
END
GO

-- 3. AlertHistory Table
IF OBJECT_ID('dbo.AlertHistory', 'U') IS NULL
BEGIN
    CREATE TABLE AlertHistory (
        HistoryID INT IDENTITY(1,1) PRIMARY KEY,
        AlertID INT NOT NULL,
        Action NVARCHAR(50) NOT NULL,  -- Created | Assigned | StatusChanged | Escalated | Commented | Closed
        OldStatus NVARCHAR(20) NULL,
        NewStatus NVARCHAR(20) NULL,
        ActionBy INT NULL,
        Comments NVARCHAR(1000) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AlertHistory_Alert FOREIGN KEY (AlertID) REFERENCES FRMAlerts(AlertID) ON DELETE CASCADE,
        CONSTRAINT FK_AlertHistory_User FOREIGN KEY (ActionBy) REFERENCES Users(UserId)
    );
END
GO

-- 4. AlertComments Table
IF OBJECT_ID('dbo.AlertComments', 'U') IS NULL
BEGIN
    CREATE TABLE AlertComments (
        CommentID INT IDENTITY(1,1) PRIMARY KEY,
        AlertID INT NOT NULL,
        AnalystID INT NOT NULL,
        Comment NVARCHAR(1000) NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_AlertComments_Alert FOREIGN KEY (AlertID) REFERENCES FRMAlerts(AlertID) ON DELETE CASCADE,
        CONSTRAINT FK_AlertComments_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId)
    );
END
GO

-- Indexes for performance
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FRMAlerts_Status_Priority')
    CREATE INDEX IX_FRMAlerts_Status_Priority ON FRMAlerts(Status, Priority);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FRMAlerts_CustomerID')
    CREATE INDEX IX_FRMAlerts_CustomerID ON FRMAlerts(CustomerID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_FRMAlerts_AssignedAnalystID')
    CREATE INDEX IX_FRMAlerts_AssignedAnalystID ON FRMAlerts(AssignedAnalystID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlertHistory_AlertID')
    CREATE INDEX IX_AlertHistory_AlertID ON AlertHistory(AlertID, Timestamp DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AlertComments_AlertID')
    CREATE INDEX IX_AlertComments_AlertID ON AlertComments(AlertID, Timestamp DESC);
GO

/* =====================================================================
   FUNCTIONS
   ===================================================================== */

-- Calculate Priority based on Risk Score & Severity
CREATE OR ALTER FUNCTION dbo.fn_CalculateAlertPriority (@RiskScore INT, @Severity NVARCHAR(20))
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Priority NVARCHAR(20) = 'Low';
    IF (@RiskScore >= 85 OR @Severity = 'Critical')
        SET @Priority = 'Critical';
    ELSE IF (@RiskScore >= 65 OR @Severity = 'High')
        SET @Priority = 'High';
    ELSE IF (@RiskScore >= 40 OR @Severity = 'Medium')
        SET @Priority = 'Medium';
    
    RETURN @Priority;
END
GO

-- Calculate Alert Age in hours
CREATE OR ALTER FUNCTION dbo.fn_AlertAge (@AlertID INT)
RETURNS INT
AS
BEGIN
    DECLARE @AgeHours INT = 0;
    DECLARE @CreatedDate DATETIME2, @ClosedDate DATETIME2;

    SELECT @CreatedDate = CreatedDate, @ClosedDate = ClosedDate
    FROM FRMAlerts WHERE AlertID = @AlertID;

    IF (@CreatedDate IS NOT NULL)
    BEGIN
        SET @AgeHours = DATEDIFF(HOUR, @CreatedDate, ISNULL(@ClosedDate, SYSUTCDATETIME()));
    END

    RETURN @AgeHours;
END
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

-- Create FRM Alert
CREATE OR ALTER PROCEDURE dbo.usp_CreateFRMAlert
    @CustomerID INT,
    @AccountID INT,
    @TransactionID BIGINT = NULL,
    @AlertType NVARCHAR(50),
    @AlertCategory NVARCHAR(50),
    @Severity NVARCHAR(20) = 'Medium',
    @RiskScore INT = 50,
    @ResolutionNotes NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AlertNumber NVARCHAR(50);
    SET @AlertNumber = CONCAT('FRM-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR(4)), 4));

    DECLARE @Priority NVARCHAR(20) = dbo.fn_CalculateAlertPriority(@RiskScore, @Severity);

    INSERT INTO FRMAlerts (AlertNumber, CustomerID, AccountID, TransactionID, AlertType, AlertCategory, Priority, Severity, Status, RiskScore, CreatedDate, LastUpdated)
    VALUES (@AlertNumber, @CustomerID, @AccountID, @TransactionID, @AlertType, @AlertCategory, @Priority, @Severity, 'Open', @RiskScore, SYSUTCDATETIME(), SYSUTCDATETIME());

    DECLARE @NewAlertID INT = SCOPE_IDENTITY();

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@NewAlertID, 'Created', NULL, 'Open', NULL, ISNULL(@ResolutionNotes, 'Alert automatically generated'), SYSUTCDATETIME());

    SELECT @NewAlertID AS AlertID, @AlertNumber AS AlertNumber;
END
GO

-- Assign Alert
CREATE OR ALTER PROCEDURE dbo.usp_AssignAlert
    @AlertID INT,
    @AnalystID INT,
    @AssignedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus NVARCHAR(20);
    SELECT @OldStatus = Status FROM FRMAlerts WHERE AlertID = @AlertID;

    UPDATE FRMAlerts
    SET AssignedAnalystID = @AnalystID,
        Status = CASE WHEN Status = 'Open' THEN 'InProgress' ELSE Status END,
        LastUpdated = SYSUTCDATETIME()
    WHERE AlertID = @AlertID;

    INSERT INTO AlertAssignments (AlertID, AnalystID, AssignedDate, AssignedBy)
    VALUES (@AlertID, @AnalystID, SYSUTCDATETIME(), @AssignedBy);

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@AlertID, 'Assigned', @OldStatus, CASE WHEN @OldStatus = 'Open' THEN 'InProgress' ELSE @OldStatus END, @AssignedBy, CONCAT('Assigned to Analyst ID: ', @AnalystID), SYSUTCDATETIME());
END
GO

-- Update Alert Status
CREATE OR ALTER PROCEDURE dbo.usp_UpdateAlertStatus
    @AlertID INT,
    @NewStatus NVARCHAR(20),
    @ActionBy INT = NULL,
    @Comments NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus NVARCHAR(20);
    SELECT @OldStatus = Status FROM FRMAlerts WHERE AlertID = @AlertID;

    UPDATE FRMAlerts
    SET Status = @NewStatus,
        LastUpdated = SYSUTCDATETIME(),
        ClosedDate = CASE WHEN @NewStatus IN ('Closed', 'FalsePositive') THEN SYSUTCDATETIME() ELSE ClosedDate END
    WHERE AlertID = @AlertID;

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@AlertID, 'StatusChanged', @OldStatus, @NewStatus, @ActionBy, @Comments, SYSUTCDATETIME());
END
GO

-- Escalate Alert
CREATE OR ALTER PROCEDURE dbo.usp_EscalateAlert
    @AlertID INT,
    @ActionBy INT = NULL,
    @Reason NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus NVARCHAR(20);
    SELECT @OldStatus = Status FROM FRMAlerts WHERE AlertID = @AlertID;

    UPDATE FRMAlerts
    SET Status = 'Escalated',
        Priority = 'Critical',
        LastUpdated = SYSUTCDATETIME()
    WHERE AlertID = @AlertID;

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@AlertID, 'Escalated', @OldStatus, 'Escalated', @ActionBy, @Reason, SYSUTCDATETIME());
END
GO

-- Close Alert
CREATE OR ALTER PROCEDURE dbo.usp_CloseAlert
    @AlertID INT,
    @Resolution NVARCHAR(100),
    @ResolutionNotes NVARCHAR(1000),
    @ActionBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OldStatus NVARCHAR(20);
    SELECT @OldStatus = Status FROM FRMAlerts WHERE AlertID = @AlertID;

    UPDATE FRMAlerts
    SET Status = 'Closed',
        Resolution = @Resolution,
        ResolutionNotes = @ResolutionNotes,
        ClosedDate = SYSUTCDATETIME(),
        LastUpdated = SYSUTCDATETIME()
    WHERE AlertID = @AlertID;

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@AlertID, 'Closed', @OldStatus, 'Closed', @ActionBy, CONCAT('Resolution: ', @Resolution, ' - ', @ResolutionNotes), SYSUTCDATETIME());
END
GO

-- Add Comment to Alert
CREATE OR ALTER PROCEDURE dbo.usp_AddAlertComment
    @AlertID INT,
    @AnalystID INT,
    @Comment NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO AlertComments (AlertID, AnalystID, Comment, Timestamp)
    VALUES (@AlertID, @AnalystID, @Comment, SYSUTCDATETIME());

    INSERT INTO AlertHistory (AlertID, Action, OldStatus, NewStatus, ActionBy, Comments, Timestamp)
    VALUES (@AlertID, 'Commented', NULL, NULL, @AnalystID, @Comment, SYSUTCDATETIME());
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_OpenAlerts AS
SELECT a.AlertID, a.AlertNumber, a.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       a.AccountID, acc.AccountNumber, a.TransactionID, a.AlertType, a.AlertCategory,
       a.Priority, a.Severity, a.Status, a.RiskScore, a.AssignedAnalystID,
       u.Username AS AssignedAnalystName, a.CreatedDate, a.LastUpdated,
       dbo.fn_AlertAge(a.AlertID) AS AgeHours
FROM FRMAlerts a
INNER JOIN Customers c ON c.CustomerId = a.CustomerID
INNER JOIN Accounts acc ON acc.AccountId = a.AccountID
LEFT JOIN Users u ON u.UserId = a.AssignedAnalystID
WHERE a.Status IN ('Open', 'InProgress', 'Escalated');
GO

CREATE OR ALTER VIEW dbo.vw_ClosedAlerts AS
SELECT a.AlertID, a.AlertNumber, a.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       a.AccountID, acc.AccountNumber, a.AlertType, a.AlertCategory,
       a.Priority, a.Severity, a.Status, a.RiskScore, a.Resolution, a.ResolutionNotes,
       a.AssignedAnalystID, u.Username AS AssignedAnalystName, a.CreatedDate, a.ClosedDate,
       dbo.fn_AlertAge(a.AlertID) AS DurationHours
FROM FRMAlerts a
INNER JOIN Customers c ON c.CustomerId = a.CustomerID
INNER JOIN Accounts acc ON acc.AccountId = a.AccountID
LEFT JOIN Users u ON u.UserId = a.AssignedAnalystID
WHERE a.Status IN ('Closed', 'FalsePositive');
GO

CREATE OR ALTER VIEW dbo.vw_CriticalAlerts AS
SELECT a.AlertID, a.AlertNumber, a.CustomerID, c.FirstName + ' ' + c.LastName AS CustomerName,
       a.AccountID, a.AlertType, a.Priority, a.Severity, a.Status, a.RiskScore,
       a.AssignedAnalystID, u.Username AS AssignedAnalystName, a.CreatedDate
FROM FRMAlerts a
INNER JOIN Customers c ON c.CustomerId = a.CustomerID
LEFT JOIN Users u ON u.UserId = a.AssignedAnalystID
WHERE a.Priority = 'Critical' OR a.Severity = 'Critical';
GO

CREATE OR ALTER VIEW dbo.vw_AnalystAssignments AS
SELECT u.UserId AS AnalystID, u.Username, u.Email,
       COUNT(a.AlertID) AS TotalAssignedAlerts,
       SUM(CASE WHEN a.Status = 'Open' THEN 1 ELSE 0 END) AS OpenAlerts,
       SUM(CASE WHEN a.Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgressAlerts,
       SUM(CASE WHEN a.Status = 'Escalated' THEN 1 ELSE 0 END) AS EscalatedAlerts,
       SUM(CASE WHEN a.Status = 'Closed' THEN 1 ELSE 0 END) AS ClosedAlerts
FROM Users u
LEFT JOIN FRMAlerts a ON a.AssignedAnalystID = u.UserId
GROUP BY u.UserId, u.Username, u.Email;
GO

/* =====================================================================
   TRIGGERS
   ===================================================================== */

-- Auto Update LastUpdated on FRMAlerts
CREATE OR ALTER TRIGGER trg_FRMAlerts_AutoUpdateTimestamp
ON FRMAlerts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE FRMAlerts
    SET LastUpdated = SYSUTCDATETIME()
    FROM FRMAlerts a
    INNER JOIN inserted i ON a.AlertID = i.AlertID;
END
GO
