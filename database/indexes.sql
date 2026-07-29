USE EnterpriseFraudRiskDB;
GO

CREATE INDEX IX_Customers_LastName ON Customers(LastName);
CREATE INDEX IX_Customers_Country ON Customers(Country);
CREATE INDEX IX_Accounts_CustomerId ON Accounts(CustomerId);
CREATE INDEX IX_Cards_AccountId ON Cards(AccountId);
CREATE INDEX IX_Transactions_AccountId_TransactionAt ON Transactions(AccountId, TransactionAt DESC);
CREATE INDEX IX_Transactions_CardId ON Transactions(CardId);
CREATE INDEX IX_Transactions_MerchantId ON Transactions(MerchantId);
CREATE INDEX IX_Transactions_Country ON Transactions(Country);
CREATE INDEX IX_FraudAlerts_CustomerId ON FraudAlerts(CustomerId);
CREATE INDEX IX_FraudAlerts_Status ON FraudAlerts(Status);
CREATE INDEX IX_FraudAlerts_CreatedAt ON FraudAlerts(CreatedAt DESC);
CREATE INDEX IX_LoginHistory_UserId_AttemptedAt ON LoginHistory(UserId, AttemptedAt DESC);
CREATE INDEX IX_AuditLog_EntityName_EntityId ON AuditLog(EntityName, EntityId);
GO
