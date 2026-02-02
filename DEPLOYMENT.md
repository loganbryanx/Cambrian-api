# Deployment (AWS Fargate or Lambda)

This API is configured to deploy on AWS Fargate using Docker. You can also start with Lambda + API Gateway if you want a fully serverless option and only need HTTP endpoints.

## Option A: AWS Fargate (recommended)

1) Build and push the container image to ECR.
2) Use docker/ecs-task-def.json for the task definition.
3) Run in ECS on Fargate (no cluster management).
4) Health check path: /auth/health.
5) Deploy and scale as needed.

## Option B: Lambda + API Gateway

If you only need HTTP APIs and want scale-to-zero, wrap the ASP.NET app for Lambda and front it with API Gateway. This reduces ops overhead but can add cold-start latency.

## Environment variables

Set in your task definition or Lambda environment:

- ASPNETCORE_ENVIRONMENT=Production
- ASPNETCORE_URLS=http://+:3000
- CORS_ORIGINS=https://YOUR-VERCEL-APP.vercel.app
- BILLING_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...;
- PLAY_EVENTS_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...;
- STRIPE_SECRET_KEY=sk_live_...
- STRIPE_LISTENER_PRICE_ID=price_...
- STRIPE_CREATOR_PRICE_ID=price_...
- STRIPE_SUCCESS_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=success
- STRIPE_CANCEL_URL=https://YOUR-VERCEL-APP.vercel.app/account?status=cancel
- STRIPE_WEBHOOK_SECRET=whsec_...

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
