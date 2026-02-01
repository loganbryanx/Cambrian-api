# Cambrian API

ASP.NET Core API organized by clean architecture layers.

## Related Repositories

- Frontend: https://github.com/Cambrian/cambrian-frontend
- API: https://github.com/Cambrian/cambrian-api
- Infrastructure: https://github.com/Cambrian/cambrian-infra

## Project Management

**GitHub Project Board:** https://github.com/users/loganbryanx/projects

To access the Cambrian project board:
1. Visit the [Projects tab](https://github.com/users/loganbryanx/projects) under the user account
2. Look for the Cambrian project board with columns: Backlog, Frontend, API, Infra, Blocked, Done
3. Issues from all three repositories (Frontend, API, Infrastructure) appear in one unified board

This allows you to manage Cambrian as one product while still deploying repositories independently.

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
