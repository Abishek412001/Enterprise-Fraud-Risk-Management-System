USE EnterpriseFraudRiskDB;
GO

/* =====================================================================
   Enterprise Fraud Risk Management System - Phase 9: Enterprise Security & RBAC
   security_rbac_schema.sql
   ===================================================================== */

-- 1. Roles Table
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE Roles (
        RoleId INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(255) NULL
    );

    INSERT INTO Roles (RoleName, Description) VALUES
    ('Administrator', 'Full system access and security administration'),
    ('FRM Manager', 'Fraud Risk Management Operations Manager'),
    ('Senior Fraud Analyst', 'Lead investigator with escalation rights'),
    ('Fraud Analyst', 'Standard analyst conducting investigations'),
    ('Risk Analyst', 'Risk scoring and telemetry specialist'),
    ('Partner Support', 'External partner liaisons'),
    ('Compliance Officer', 'Audit and compliance reporting officer'),
    ('Auditor', 'Read-only compliance auditor'),
    ('Read Only User', 'View-only access across panels');
END
GO

-- 2. Permissions Table
IF OBJECT_ID('dbo.Permissions', 'U') IS NULL
BEGIN
    CREATE TABLE Permissions (
        PermissionId INT IDENTITY(1,1) PRIMARY KEY,
        PermissionName NVARCHAR(100) NOT NULL UNIQUE,
        Category NVARCHAR(50) NOT NULL
    );

    INSERT INTO Permissions (PermissionName, Category) VALUES
    ('ManageUsers', 'Security'),
    ('AssignAlerts', 'Alerts'),
    ('CreateCases', 'Cases'),
    ('CloseCases', 'Cases'),
    ('FreezeAccounts', 'Actions'),
    ('UnfreezeAccounts', 'Actions'),
    ('ViewReports', 'Reports'),
    ('ExportReports', 'Reports'),
    ('ManageFraudRules', 'Rules'),
    ('ViewAuditLogs', 'Compliance');
END
GO

-- 3. RolePermissions Table
IF OBJECT_ID('dbo.RolePermissions', 'U') IS NULL
BEGIN
    CREATE TABLE RolePermissions (
        RolePermissionId INT IDENTITY(1,1) PRIMARY KEY,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(PermissionId) ON DELETE CASCADE
    );
END
GO

-- 4. RefreshTokens Table
IF OBJECT_ID('dbo.RefreshTokens', 'U') IS NULL
BEGIN
    CREATE TABLE RefreshTokens (
        TokenId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        Token NVARCHAR(500) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        CreatedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
END
GO

-- 5. UserSessions Table
IF OBJECT_ID('dbo.UserSessions', 'U') IS NULL
BEGIN
    CREATE TABLE UserSessions (
        SessionId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        IpAddress NVARCHAR(45) NOT NULL,
        UserAgent NVARCHAR(500) NULL,
        LoginTime DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        LastActivity DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_UserSessions_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
END
GO

-- 6. PasswordHistory Table
IF OBJECT_ID('dbo.PasswordHistory', 'U') IS NULL
BEGIN
    CREATE TABLE PasswordHistory (
        HistoryId INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        ChangedDate DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_PasswordHistory_User FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
    );
END
GO

/* =====================================================================
   STORED PROCEDURES
   ===================================================================== */

CREATE OR ALTER PROCEDURE dbo.usp_AssignRole
    @UserId INT,
    @RoleName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET Role = @RoleName WHERE UserId = @UserId;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RecordAuditLog
    @UserId INT = NULL,
    @Action NVARCHAR(100),
    @EntityType NVARCHAR(50),
    @EntityId NVARCHAR(50) = NULL,
    @Details NVARCHAR(1000) = NULL,
    @IpAddress NVARCHAR(45) = '127.0.0.1'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AuditLogs (UserId, Action, EntityType, EntityId, Details, IpAddress, Timestamp)
    VALUES (@UserId, @Action, @EntityType, @EntityId, @Details, @IpAddress, SYSUTCDATETIME());
END
GO

/* =====================================================================
   VIEWS
   ===================================================================== */

CREATE OR ALTER VIEW dbo.vw_UserPermissions AS
SELECT u.UserId, u.Username, u.Email, u.Role
FROM Users u;
GO

CREATE OR ALTER VIEW dbo.vw_SecurityEvents AS
SELECT a.AuditId AS EventId, a.UserId, u.Username, a.Action AS EventType, a.IpAddress, a.Timestamp
FROM AuditLogs a
LEFT JOIN Users u ON u.UserId = a.UserId;
GO
