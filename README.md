# Enterprise Fraud Risk Management System (Lite)

A runnable MS SQL Server 2022 project simulating a bank's fraud-detection
core: customers, accounts, cards, transactions, and a set of fraud rules
that fire in real time as transactions are recorded. Scoped down from a
full "50k-row enterprise repo" to something you can actually run, read,
and talk through end-to-end tonight.

## Folder structure

```
Enterprise-Fraud-Risk-Lite/
├── README.md
├── database/
│   ├── schema.sql       -- tables, constraints, indexes
│   ├── seed_data.sql     -- 50 customers, ~75 accounts/cards, ~3,000 transactions, logins
│   ├── functions.sql     -- fnAverageSpend, fnVelocityCount, fnFailedLoginCount, fnRecentTransactions
│   ├── procedures.sql    -- usp_CreateCustomer, usp_RecordTransaction (fraud rules live here),
│   │                        usp_CalculateRiskScore, usp_BlockCard, usp_ResolveFraudCase, usp_DailyFraudSummary
│   ├── triggers.sql       -- audit logging + auto-block on repeated High alerts
│   └── views.sql          -- vwHighRiskCustomers, vwDailyFraudSummary
└── reports/
    └── analytical_queries.sql  -- recursive CTE gap-fill, PIVOT, UNPIVOT,
                                    ROW_NUMBER/NTILE/LEAD/LAG, DENSE_RANK, EXISTS
```

## How to run

In VS Code with the SQL Server (mssql) extension, or via `sqlcmd`, connect
to a local SQL Server 2022 instance and run the files **in this order**:

```
1. database/schema.sql
2. database/seed_data.sql
3. database/functions.sql
4. database/procedures.sql
5. database/triggers.sql
6. database/views.sql
7. reports/analytical_queries.sql
```

Every script is idempotent (drops/recreates or clears-and-reseeds), so
you can rerun the whole sequence any time without manual cleanup.

```powershell
sqlcmd -S localhost -i database\schema.sql
sqlcmd -S localhost -i database\seed_data.sql
sqlcmd -S localhost -i database\functions.sql
sqlcmd -S localhost -i database\procedures.sql
sqlcmd -S localhost -i database\triggers.sql
sqlcmd -S localhost -i database\views.sql
sqlcmd -S localhost -i reports\analytical_queries.sql
```

## What's deliberately seeded to demo the fraud rules

- **AccountID 5**: 8 transactions inside a 7-minute window right now →
  trips the velocity rule the moment you call `usp_RecordTransaction` again.
- **CustomerID 7**: 6 failed logins in the last 6 minutes → `fnFailedLoginCount(7, 10)` returns 6.
- **CustomerID 12 and 31**: seeded into `Blacklist`.
- Roughly 0.5% of the 3,000 baseline transactions are >$6,000 (high-value rule),
  and ~0.7% are flagged with country `RU` against a `US` home country (foreign-country rule).

## Quick smoke test after running everything

```sql
EXEC dbo.usp_RecordTransaction @AccountID = 5, @MerchantID = 1, @Amount = 45.00, @TransactionCountry = 'US', @Channel = 'Online';
-- expect FinalStatus = 'Flagged' and a new VelocityFraud row in FraudAlerts

EXEC dbo.usp_CalculateRiskScore @CustomerID = 12;
-- expect NewScore >= 40 (blacklisted)

SELECT TOP 10 * FROM dbo.vwHighRiskCustomers ORDER BY RiskRank;
SELECT TOP 10 * FROM dbo.vwDailyFraudSummary ORDER BY AlertDate DESC;
EXEC dbo.usp_DailyFraudSummary;
```

## SQL concepts you can point to in the interview

- **Schema design**: 3NF normalization, PK/FK, composite uniqueness (`AccountNumber`, `CardNumberMasked`), CHECK/DEFAULT constraints, computed status flows.
- **Indexing**: covering index with `INCLUDE`, and a **filtered index**
  (`IX_FraudAlerts_HighOpen`) — good talking point on selective indexing for hot-path queries.
- **T-SQL**: `MERGE` (upsert in `usp_CalculateRiskScore`), window function `RANK()` (in `vwHighRiskCustomers`), scalar + inline table-valued functions, `TRY...CATCH` with `XACT_ABORT`/`ROLLBACK` on every write path, set-based `AFTER INSERT` triggers (not row-by-row cursors).
- **Fraud logic**: velocity fraud (transactions/time window), high-value thresholding, blacklist screening, cross-border anomaly detection, and an auto-response trigger (auto-block cards after repeated High alerts) — this last one is a good example of *detection feeding directly into control action*, which is the kind of design conversation a Fraud Risk Analyst interview tends to probe.
- **Talking point on scope**: seed data uses `CHECKSUM(NEWID())` for pseudo-random but reproducible-in-shape volume generation rather than static INSERT lists — worth mentioning if asked how you'd generate test data at scale.

Add to the talking points above: `reports/analytical_queries.sql` has a
**recursive CTE** (gap-filling a 30-day calendar spine so days with zero
fraud alerts still show up as 0 instead of vanishing — a real pattern for
trend dashboards), a **PIVOT** (transaction counts by channel across the
busiest merchants) and its inverse **UNPIVOT** (severity columns back into
rows), plus `NTILE`/`LEAD`/`LAG`/`DENSE_RANK` dashboard queries.

## Honest scope note

This is intentionally the "highest-value slice," not the full 40-file/50k-row
spec — it still skips the docs/ folder (ERD.md, BusinessRequirements.md,
etc.) and a few of the less interview-relevant objects, so it stays
something you can actually read and run tonight. If you want the docs
folder or anything else layered on top, just ask.
