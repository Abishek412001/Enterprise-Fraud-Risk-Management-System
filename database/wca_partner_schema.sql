USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 7: WCA & Partner Comms
   wca_partner_schema.sql
   ===================================================================== */

-- 1. WCAInteractions Table
IF OBJECT_ID('dbo.WCAInteractions', 'U') IS NULL
BEGIN
    CREATE TABLE WCAInteractions (
        InteractionID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NULL,
        AlertID INT NULL,
        CustomerID INT NOT NULL,
        AnalystID INT NOT NULL,
        ActionType NVARCHAR(50) NOT NULL, -- FreezeAccount | UnfreezeAccount | SuspendCard | BlockDevice | PartnerContacted | FraudConfirmed
        ActionCategory NVARCHAR(50) NOT NULL DEFAULT 'InvestigationAction',
        ActionDescription NVARCHAR(1000) NOT NULL,
        Comments NVARCHAR(1000) NULL,
        StatusBefore NVARCHAR(50) NULL,
        StatusAfter NVARCHAR(50) NULL,
        Timestamp DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WCAInteractions_Customers FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerId),
        CONSTRAINT FK_WCAInteractions_Analyst FOREIGN KEY (AnalystID) REFERENCES Users(UserId),
        CONSTRAINT FK_WCAInteractions_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID)
    );
END
GO

-- 2. PartnerDirectory Table
IF OBJECT_ID('dbo.PartnerDirectory', 'U') IS NULL
BEGIN
    CREATE TABLE PartnerDirectory (
        PartnerID INT IDENTITY(1,1) PRIMARY KEY,
        PartnerName NVARCHAR(150) NOT NULL, -- Visa Risk Operations | Mastercard Fraud Net | Law Enforcement Cyber Unit | Equifax Fraud Desk
        Department NVARCHAR(100) NOT NULL DEFAULT 'Fraud Operations',
        Email NVARCHAR(150) NOT NULL,
        Phone NVARCHAR(50) NULL,
        EscalationContact NVARCHAR(150) NULL
    );
END
GO

-- 3. PartnerCommunications Table
IF OBJECT_ID('dbo.PartnerCommunications', 'U') IS NULL
BEGIN
    CREATE TABLE PartnerCommunications (
        CommunicationID INT IDENTITY(1,1) PRIMARY KEY,
        CaseID INT NULL,
        PartnerID INT NOT NULL,
        PartnerName NVARCHAR(150) NOT NULL,
        CommunicationType NVARCHAR(50) NOT NULL DEFAULT 'InformationRequest', -- InformationRequest | ChargebackDispute | LawEnforcementSubpoena
        Direction NVARCHAR(20) NOT NULL DEFAULT 'Outbound', -- Outbound | Inbound
        Channel NVARCHAR(50) NOT NULL DEFAULT 'Email',       -- Email | Phone | Portal | SecureMessaging
        Subject NVARCHAR(255) NOT NULL,
        Message NVARCHAR(MAX) NOT NULL,
        Status NVARCHAR(50) NOT NULL DEFAULT 'Sent',          -- Sent | ResponseReceived | PendingResponse | Closed
        SentDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        ReceivedDate DATETIME2 NULL,
        CONSTRAINT FK_PartnerCommunications_Partner FOREIGN KEY (PartnerID) REFERENCES PartnerDirectory(PartnerID),
        CONSTRAINT FK_PartnerCommunications_Cases FOREIGN KEY (CaseID) REFERENCES Cases(CaseID)
    );
END
GO

-- 4. CommunicationTemplates Table
IF OBJECT_ID('dbo.CommunicationTemplates', 'U') IS NULL
BEGIN
    CREATE TABLE CommunicationTemplates (
        TemplateID INT IDENTITY(1,1) PRIMARY KEY,
        TemplateName NVARCHAR(100) NOT NULL,
        Category NVARCHAR(50) NOT NULL DEFAULT 'AccountStatus',
        Subject NVARCHAR(255) NOT NULL,
        MessageBody NVARCHAR(MAX) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1
    );
END
GO

-- Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_WCAInteractions_CustomerID')
    CREATE INDEX IX_WCAInteractions_CustomerID ON WCAInteractions(CustomerID, Timestamp DESC);
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PartnerCommunications_PartnerID')
    CREATE INDEX IX_PartnerCommunications_PartnerID ON PartnerCommunications(PartnerID, SentDate DESC);
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.usp_RecordWCAInteraction
    @CaseID INT = NULL,
    @AlertID INT = NULL,
    @CustomerID INT,
    @AnalystID INT,
    @ActionType NVARCHAR(50),
    @ActionCategory NVARCHAR(50) = 'InvestigationAction',
    @ActionDescription NVARCHAR(1000),
    @Comments NVARCHAR(1000) = NULL,
    @StatusBefore NVARCHAR(50) = NULL,
    @StatusAfter NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO WCAInteractions (CaseID, AlertID, CustomerID, AnalystID, ActionType, ActionCategory, ActionDescription, Comments, StatusBefore, StatusAfter, Timestamp)
    VALUES (@CaseID, @AlertID, @CustomerID, @AnalystID, @ActionType, @ActionCategory, @ActionDescription, @Comments, @StatusBefore, @StatusAfter, SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS InteractionID;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_CreatePartnerCommunication
    @CaseID INT = NULL,
    @PartnerID INT,
    @PartnerName NVARCHAR(150),
    @CommunicationType NVARCHAR(50),
    @Channel NVARCHAR(50) = 'Email',
    @Subject NVARCHAR(255),
    @Message NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO PartnerCommunications (CaseID, PartnerID, PartnerName, CommunicationType, Direction, Channel, Subject, Message, Status, SentDate)
    VALUES (@CaseID, @PartnerID, @PartnerName, @CommunicationType, 'Outbound', @Channel, @Subject, @Message, 'PendingResponse', SYSUTCDATETIME());

    SELECT SCOPE_IDENTITY() AS CommunicationID;
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_WCAInteractions AS
SELECT w.InteractionID, w.CaseID, w.AlertID, w.CustomerID,
       cust.FirstName + ' ' + cust.LastName AS CustomerName,
       w.AnalystID, u.Username AS AnalystName,
       w.ActionType, w.ActionCategory, w.ActionDescription, w.Comments,
       w.StatusBefore, w.StatusAfter, w.Timestamp
FROM WCAInteractions w
INNER JOIN Customers cust ON cust.CustomerId = w.CustomerID
INNER JOIN Users u ON u.UserId = w.AnalystID;
GO

CREATE OR ALTER VIEW dbo.vw_PartnerCommunicationHistory AS
SELECT pc.CommunicationID, pc.CaseID, pc.PartnerID, pc.PartnerName,
       pc.CommunicationType, pc.Direction, pc.Channel, pc.Subject, pc.Message,
       pc.Status, pc.SentDate, pc.ReceivedDate
FROM PartnerCommunications pc;
GO
