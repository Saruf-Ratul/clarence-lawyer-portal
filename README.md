# Clarence Lawyer Portal

Attorney-first landlord-tenant case management platform integrated with Rent Manager.

## What is implemented now (module-by-module scaffold)
- Backend API modules:
  - Dashboard KPIs
  - Clients
  - Properties (by client)
  - Cases (list/detail/status update)
  - Rent Manager Intake Inbox (list + accept case)
- Frontend modules/pages:
  - Dashboard
  - RM Intake Inbox
  - Clients
  - Properties
  - Cases
  - Case Detail
  - Forms & Filings (scaffold)
  - Reports (scaffold)
  - Settings (scaffold)

## Project structure
- `backend/Clarence.Api` - API host and route mappings
- `backend/Clarence.Application` - DTOs and service contracts
- `backend/Clarence.Domain` - core entities and enums
- `backend/Clarence.Infrastructure` - in-memory store + services
- `frontend/clarence-web` - React TypeScript single-page app
- `docs/BRS.md` - business requirements
- `docs/SRS.md` - system requirements starter

## Run
### Backend
```bash
cd backend/Clarence.Api
dotnet restore
dotnet run
```

### Frontend
```bash
cd frontend/clarence-web
npm install
npm run dev
```

> Note: If you run frontend separately, set up a dev proxy or serve through the same host so `/api/*` resolves to backend.


## Rent Manager data pull and NJ form mapping
- Endpoint `POST /api/rm-sync/pull` triggers RM import service (currently mock import; real RM API credentials and endpoint config required).
- Imported RM submissions appear in `GET /api/rm-inbox`.
- Accepting submissions creates legal cases via `POST /api/rm-inbox/accept`.
- Generated NJ form data is available at `GET /api/cases/{caseId}/forms/nj-lt`.

## Full SQL database
- Full SQL Server schema is included at `database/clarence_full_schema.sql`.
