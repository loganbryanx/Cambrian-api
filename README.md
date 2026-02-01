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
dotnet run --project src/auth/Cambrian.Api
```

## Database

```bash
docker compose -f docker/docker-compose.yml up -d
```

## Deployment

See [DEPLOYMENT.md](DEPLOYMENT.md).

## Troubleshooting

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md).
