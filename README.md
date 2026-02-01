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

See [DEPLOYMENT.md](DEPLOYMENT.md) for information on deploying to Render and configuring CORS for production.

## Verification

After deployment, see [VERIFICATION.md](VERIFICATION.md) for step-by-step verification:
- Confirm Render API health check works
- Verify Vercel environment variables
- Test CORS configuration
- Test frontend flow: Signup → Discover → Save → Library

Quick verification script available: `./scripts/verify-deployment.sh`

## Troubleshooting

If you encounter deployment or integration issues, see [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for help with:
- Gathering deployment URLs
- Capturing console errors
- Common issues and solutions
