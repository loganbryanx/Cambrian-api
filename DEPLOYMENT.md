# Deployment (Render)

This API is configured to deploy on Render using Docker.

## Create the service

1) Render → New → Web Service.
2) Connect repo: cambrian-api.
3) Environment: Docker.
4) Root directory: /.
5) Health check path: /auth/health.
6) Deploy.

## Environment variables

Set in Render → Environment:

- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:3000

## Verify

```
curl https://YOUR-RENDER-API.onrender.com/auth/health
```

Expected:
```
{"status":"ok"}
```

## Troubleshooting

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md).
