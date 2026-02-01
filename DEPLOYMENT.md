# Deployment (Render)

This API is configured to deploy on Render using Docker.

## Create the service

1) Render → New → Web Service.
2) Connect repo: cambrian-api.
3) Environment: Docker.
4) Root directory: /.
5) Dockerfile path: docker/Dockerfile.
5) Health check path: /auth/health.
6) Deploy.

## Environment variables

Set in Render → Environment:

- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:3000
- CORS_ORIGINS=https://YOUR-VERCEL-APP.vercel.app
- STRIPE_SECRET_KEY=sk_live_...
- STRIPE_LISTENER_PRICE_ID=price_...
- STRIPE_CREATOR_PRICE_ID=price_...
- STRIPE_SUCCESS_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=success
- STRIPE_CANCEL_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=cancel
- STRIPE_WEBHOOK_SECRET=whsec_...

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
