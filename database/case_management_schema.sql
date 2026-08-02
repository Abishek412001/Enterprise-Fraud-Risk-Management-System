USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 5: Case Management & SLA
   case_management_schema.sql
   ===================================================================== */

-- 1. Cases Table
IF OBJECT_ID('dbo.Cases', 'U') IS NULL
BEGIN
    CREATE TABLE Cases (
        CaseID INT IDENTITY(1,1) PRIMARY KEY,
        CaseNumber NVARCHAR(50) NOT NULL UNIQUE,
        CaseType NVARCHAR(50) NOT NULL DEFAULT 'FraudInvestigation', -- FraudInvestigation | ATOInvestigation | SIEMIncident | Chargeback
        CaseTitle NVARCHAR(200) NOT NULL,
        CaseDescription NVARCHAR(1000) NULL,
        CustomerID INT NOT NULL,
        Priority NVARCHAR(20) NOT NULL DEFAULT 'Medium',  -- Low | Medium | High | Critical
        Severity NVARCHAR(20) NOT NULL DEFAULT 'Medium',  -- Low | Medium | High | Critical
        Status NVARCHAR(20) NOT NULL DEFAULT 'Open',       -- Open | InProgress | Escalated | PendingPartner | Resolved | Closed
        AssignedAnalystID INT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        DueDate DATETIME2 NOT NULL,
        ResolvedDate DATETIME2 NULL,
        ClosedDate DATETIME2 NULL,
        RootCause NVARCHAR(100) NULL,
        Resolution NVARCHAR(100) NULL,
        FalsePositive BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Cases_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_Cases_Analyst FOREIGN KEY (AssignedAnalystID) REFERENCES Users(UserId),
        CONSTRAINT CK_Cases_Priority CHECK (Priority IN ('Low','Medium','High','Critical')),
        CONSTRAINT CK_Cases_Status CHECK (Status IN ('Open','InProgress','Escalated','PendingPartner','Resolved','Closed'))
    );
END
GO

-- 2. CaseAlerts Table
IF OBJECT_ID('dbo.CaseAlerts', 'U') IS NULL
BEGIN
    CREATE TABLE CaseAlerts (
        CaseAlertID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        AlertType NVARCHAR(50) NOT NULL, -- FRM | ATO | Sentinel | Legacy
        AlertID INT NOT NULL,
        LinkedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CaseAlerts_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE
    );
END
GO

-- 3. CaseTransactions Table
IF OBJECT_ID('dbo.CaseTransactions', 'U') IS NULL
BEGIN
    CREATE TABLE CaseTransactions (
        CaseTransactionID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        TransactionID BIGINT NOT NULL,
        LinkedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CaseTransactions_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT FK_CaseTransactions_Transactions FOREIGN KEY (TransactionID) REFERENCES Transactions(TransactionId)
    );
END
GO

-- 4. CaseNotes Table
IF OBJECT_ID('dbo.CaseNotes', 'U') IS NULL
BEGIN
    CREATE TABLE CaseNotes (
        NoteID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        AnalystID INT NOT NULL,
        NoteType NVARCHAR(50) NOT NULL DEFAULT 'InvestigationNote', -- InvestigationNote | Evidence | EscalationReason | PartnerUpdate
        Comment NVARCHAR(1000) NOT NULL,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CaseNotes_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT FK_CaseNotes_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId)
    );
END
GO

-- 5. CaseTimeline Table
IF OBJECT_ID('dbo.CaseTimeline', 'U') IS NULL
BEGIN
    CREATE TABLE CaseTimeline (
        TimelineID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        Action NVARCHAR(100) NOT NULL,  -- Created | AlertLinked | TransactionLinked | Assigned | StatusChanged | AccountFrozen | Closed
        ActionBy INT NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        Details NVARCHAR(1000) NULL,
        CONSTRAINT FK_CaseTimeline_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT FK_CaseTimeline_User FOREIGN KEY (ActionBy) REFERENCES Users(UserId)
    );
END
GO

-- 6. CaseAttachments Table
IF OBJECT_ID('dbo.CaseAttachments', 'U') IS NULL
BEGIN
    CREATE TABLE CaseAttachments (
        AttachmentID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        FileName NVARCHAR(255) NOT NULL,
        FileType NVARCHAR(50) NOT NULL,
        UploadedBy INT NOT NULL,
        UploadDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CaseAttachments_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT FK_CaseAttachments_User FOREIGN KEY (UploadedBy) REFERENCES Users(UserId)
    );
END
GO

-- 7. CaseEscalations Table
IF OBJECT_ID('dbo.CaseEscalations', 'U') IS NULL
BEGIN
    CREATE TABLE CaseEscalations (
        EscalationID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL,
        EscalatedTo INT NOT NULL,
        EscalationReason NVARCHAR(1000) NOT NULL,
        EscalationDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_CaseEscalations_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT FK_CaseEscalations_User FOREIGN KEY (EscalatedTo) REFERENCES Users(UserId)
    );
END
GO

-- 8. SLATracking Table
IF OBJECT_ID('dbo.SLATracking', 'U') IS NULL
BEGIN
    CREATE TABLE SLATracking (
        SLAID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NOT NULL UNIQUE,
        StartTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        TargetResolution DATETIME2 NOT NULL,
        ActualResolution DATETIME2 NULL,
        SLAStatus NVARCHAR(20) NOT NULL DEFAULT 'OnTrack', -- OnTrack | NearBreach | Breached | Met
        CONSTRAINT FK_SLATracking_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID) ON DELETE CASCADE,
        CONSTRAINT CK_SLATracking_Status CHECK (SLAStatus IN ('OnTrack','NearBreach','Breached','Met'))
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cases_Status_Priority')
    CREATE INDEX IX_Cases_Status_Priority ON Cases(Status, Priority);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cases_CustomerID')
    CREATE INDEX IX_Cases_CustomerID ON Cases(CustomerID);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_SLATracking_Status')
    CREATE INDEX IX_SLATracking_Status ON SLATracking(SLAStatus, TargetResolution);
GO

/* =====================================================================
   FUNCTIONS
   ===================================================================== */

CREATE OR ALTER FUNCTION dbo.fn_CaseAge (@CaseID INT)
RETURNS INT
AS
BEGIN
    DECLARE @AgeHours INT = 0;
    DECLARE @CreatedDate DATETIME2, @ClosedDate DATETIME2;
    SELECT @CreatedDate = CreatedDate, @ClosedDate = ClosedDate FROM Cases WHERE CaseID = @CaseID;
    IF (@CreatedDate IS NOT NULL)
        SET @AgeHours = DATEDIFF(HOUR, @CreatedDate, ISNULL(@ClosedDate, SYSUTCDATETIME()));
    RETURN @AgeHours;
END
GO

CREATE OR ALTER FUNCTION dbo.fn_SLAStatus (@CaseID INT)
RETURNS NVARCHAR(20)
AS
BEGIN
    DECLARE @Status NVARCHAR(20) = 'OnTrack';
    DECLARE @TargetResolution DATETIME2, @ActualResolution DATETIME2;

    SELECT @TargetResolution = TargetResolution, @ActualResolution = ActualResolution
    FROM SLATracking WHERE CaseID = @CaseID;

    IF (@ActualResolution IS NOT NULL)
    BEGIN
        IF (@ActualResolution <= @TargetResolution) SET @Status = 'Met';
        ELSE SET @Status = 'Breached';
    END
    ELSE
    BEGIN
        IF (SYSUTCDATETIME() > @TargetResolution) SET @Status = 'Breached';
        ELSE IF (DATEDIFF(MINUTE, SYSUTCDATETIME(), @TargetResolution) <= 60) SET @Status = 'NearBreach';
        ELSE SET @Status = 'OnTrack';
    END

    RETURN @Status;
END
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

-- Add Timeline Entry
CREATE OR ALTER PROCEDURE dbo.usp_AddTimelineEntry
    @CaseID INT,
    @Action NVARCHAR(100),
    @ActionBy INT = NULL,
    @Details NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO CaseTimeline (CaseID, Action, ActionBy, Timestamp, Details)
    VALUES (@CaseID, @Action, @ActionBy, SYSUTCDATETIME(), @Details);
END
GO

-- Create Case
CREATE OR ALTER PROCEDURE dbo.usp_CreateCase
    @CaseType NVARCHAR(50),
    @CaseTitle NVARCHAR(200),
    @CaseDescription NVARCHAR(1000),
    @CustomerID INT,
    @Priority NVARCHAR(20) = 'Medium',
    @Severity NVARCHAR(20) = 'Medium',
    @CreatedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CaseNumber NVARCHAR(50);
    SET @CaseNumber = CONCAT('CAS-', FORMAT(SYSUTCDATETIME(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR(4)), 4));

    -- Calculate SLA Target Resolution Date based on Priority
    DECLARE @Hours INT = CASE 
        WHEN @Priority = 'Critical' THEN 2
        WHEN @Priority = 'High' THEN 8
        WHEN @Priority = 'Medium' THEN 24
        ELSE 72
    END;

    DECLARE @DueDate DATETIME2 = DATEADD(HOUR, @Hours, SYSUTCDATETIME());

    INSERT INTO Cases (CaseNumber, CaseType, CaseTitle, CaseDescription, CustomerID, Priority, Severity, Status, CreatedDate, DueDate)
    VALUES (@CaseNumber, @CaseType, @CaseTitle, @CaseDescription, @CustomerID, @Priority, @Severity, 'Open', SYSUTCDATETIME(), @DueDate);

    DECLARE @NewCaseID INT = SCOPE_IDENTITY();

    -- Create SLA Tracking Entry
    INSERT INTO SLATracking (CaseID, StartTime, TargetResolution, SLAStatus)
    VALUES (@NewCaseID, SYSUTCDATETIME(), @DueDate, 'OnTrack');

    -- Record Timeline
    EXEC dbo.usp_AddTimelineEntry @CaseID = @NewCaseID, @Action = 'Case Created', @ActionBy = @CreatedBy, @Details = @CaseTitle;

    SELECT @NewCaseID AS CaseID, @CaseNumber AS CaseNumber;
END
GO

-- Assign Case
CREATE OR ALTER PROCEDURE dbo.usp_AssignCase
    @CaseID INT,
    @AnalystID INT,
    @AssignedBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Cases
    SET AssignedAnalystID = @AnalystID, Status = CASE WHEN Status = 'Open' THEN 'InProgress' ELSE Status END
    WHERE CaseID = @CaseID;

    EXEC dbo.usp_AddTimelineEntry @CaseID = @CaseID, @Action = 'Assigned to Analyst', @ActionBy = @AssignedBy, @Details = CONCAT('Assigned to Analyst ID: ', @AnalystID);
END
GO

-- Escalate Case
CREATE OR ALTER PROCEDURE dbo.usp_EscalateCase
    @CaseID INT,
    @EscalatedTo INT,
    @EscalationReason NVARCHAR(1000),
    @ActionBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Cases
    SET Status = 'Escalated', Priority = 'Critical'
    WHERE CaseID = @CaseID;

    INSERT INTO CaseEscalations (CaseID, EscalatedTo, EscalationReason, EscalationDate)
    VALUES (@CaseID, @EscalatedTo, @EscalationReason, SYSUTCDATETIME());

    EXEC dbo.usp_AddTimelineEntry @CaseID = @CaseID, @Action = 'Case Escalated', @ActionBy = @ActionBy, @Details = @EscalationReason;
END
GO

-- Close Case
CREATE OR ALTER PROCEDURE dbo.usp_CloseCase
    @CaseID INT,
    @Resolution NVARCHAR(100),
    @RootCause NVARCHAR(100),
    @FalsePositive BIT = 0,
    @ActionBy INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    UPDATE Cases
    SET Status = 'Closed',
        Resolution = @Resolution,
        RootCause = @RootCause,
        FalsePositive = @FalsePositive,
        ResolvedDate = @Now,
        ClosedDate = @Now
    WHERE CaseID = @CaseID;

    -- Update SLA Tracking
    DECLARE @SlaStatus NVARCHAR(20) = dbo.fn_SLAStatus(@CaseID);
    UPDATE SLATracking
    SET ActualResolution = @Now, SLAStatus = @SlaStatus
    WHERE CaseID = @CaseID;

    EXEC dbo.usp_AddTimelineEntry @CaseID = @CaseID, @Action = 'Case Closed', @ActionBy = @ActionBy, @Details = CONCAT('Resolution: ', @Resolution);
END
GO

-- Add Case Note
CREATE OR ALTER PROCEDURE dbo.usp_AddCaseNote
    @CaseID INT,
    @AnalystID INT,
    @NoteType NVARCHAR(50) = 'InvestigationNote',
    @Comment NVARCHAR(1000)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO CaseNotes (CaseID, AnalystID, NoteType, Comment, CreatedDate)
    VALUES (@CaseID, @AnalystID, @NoteType, @Comment, SYSUTCDATETIME());

    EXEC dbo.usp_AddTimelineEntry @CaseID = @CaseID, @Action = 'Note Added', @ActionBy = @AnalystID, @Details = @Comment;
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_OpenCases AS
SELECT c.CaseID, c.CaseNumber, c.CaseType, c.CaseTitle, c.CustomerID,
       cust.FirstName + ' ' + cust.LastName AS CustomerName,
       c.Priority, c.Severity, c.Status, c.AssignedAnalystID, u.Username AS AssignedAnalystName,
       c.CreatedDate, c.DueDate, sla.SLAStatus, dbo.fn_CaseAge(c.CaseID) AS AgeHours
FROM Cases c
INNER JOIN Customers cust ON cust.CustomerId = c.CustomerID
LEFT JOIN Users u ON u.UserId = c.AssignedAnalystID
LEFT JOIN SLATracking sla ON sla.CaseID = c.CaseID
WHERE c.Status IN ('Open', 'InProgress', 'Escalated', 'PendingPartner');
GO

CREATE OR ALTER VIEW dbo.vw_SLABreaches AS
SELECT c.CaseID, c.CaseNumber, c.CaseTitle, c.Priority, c.Status,
       c.AssignedAnalystID, u.Username AS AssignedAnalystName,
       sla.StartTime, sla.TargetResolution, sla.ActualResolution, sla.SLAStatus
FROM Cases c
INNER JOIN SLATracking sla ON sla.CaseID = c.CaseID
LEFT JOIN Users u ON u.UserId = c.AssignedAnalystID
WHERE sla.SLAStatus = 'Breached';
GO
