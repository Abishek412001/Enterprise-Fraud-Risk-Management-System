# Database Architecture & Entity Relationship Schema

## Core Relational Tables
1. `Customers`, `Accounts`, `Cards`, `Merchants`, `Transactions`: Core banking transaction ledger.
2. `FRMAlerts`, `AlertAssignments`, `AlertHistory`, `AlertComments`: FRM alert module.
3. `Devices`, `CustomerSessions`, `ATOAlerts`: Account Takeover module.
4. `SentinelIncidents`, `SentinelAlerts`, `ThreatIndicators`, `SecurityEvents`: Microsoft Sentinel SIEM module.
5. `Cases`, `CaseAlerts`, `CaseTransactions`, `CaseNotes`, `SLATracking`: Enterprise Case Management.
6. `AnalystActions`, `InvestigationSessions`, `InvestigationTimeline`, `Evidence`, `DeviceTrust`: Investigation Workspace.
7. `WCAInteractions`, `PartnerCommunications`, `CommunicationTemplates`, `PartnerDirectory`: WCA Audit Log & Partner Portal.
8. `FraudMetrics`, `AnalystMetrics`, `FraudTrends`, `DailyStatistics`: Metrics & BI Analytics.
9. `Roles`, `Permissions`, `RolePermissions`, `RefreshTokens`, `UserSessions`, `PasswordHistory`: Security & RBAC.
