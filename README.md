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

### Backend API (This Repository)
This .NET API should be deployed to a platform that supports ASP.NET Core:
- **Azure App Service** - Native .NET support (Recommended)
- **AWS Elastic Beanstalk** - Container or native support
- **Railway** - Docker support with generous free tier
- **Render** - Docker support
- **Fly.io** - Docker support

Use the included `Dockerfile` for containerized deployments.

### Frontend App (cambrian-app repository)
The Vite frontend should be deployed to **Vercel**:
- See [FRONTEND_VERCEL_DEPLOYMENT.md](FRONTEND_VERCEL_DEPLOYMENT.md) - Complete guide for deploying the frontend
- See [CORS_CONFIGURATION.md](CORS_CONFIGURATION.md) - Configure this API to accept requests from Vercel
- See [frontend-vercel.json](frontend-vercel.json) - Sample configuration file for the frontend repo

**Architecture**: Frontend on Vercel → Backend on Azure/AWS/Railway
