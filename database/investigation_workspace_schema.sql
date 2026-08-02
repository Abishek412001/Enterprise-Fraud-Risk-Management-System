USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 6: Investigation Workspace
   investigation_workspace_schema.sql
   ===================================================================== */

-- 1. InvestigationSessions Table
IF OBJECT_ID('dbo.InvestigationSessions', 'U') IS NULL
BEGIN
    CREATE TABLE InvestigationSessions (
        SessionID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        AnalystID INT NOT NULL,
        StartTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        EndTime DATETIME2 NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Active', -- Active | Completed | Suspended
        SummaryNotes NVARCHAR(1000) NULL,
        CONSTRAINT FK_InvestigationSessions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_InvestigationSessions_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId)
    );
END
GO

-- 2. AnalystActions Table
IF OBJECT_ID('dbo.AnalystActions', 'U') IS NULL
BEGIN
    CREATE TABLE AnalystActions (
        ActionID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        AnalystID INT NOT NULL,
        SessionID INT NULL,
        ActionType NVARCHAR(50) NOT NULL, -- FreezeAccount | UnfreezeAccount | SuspendCard | ActivateCard | BlockDevice | TrustDevice | RequestVerification
        Reason NVARCHAR(500) NOT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Details NVARCHAR(1000) NULL,
        CONSTRAINT FK_AnalystActions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_AnalystActions_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId),
        CONSTRAINT FK_AnalystActions_Session FOREIGN KEY (SessionID) REFERENCES InvestigationSessions(SessionID)
    );
END
GO

-- 3. InvestigationTimeline Table
IF OBJECT_ID('dbo.InvestigationTimeline', 'U') IS NULL
BEGIN
    CREATE TABLE InvestigationTimeline (
        TimelineID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        EventCategory NVARCHAR(50) NOT NULL, -- Alert | Action | Transaction | RiskUpdate | AccountChange
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        PerformedBy INT NULL,
        CONSTRAINT FK_InvestigationTimeline_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_InvestigationTimeline_User FOREIGN KEY (PerformedBy) REFERENCES Users(UserId)
    );
END
GO

-- 4. Evidence Table
IF OBJECT_ID('dbo.Evidence', 'U') IS NULL
BEGIN
    CREATE TABLE Evidence (
        EvidenceID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        SessionID INT NULL,
        EvidenceType NVARCHAR(50) NOT NULL, -- IPLog | Document | Screenshot | TransactionReceipt | DeviceSignature
        Title NVARCHAR(200) NOT NULL,
        FileLocation NVARCHAR(500) NULL,
        UploadedBy INT NOT NULL,
        UploadDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_Evidence_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_Evidence_Session FOREIGN KEY (SessionID) REFERENCES InvestigationSessions(SessionID),
        CONSTRAINT FK_Evidence_User FOREIGN KEY (UploadedBy) REFERENCES Users(UserId)
    );
END
GO

-- 5. DeviceTrust Table
IF OBJECT_ID('dbo.DeviceTrust', 'U') IS NULL
BEGIN
    CREATE TABLE DeviceTrust (
        TrustID INT IDENTITY(1,1) PRIMARY KEY,
        DeviceID INT NOT NULL,
        TrustScore INT NOT NULL DEFAULT 50, -- 0 (High Risk/Blocked) to 100 (Fully Trusted)
        Status NVARCHAR(20) NOT NULL DEFAULT 'Untrusted', -- Trusted | Untrusted | Blocked
        LastEvaluated DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DeviceTrust_Devices FOREIGN KEY (DeviceID) REFERENCES Devices(DeviceID) ON DELETE CASCADE
    );
END
GO

-- 6. CustomerRiskHistory Table
IF OBJECT_ID('dbo.CustomerRiskHistory', 'U') IS NULL
BEGIN
    CREATE TABLE CustomerRiskHistory (
        HistoryID INT IDENTITY(1,1) PRIMARY KEY,
        CustomerID INT NOT NULL,
        OldRiskScore INT NOT NULL,
        NewRiskScore INT NOT NULL,
        ChangeReason NVARCHAR(255) NOT NULL,
        ChangedBy INT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CustomerRiskHistory_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_CustomerRiskHistory_User FOREIGN KEY (ChangedBy) REFERENCES Users(UserId)
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AnalystActions_CustomerID')
    CREATE INDEX IX_AnalystActions_CustomerID ON AnalystActions(CustomerID, Timestamp DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_InvestigationTimeline_CustomerID')
    CREATE INDEX IX_InvestigationTimeline_CustomerID ON InvestigationTimeline(CustomerID, Timestamp DESC);
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.usp_StartInvestigation
    @CustomerID INT,
    @AnalystID INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO InvestigationSessions (CustomerID, AnalystID, StartTime, Status)
    VALUES (@CustomerID, @AnalystID, SYSUTCDATETIME(), 'Active');

    DECLARE @SessionID INT = SCOPE_IDENTITY();

    INSERT INTO InvestigationTimeline (CustomerID, EventCategory, Title, Description, Timestamp, PerformedBy)
    VALUES (@CustomerID, 'Action', 'Investigation Started', 'Analyst initiated investigation session.', SYSUTCDATETIME(), @AnalystID);

    SELECT @SessionID AS SessionID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CloseInvestigation
    @SessionID INT,
    @SummaryNotes NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE InvestigationSessions
    SET EndTime = SYSUTCDATETIME(), Status = 'Completed', SummaryNotes = @SummaryNotes
    WHERE SessionID = @SessionID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RecordAnalystAction
    @CustomerID INT,
    @AnalystID INT,
    @SessionID INT = NULL,
    @ActionType NVARCHAR(50),
    @Reason NVARCHAR(500),
    @Details NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AnalystActions (CustomerID, AnalystID, SessionID, ActionType, Reason, Timestamp, Details)
    VALUES (@CustomerID, @AnalystID, @SessionID, @ActionType, @Reason, SYSUTCDATETIME(), @Details);

    INSERT INTO InvestigationTimeline (CustomerID, EventCategory, Title, Description, Timestamp, PerformedBy)
    VALUES (@CustomerID, 'Action', @ActionType, @Reason, SYSUTCDATETIME(), @AnalystID);
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_FreezeCustomerAccount
    @CustomerID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Customers SET IsBlacklisted = 1 WHERE CustomerId = @CustomerID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'FreezeAccount',
        @Reason = @Reason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_UnfreezeCustomerAccount
    @CustomerID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Customers SET IsBlacklisted = 0 WHERE CustomerId = @CustomerID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'UnfreezeAccount',
        @Reason = @Reason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_SuspendCard
    @CardID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Cards SET CardStatus = 'Blocked' WHERE CardId = @CardID;

    DECLARE @CustomerID INT;
    SELECT TOP 1 @CustomerID = a.CustomerId FROM Cards c INNER JOIN Accounts a ON a.AccountId = c.AccountId WHERE c.CardId = @CardID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'SuspendCard',
        @Reason = @Reason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ActivateCard
    @CardID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Cards SET CardStatus = 'Active' WHERE CardId = @CardID;

    DECLARE @CustomerID INT;
    SELECT TOP 1 @CustomerID = a.CustomerId FROM Cards c INNER JOIN Accounts a ON a.AccountId = c.AccountId WHERE c.CardId = @CardID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'ActivateCard',
        @Reason = @Reason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_BlockDevice
    @DeviceID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Devices SET IsBlocked = 1, IsTrusted = 0 WHERE DeviceID = @DeviceID;

    DECLARE @CustomerID INT;
    SELECT @CustomerID = CustomerID FROM Devices WHERE DeviceID = @DeviceID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'BlockDevice',
        @Reason = @Reason;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_TrustDevice
    @DeviceID INT,
    @AnalystID INT,
    @Reason NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Devices SET IsBlocked = 0, IsTrusted = 1 WHERE DeviceID = @DeviceID;

    DECLARE @CustomerID INT;
    SELECT @CustomerID = CustomerID FROM Devices WHERE DeviceID = @DeviceID;

    EXEC dbo.usp_RecordAnalystAction 
        @CustomerID = @CustomerID,
        @AnalystID = @AnalystID,
        @ActionType = 'TrustDevice',
        @Reason = @Reason;
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_Customer360 AS
SELECT c.CustomerId, c.FirstName + ' ' + c.LastName AS FullName, c.Email, c.Phone,
       c.KycStatus, c.AmlRiskLevel, c.IsBlacklisted AS IsFrozen, c.CreatedDate AS CustomerSince,
       crs.RiskScore AS CurrentRiskScore, crs.RiskCategory
FROM Customers c
LEFT JOIN CustomerRiskScores crs ON crs.CustomerId = c.CustomerId;
GO
