# Cambrian API

ASP.NET Core API organized by clean architecture layers.

## Structure

- src/
  - Cambrian.Api/ (ASP.NET Core Web API)
  - Cambrian.Application/ (use-cases / services)
  - Cambrian.Domain/ (entities + domain rules)
  - Cambrian.Infrastructure/ (EF Core, DB, external integrations)
- tests/
  - Cambrian.Api.Tests/
  - Cambrian.Application.Tests/
- docker-compose.yml (database container)

## Quick start

```bash
dotnet restore
```

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project src/Cambrian.Api
```

## Database

```bash
docker compose up -d
```

## Deployment

### Backend API Deployment
- See [VERCEL_DEPLOYMENT.md](VERCEL_DEPLOYMENT.md) for deploying this API (recommended platforms: Azure, AWS, Railway)

### Frontend Deployment to Vercel
- See [FRONTEND_VERCEL_DEPLOYMENT.md](FRONTEND_VERCEL_DEPLOYMENT.md) for deploying the Vite frontend (cambrian-app)
- See [CORS_CONFIGURATION.md](CORS_CONFIGURATION.md) for connecting frontend to this API
