# Enterprise Fraud Risk Management System - Architecture Guide

## System Topology & Layering

The Enterprise Fraud Risk Management System (EFRS) is architected following a 3-tier clean enterprise architecture pattern:

### 1. Presentation Layer (Frontend)
- **Technology**: HTML5, Vanilla JavaScript (ES6+), Bootstrap 5, Chart.js, Bootstrap Icons.
- **Security**: JWT tokens stored in secure local storage, client-side route protection, sanitize inputs against XSS.

### 2. Application & API Layer (Backend)
- **Technology**: ASP.NET Core 8 Web API.
- **Design Patterns**: Repository Pattern, Service Layer Pattern, Dependency Injection (DI), Middleware Pipeline for Error Handling & JWT Validation.

### 3. Data Storage Layer (Database)
- **Technology**: Microsoft SQL Server 2022 / Express.
- **Components**: Relational Tables, Foreign Key Constraints, Indexes, Stored Procedures for High-Speed ADO.NET execution, Views for aggregated reporting.
