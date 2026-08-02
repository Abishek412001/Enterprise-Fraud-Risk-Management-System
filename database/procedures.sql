USE EnterpriseFraudRiskDB;
GO

/* =============================================
   usp_RegisterUser
   PasswordHash must already be BCrypt-hashed by the application layer.
   ============================================= */
CREATE OR ALTER PROCEDURE usp_RegisterUser
    @Username NVARCHAR(50),
    @Email NVARCHAR(150),
    @PasswordHash NVARCHAR(255),
    @Role NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username OR Email = @Email)
    BEGIN
        RAISERROR('Username or email already exists.', 16, 1);
        RETURN;
    END

    INSERT INTO Users (Username, Email, PasswordHash, Role)
    VALUES (@Username, @Email, @PasswordHash, @Role);

    SELECT SCOPE_IDENTITY() AS NewUserId;
END
GO

/* =============================================
   usp_LoginUser
   Returns the stored hash for the app layer to verify with BCrypt.Verify,
   and records the login attempt via a separate call to usp_RecordLoginAttempt.
   ============================================= */
CREATE OR ALTER PROCEDURE usp_LoginUser
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT UserId, Username, Email, PasswordHash, Role, IsActive, FailedLoginCount
    FROM Users
    WHERE Username = @Username;
END
GO

CREATE OR ALTER PROCEDURE usp_RecordLoginAttempt
    @UserId INT,
    @IpAddress NVARCHAR(45),
    @IsSuccessful BIT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO LoginHistory (UserId, IpAddress, IsSuccessful)
    VALUES (@UserId, @IpAddress, @IsSuccessful);

    IF (@IsSuccessful = 1)
        UPDATE Users SET FailedLoginCount = 0, LastLoginAt = SYSUTCDATETIME() WHERE UserId = @UserId;
    ELSE
        UPDATE Users SET FailedLoginCount = FailedLoginCount + 1 WHERE UserId = @UserId;
END
GO

/* =============================================
   usp_CreateCustomer
   ============================================= */
CREATE OR ALTER PROCEDURE usp_CreateCustomer
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(150),
    @Phone NVARCHAR(20),
    @NationalIdNumber NVARCHAR(50),
    @DateOfBirth DATE,
    @Address NVARCHAR(255),
    @City NVARCHAR(100),
    @Country NVARCHAR(100),
    @CreatedByUserId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Customers (FirstName, LastName, Email, Phone, NationalIdNumber, DateOfBirth, Address, City, Country, CreatedByUserId)
    VALUES (@FirstName, @LastName, @Email, @Phone, @NationalIdNumber, @DateOfBirth, @Address, @City, @Country, @CreatedByUserId);

    DECLARE @NewId INT = SCOPE_IDENTITY();

    INSERT INTO CustomerRiskScore (CustomerId, Score, RiskLevel)
    VALUES (@NewId, 0, 'Low');

    SELECT @NewId AS NewCustomerId;
END
GO

/* =============================================
   usp_CreateAccount
   ============================================= */
CREATE OR ALTER PROCEDURE usp_CreateAccount
    @CustomerId INT,
    @AccountNumber NVARCHAR(34),
    @AccountType NVARCHAR(20),
    @Currency CHAR(3),
    @OpeningBalance DECIMAL(18,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Accounts (CustomerId, AccountNumber, AccountType, Currency, Balance)
    VALUES (@CustomerId, @AccountNumber, @AccountType, @Currency, @OpeningBalance);

    SELECT SCOPE_IDENTITY() AS NewAccountId;
END
GO

/* =============================================
   usp_IssueCard
   ============================================= */
CREATE OR ALTER PROCEDURE usp_IssueCard
    @AccountId INT,
    @CardNumberMasked NVARCHAR(25),
    @CardNumberHash NVARCHAR(255),
    @CardType NVARCHAR(20),
    @ExpiryDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Cards (AccountId, CardNumberMasked, CardNumberHash, CardType, ExpiryDate)
    VALUES (@AccountId, @CardNumberMasked, @CardNumberHash, @CardType, @ExpiryDate);

    SELECT SCOPE_IDENTITY() AS NewCardId;
END
GO

CREATE OR ALTER PROCEDURE usp_BlockCard
    @CardId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Cards SET Status = 'Blocked', ModifiedAt = SYSUTCDATETIME() WHERE CardId = @CardId;
END
GO

CREATE OR ALTER PROCEDURE usp_ReplaceCard
    @OldCardId INT,
    @NewCardNumberMasked NVARCHAR(25),
    @NewCardNumberHash NVARCHAR(255),
    @NewExpiryDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AccountId INT, @CardType NVARCHAR(20);
    SELECT @AccountId = AccountId, @CardType = CardType FROM Cards WHERE CardId = @OldCardId;

    UPDATE Cards SET Status = 'Replaced', ModifiedAt = SYSUTCDATETIME() WHERE CardId = @OldCardId;

    INSERT INTO Cards (AccountId, CardNumberMasked, CardNumberHash, CardType, ExpiryDate)
    VALUES (@AccountId, @NewCardNumberMasked, @NewCardNumberHash, @CardType, @NewExpiryDate);

    SELECT SCOPE_IDENTITY() AS NewCardId;
END
GO

/* =============================================
   usp_AddMerchant
   ============================================= */
CREATE OR ALTER PROCEDURE usp_AddMerchant
    @MerchantName NVARCHAR(150),
    @MerchantCategory NVARCHAR(50),
    @Country NVARCHAR(100),
    @RiskLevel NVARCHAR(20) = 'Low'
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Merchants (MerchantName, MerchantCategory, Country, RiskLevel)
    VALUES (@MerchantName, @MerchantCategory, @Country, @RiskLevel);

    SELECT SCOPE_IDENTITY() AS NewMerchantId;
END
GO

/* =============================================
   usp_RecordTransaction
   Single entry point for all transactions from the website.
   Fraud-detection triggers on Transactions fire automatically after insert.
   ============================================= */
CREATE OR ALTER PROCEDURE usp_RecordTransaction
    @AccountId INT,
    @CardId INT = NULL,
    @MerchantId INT,
    @Amount DECIMAL(18,2),
    @Currency CHAR(3),
    @Country NVARCHAR(100),
    @IpAddress NVARCHAR(45),
    @Channel NVARCHAR(20),
    @GpsLatitude DECIMAL(9,6) = NULL,
    @GpsLongitude DECIMAL(9,6) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Transactions (AccountId, CardId, MerchantId, Amount, Currency, Country, IpAddress, Channel, GpsLatitude, GpsLongitude)
        VALUES (@AccountId, @CardId, @MerchantId, @Amount, @Currency, @Country, @IpAddress, @Channel, @GpsLatitude, @GpsLongitude);

        DECLARE @NewTransactionId BIGINT = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT @NewTransactionId AS NewTransactionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

/* =============================================
   usp_UpdateRiskScore
   Recomputes and persists a customer's risk score using fn_CustomerRiskScore.
   ============================================= */
CREATE OR ALTER PROCEDURE usp_UpdateRiskScore
    @CustomerId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Score INT = dbo.fn_CustomerRiskScore(@CustomerId);
    DECLARE @Level NVARCHAR(20) =
        CASE
            WHEN @Score >= 80 THEN 'Critical'
            WHEN @Score >= 50 THEN 'High'
            WHEN @Score >= 20 THEN 'Medium'
            ELSE 'Low'
        END;

    IF EXISTS (SELECT 1 FROM CustomerRiskScore WHERE CustomerId = @CustomerId)
        UPDATE CustomerRiskScore
        SET Score = @Score, RiskLevel = @Level, LastCalculatedAt = SYSUTCDATETIME()
        WHERE CustomerId = @CustomerId;
    ELSE
        INSERT INTO CustomerRiskScore (CustomerId, Score, RiskLevel)
        VALUES (@CustomerId, @Score, @Level);
END
GO
