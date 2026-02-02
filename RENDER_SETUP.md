# Render Deployment Setup

## Current Status
The code has been updated to support Render deployment:
- ✅ HTTPS redirection disabled in Production (prevents redirect loops)
- ✅ Dockerfile configured to use `${PORT:-3000}` (supports both Render and AWS)
- ✅ DEPLOYMENT.md updated with Render instructions

## Required Steps in Render Dashboard

### 1. Configure Environment Variable
In your Render service dashboard, go to **Environment** and add/update:

```
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

**Important**: Make sure this variable is set before deploying. Render automatically provides the `$PORT` variable (usually 10000).

### 2. Verify Other Required Variables
Ensure these are also set in Render's Environment section:

```bash
ASPNETCORE_ENVIRONMENT=Production
CORS_ORIGINS=https://your-frontend-app.vercel.app
BILLING_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
PLAY_EVENTS_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_LISTENER_PRICE_ID=price_...
STRIPE_CREATOR_PRICE_ID=price_...
STRIPE_SUCCESS_URL=https://your-frontend-app.vercel.app/account?status=success
STRIPE_CANCEL_URL=https://your-frontend-app.vercel.app/account?status=cancel
STRIPE_WEBHOOK_SECRET=whsec_...
```

### 3. Deploy
Click **Manual Deploy** → **Deploy latest commit** in the Render dashboard.

### 4. Verify Deployment
Once deployment completes, test the health endpoint:

```powershell
Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/auth/health" | Select-Object StatusCode, Content
```

Expected response:
- **Status**: 200
- **Content**: `{"status":"ok"}`

## Troubleshooting

### If health check still fails:
1. Check Render logs for port binding messages
2. Verify `ASPNETCORE_URLS` is set correctly
3. Ensure the service is using the Docker environment (not buildpack)
4. Confirm Dockerfile path is set to `docker/Dockerfile`

### Check which port the app is listening on:
Look for this line in Render logs:
```
Now listening on: http://0.0.0.0:10000
```

The port number should match Render's assigned port.
