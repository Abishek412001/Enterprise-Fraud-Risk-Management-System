# REST API Documentation & Endpoint Reference

## Authentication
- `POST /api/auth/login`: Authenticate analyst/admin and issue JWT Bearer Token.
- `POST /api/auth/register`: Provision new user account.

## FRM Alert Management
- `GET /api/frmalerts`: Paged list of FRM alerts.
- `GET /api/frmalerts/{id}`: Detailed FRM alert view.
- `POST /api/frmalerts/{id}/assign`: Assign alert to analyst.
- `POST /api/frmalerts/{id}/close`: Resolve/close alert.

## Account Takeover (ATO) Monitoring
- `GET /api/atoalerts`: List of ATO alerts.
- `POST /api/sessions/record-login`: Telemetry capture for device logins.

## Microsoft Sentinel SIEM
- `GET /api/sentinelalerts`: List of SIEM security alerts.
- `GET /api/incidents`: List of aggregated security incidents.

## Case Management & SLA
- `GET /api/cases`: Case queue with SLA status.
- `POST /api/cases`: Create case from alert.

## Investigation Workspace & Actions
- `POST /api/account/freeze`: Freeze customer account.
- `POST /api/account/unfreeze`: Unfreeze customer account.
- `POST /api/card/suspend`: Suspend card.
- `POST /api/device/block`: Block hardware device fingerprint.

## Metrics & Export
- `GET /api/executive`: Executive C-Suite telemetry.
- `GET /api/export/csv`: Export report CSV.
- `GET /api/export/pdf`: Export report PDF.
