# Troubleshooting

Use this guide to collect the exact details needed to debug Render + Vercel deployments.

## 1) Find your Render API URL

In Render:
- Open your service → Settings → URL (or the top of the service page).
- It will look like: https://your-service.onrender.com

Quick health check:
```
curl https://YOUR-RENDER-API.onrender.com/auth/health
```
Expected response:
```
{"status":"ok"}
```

## 2) Find your Vercel Frontend URL

In Vercel:
- Open your project → Overview.
- Use the Production URL (not preview).
- It will look like: https://your-app.vercel.app

## 3) Capture browser errors

In your browser (Chrome/Edge):
- Open Developer Tools (F12).
- Console tab: copy any errors.
- Network tab: click a failed request and copy the Request URL, Status, and Response.

## 4) Check Render logs

In Render:
- Service → Logs.
- Copy the most recent error lines.

## 5) Common issues

### CORS error
Error looks like:
- "No 'Access-Control-Allow-Origin' header is present"

Fix:
- Add your Vercel URL to the API CORS allowlist.

### 404 on API route
- Ensure you’re calling https://YOUR-RENDER-API.onrender.com/auth/health (or the correct endpoint).

### 500 errors
- Check Render logs for exceptions.

## 6) Report template

Provide this when asking for help:

- Render API URL:
- Vercel URL:
- /auth/health response:
- Browser console errors:
- Network error details (URL + status):
- Render logs (last 20 lines):
