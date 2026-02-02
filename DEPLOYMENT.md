# Deployment (AWS Fargate, Lambda, or Render)

This API is configured to deploy on AWS Fargate using Docker. You can also start with Lambda + API Gateway if you want a fully serverless option and only need HTTP endpoints. For simpler deployments, Render.com is also supported.

## Option A: AWS Fargate (recommended for production)

1) Build and push the container image to ECR.
2) Use docker/ecs-task-def.json for the task definition.
3) Run in ECS on Fargate (no cluster management).
4) Health check path: /auth/health.
5) Deploy and scale as needed.

## Option B: Lambda + API Gateway

If you only need HTTP APIs and want scale-to-zero, wrap the ASP.NET app for Lambda and front it with API Gateway. This reduces ops overhead but can add cold-start latency.

## Option C: Render.com (easiest for testing/staging)

Render requires services to bind to the port provided via the `PORT` environment variable.

1) Connect your GitHub repository to Render
2) Select "Web Service" and choose "Docker" as the environment
3) Set the Dockerfile path to `docker/Dockerfile`
4) In Environment settings, add:
   ```
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://0.0.0.0:$PORT
   ```
   (Note: Render automatically provides `$PORT`, typically 10000)
5) Add all other required environment variables (see below)
6) Deploy and verify the /auth/health endpoint

**Important**: The Dockerfile has been configured to use `${PORT:-3000}`, which uses Render's PORT variable when available and falls back to 3000 for local/AWS deployments. HTTPS redirection is disabled in non-development environments to prevent connection issues in containerized deployments without TLS certificates.

## Environment variables

Set in your task definition, Lambda environment, or Render dashboard:

- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:3000 (for AWS) or http://0.0.0.0:$PORT (for Render)
- CORS_ORIGINS=https://YOUR-VERCEL-APP.vercel.app
- BILLING_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...;
- PLAY_EVENTS_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...;
- STRIPE_SECRET_KEY=sk_live_...
- STRIPE_LISTENER_PRICE_ID=price_...
- STRIPE_CREATOR_PRICE_ID=price_...
- STRIPE_SUCCESS_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=success
- STRIPE_CANCEL_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=cancel
- STRIPE_WEBHOOK_SECRET=whsec_...

**Note for Render**: Render automatically provides the `PORT` variable. The Dockerfile is configured to use it automatically.

## Verify

```
curl https://YOUR-API-URL/auth/health
```

Expected:
```
{"status":"ok"}
```

## Troubleshooting

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md).
