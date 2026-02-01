# Deployment (Render)

> **Note**: This is the deployment guide for the Cambrian API only. For frontend deployment, see the cambrian-frontend repository. For infrastructure management, see the cambrian-infra repository.

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

## Cross-Repository Configuration

When deploying the full Cambrian ecosystem:

1. **Deploy the API first** (this repository) and note the deployed URL
2. **Configure CORS_ORIGINS** to include the frontend domain
3. **Deploy the frontend** (cambrian-frontend) with the API URL
4. **Verify integration** by testing API calls from the frontend

Each repository deploys independently, but they need to be configured to work together. See [ORGANIZATION.md](ORGANIZATION.md) for more details about the repository structure.
