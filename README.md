# 🛡️ Enterprise Fraud Risk Management System

A full-stack fraud detection system: ASP.NET Core 8 + Entity Framework Core + Microsoft SQL Server backend, Bootstrap 5 frontend, with fraud detection implemented in SQL Server triggers.

## What's implemented right now (real, runnable code)

**Database (`/database`) — complete**
- `schema.sql` — all 10 tables (Users, Customers, Accounts, Cards, Merchants, Transactions, FraudAlerts, CustomerRiskScore, LoginHistory, AuditLog) with PKs, FKs, CHECK constraints, UNIQUE constraints
- `indexes.sql` — performance indexes on hot columns
- `functions.sql` — `fn_CustomerRiskScore`, `fn_TotalTransactions`, `fn_FailedLoginCount`, `fn_HighValueTransactionCount`
- `procedures.sql` — all 10 stored procedures listed in the spec, including `usp_RecordTransaction`
- `triggers.sql` — `trg_Transactions_FraudDetection` (velocity, high-value, duplicate, foreign, blacklisted-customer, blocked-card rules, all firing automatically on insert), plus audit triggers on Customers/Transactions/FraudAlerts
- `views.sql`, `analytical_queries.sql` — dashboard and reporting queries

**Backend (`/backend`) — Auth + Customers modules complete end-to-end**
- `Program.cs` — DI, JWT auth, Swagger, CORS, global exception middleware, Serilog logging, all wired up
- `Data/ApplicationDbContext.cs` — EF Core mapping for all 10 entities
- `Models/` — all 10 entity classes
- Full vertical slice for **Auth** (register/login, BCrypt hashing, JWT issuance) and **Customers** (search + pagination, CRUD, role-based authorization) across Controller → Service → Repository → stored procedure/EF Core

**Frontend (`/frontend`) — Auth pages + dashboard shell complete**
- `login.html` / `register.html` — working forms wired to the Auth API, validation, loading states
- `dashboard.html` — Bootstrap sidebar/navbar, dark/light theme toggle, 4 stat cards, 4 Chart.js charts, recent-transactions and recent-alerts tables
- `css/style.css`, `css/dashboard.css`, `js/login.js`, `js/register.js`, `js/dashboard.js`, `js/charts.js`

## What's scaffolded but not yet built out

The spec asks for 7 more full CRUD modules (Accounts, Cards, Merchants, Transactions, FraudAlerts, Reports with CSV/Excel/PDF export) and 6 more frontend pages (customers.html, accounts.html, cards.html, merchants.html, transactions.html, fraudalerts.html, reports.html). These weren't built in this pass — each one follows the exact same pattern as Customers (Controller → Service → Repository → stored procedure), so the fastest path is to say which module you want next and I'll build that vertical slice completely, the same way Customers was done, rather than generating seven half-finished modules at once.

## Setup

### 1. SQL Server
Run the scripts in SQL Server Management Studio **in this order**:
```
schema.sql → indexes.sql → functions.sql → procedures.sql → triggers.sql → views.sql
```

### 2. Backend
```bash
cd backend
dotnet restore
dotnet user-secrets set "Jwt:Key" "<generate-a-long-random-secret>"
dotnet run
```
Update `appsettings.json`'s `ConnectionStrings:DefaultConnection` for your SQL Server instance. Swagger UI is available at `/swagger` in development.

### 3. Frontend
Open `frontend/login.html` directly, or serve the folder with any static file server. Set `window.EFRS_API_BASE_URL` (in a `<script>` tag before `login.js`/`dashboard.js`) if your API isn't at `https://localhost:5001/api`.

## Security notes
- Passwords are hashed with BCrypt (work factor 12), never stored or logged in plaintext.
- Card numbers are stored masked (`CardNumberMasked`) plus a hash (`CardNumberHash`) — never the raw PAN.
- All writes go through parameterized stored procedures or EF Core — no string-concatenated SQL.
- JWT bearer auth with role-based authorization (`Admin`, `FraudAnalyst`) on every write endpoint.

## Author
Abishek W
