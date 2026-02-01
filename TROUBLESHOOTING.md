# Troubleshooting Guide

This guide helps you gather and report information needed to troubleshoot deployment and integration issues.

## Required Information for Support

When reporting issues with the Cambrian API deployment, please provide the following information:

### 1. Render API URL

Your Render API URL is the public URL where your API is deployed.

**How to find it:**
1. Log in to [Render Dashboard](https://dashboard.render.com/)
2. Click on your `cambrian-api` service
3. Look for the URL at the top of the page (e.g., `https://cambrian-api-xxxxx.onrender.com`)
4. Copy the complete URL including `https://`

**Test your Render API URL:**
```bash
curl https://YOUR-RENDER-API.onrender.com/auth/health
```

Expected response:
```json
{"status":"ok"}
```

If you get a different response or an error, include the exact output in your report.

### 2. Vercel Frontend URL

Your Vercel URL is where your frontend application is deployed.

**How to find it:**
1. Log in to [Vercel Dashboard](https://vercel.com/dashboard)
2. Click on your project
3. Look for the "Domains" section
4. Copy the primary domain (e.g., `https://your-app.vercel.app`)

**Note:** You may have multiple URLs:
- Production URL: `https://your-app.vercel.app`
- Preview URLs: `https://your-app-git-branch-xxx.vercel.app`

Report the URL where you're experiencing the issue.

### 3. Console Errors

Console errors help identify what's failing in your application.

#### Browser Console (Frontend Errors)

**How to capture:**
1. Open your Vercel frontend URL in a browser
2. Open Developer Tools:
   - Chrome/Edge: Press `F12` or `Ctrl+Shift+I` (Windows/Linux) / `Cmd+Option+I` (Mac)
   - Firefox: Press `F12` or `Ctrl+Shift+K` (Windows/Linux) / `Cmd+Option+K` (Mac)
   - Safari: Enable Developer menu first, then `Cmd+Option+C`
3. Click on the "Console" tab
4. Reproduce the issue (e.g., try signup → Discover → Save → Library)
5. Look for red error messages
6. Copy the complete error messages

**Common frontend errors:**

**CORS Error:**
```
Access to fetch at 'https://your-api.onrender.com/auth/login' from origin 
'https://your-app.vercel.app' has been blocked by CORS policy: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

**Network Error:**
```
POST https://your-api.onrender.com/auth/login net::ERR_NAME_NOT_RESOLVED
```

**4xx/5xx Error:**
```
POST https://your-api.onrender.com/auth/login 404 (Not Found)
POST https://your-api.onrender.com/auth/login 500 (Internal Server Error)
```

#### Network Tab (API Request/Response Details)

**How to capture:**
1. Open Developer Tools (same as above)
2. Click on the "Network" tab
3. Reproduce the issue
4. Look for red/failed requests
5. Click on the failed request
6. Copy the information from:
   - Headers tab (Request URL, Request Method, Status Code)
   - Response tab (Error message)
   - Preview tab (Formatted response)

#### Server Logs (Render API Errors)

**How to access:**
1. Log in to Render Dashboard
2. Click on your `cambrian-api` service
3. Click on "Logs" in the left sidebar
4. Look for error messages (usually in red)
5. Copy relevant error messages with timestamps

## Common Issues and Solutions

### CORS Errors

**Symptom:** Browser console shows CORS policy error

**Solution:**
1. Verify your Vercel URL is added to `Cors__AllowedOrigins` in Render
2. Go to Render Dashboard → Your Service → Environment
3. Check the `Cors__AllowedOrigins` variable
4. Ensure it includes your exact Vercel URL with protocol:
   ```
   Cors__AllowedOrigins=https://your-app.vercel.app
   ```
5. If you made changes, redeploy the service

### Health Check Fails

**Symptom:** `curl https://YOUR-RENDER-API.onrender.com/auth/health` returns error

**Possible causes:**
- Service is still deploying (check Render Dashboard)
- Service crashed (check Render Logs)
- Incorrect URL (verify in Render Dashboard)

### 404 Errors

**Symptom:** API requests return 404 Not Found

**Possible causes:**
- Incorrect API endpoint path
- API not fully deployed
- Frontend pointing to wrong API URL

### Network Timeout

**Symptom:** Requests hang and eventually timeout

**Possible causes:**
- Render service is sleeping (free tier spins down after inactivity)
- First request after sleep takes 30-60 seconds
- Try the request again after waiting

## Reporting Template

When requesting help, please provide the following information:

```
**Render API URL:**
https://your-api.onrender.com

**Health Check Status:**
[Paste the output of: curl https://your-api.onrender.com/auth/health]

**Vercel Frontend URL:**
https://your-app.vercel.app

**CORS Configuration (from Render Dashboard):**
Cors__AllowedOrigins=https://your-app.vercel.app

**Console Errors:**
[Paste browser console errors here]

**Network Tab Details (if applicable):**
Request URL: 
Status Code: 
Response Body: 

**Server Logs (if applicable):**
[Paste relevant Render logs here]

**Steps to Reproduce:**
1. 
2. 
3. 

**Expected Behavior:**
[What should happen]

**Actual Behavior:**
[What actually happens]
```

## Testing Your Deployment

After deployment, test the following:

### 1. Health Check
```bash
curl https://YOUR-RENDER-API.onrender.com/auth/health
```
Expected: `{"status":"ok"}`

### 2. CORS Preflight
```bash
curl -i -H "Origin: https://your-app.vercel.app" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS \
     https://YOUR-RENDER-API.onrender.com/auth/login
```
Expected: Response includes `Access-Control-Allow-Origin: https://your-app.vercel.app`

### 3. Actual API Request
```bash
curl -X POST https://YOUR-RENDER-API.onrender.com/auth/login \
     -H "Content-Type: application/json" \
     -H "Origin: https://your-app.vercel.app" \
     -d '{"email":"test@example.com","password":"test123"}'
```
Expected: Response includes CORS headers and either success or authentication error (not CORS error)

### 4. Frontend Integration
1. Open your Vercel URL in a browser
2. Open Developer Tools → Console
3. Try: Signup → Discover → Save → Library
4. Verify no CORS errors appear in console
5. Verify API requests succeed (check Network tab)

## Getting Help

If you've tried the solutions above and still have issues:

1. Create a GitHub issue using the deployment issue template
2. Include all information from the Reporting Template above
3. Attach screenshots of console errors if helpful
4. Be as specific as possible about when the issue occurs

See [DEPLOYMENT.md](DEPLOYMENT.md) for deployment instructions.
