# Cambrian API

> **Part of the [Cambrian Organization](ORGANIZATION.md)** - Backend services and business logic

ASP.NET Core API organized by clean architecture layers.

## What is Cambrian API?

This repository contains the backend services for the Cambrian ecosystem. It provides:
- RESTful API endpoints for frontend consumption
- Authentication and authorization services
- Business logic and data processing
- Database management and persistence
- Integration with external services (Stripe, etc.)

This is one of three repositories in the Cambrian organization:
- **cambrian-frontend** - User interface and client-side logic
- **cambrian-api** (this repo) - Backend services and API
- **cambrian-infra** - Infrastructure and deployment automation

For more information about the organization structure, see [ORGANIZATION.md](ORGANIZATION.md).

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
dotnet run --project src/auth/Cambrian.Api
```

## Database

```bash
docker compose -f docker/docker-compose.yml up -d
```

## Deployment

This API deploys independently from the frontend and infrastructure repositories. See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.

### Independent Deployment
Each component of the Cambrian ecosystem deploys separately:
- **This API**: Deployed to Render (or similar platform)
- **Frontend**: Deployed to Vercel (managed in cambrian-frontend repo)
- **Infrastructure**: Managed via IaC in cambrian-infra repo

## Troubleshooting

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md).
