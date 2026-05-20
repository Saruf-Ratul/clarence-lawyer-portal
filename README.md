# Clarence Lawyer Portal

Monorepo scaffold for a Rent Manager-integrated landlord-tenant case management system.

## Structure
- `backend/Clarence.Api` - ASP.NET Core Web API host
- `backend/Clarence.Application` - Application layer (use-cases/contracts)
- `backend/Clarence.Domain` - Domain entities and enums
- `backend/Clarence.Infrastructure` - Persistence/integrations
- `frontend/clarence-web` - React + TypeScript frontend
- `docs` - Product docs and requirements

## Quick start
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
