# Frontend (cambrian-app) Vercel Deployment Guide

This guide explains how to deploy the **cambrian-app** frontend (Vite) to Vercel, which will connect to this Cambrian API backend.

## Architecture Overview

- **Frontend**: cambrian-app (Vite/React) → Deploy to Vercel
- **Backend**: Cambrian-api (.NET) → Deploy to Azure/AWS/Railway (see VERCEL_DEPLOYMENT.md)

## Prerequisites

- GitHub repository: `cambrian-app` containing the Vite frontend application
- Vercel account (free tier works great for frontend apps)
- This API backend deployed and accessible

---

## Step 1: Prepare the Frontend Repository

Ensure your `cambrian-app` repository has the following structure:

```
cambrian-app/
├── package.json
├── vite.config.js (or .ts)
├── index.html
├── src/
│   └── ... (your React/Vue/etc. code)
└── dist/ (generated after build)
```

### Required Scripts in package.json

```json
{
  "scripts": {
    "dev": "vite",
    "build": "vite build",
    "preview": "vite preview"
  }
}
```

---

## Step 2: Create Vercel Project

### Via Vercel Dashboard

1. Go to [Vercel](https://vercel.com)
2. Click **"New Project"**
3. Click **"Import Git Repository"**
4. Select or search for: `cambrian-app`
5. Click **"Import"**

### Configure Build Settings

Vercel will auto-detect Vite, but verify these settings:

- **Framework Preset**: `Vite`
- **Root Directory**: `.` (leave as root unless using monorepo)
- **Build Command**: `npm run build`
- **Output Directory**: `dist`
- **Install Command**: `npm install` (auto-detected)
- **Development Command**: `npm run dev` (optional)

---

## Step 3: Environment Variables

Add environment variables to connect the frontend to your backend API:

### In Vercel Dashboard

Go to **Project Settings** → **Environment Variables** and add:

#### Production Environment
```
VITE_API_URL=https://your-cambrian-api.azurewebsites.net
VITE_API_TIMEOUT=30000
```

#### Preview Environment (Optional)
```
VITE_API_URL=https://staging-cambrian-api.azurewebsites.net
```

#### Development Environment (Local)
```
VITE_API_URL=http://localhost:3000
```

### Using Environment Variables in Code

In your Vite app, access these variables:

```javascript
// src/config/api.js
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

export const apiClient = axios.create({
  baseURL: API_URL,
  timeout: import.meta.env.VITE_API_TIMEOUT || 30000,
  headers: {
    'Content-Type': 'application/json',
  }
});
```

---

## Step 4: Configure SPA Routing (Important!)

For client-side routing (React Router, Vue Router, etc.), create a `vercel.json` in the **cambrian-app** repository:

```json
{
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

This ensures all routes are handled by your SPA and not treated as 404s.

---

## Step 5: Update Backend CORS Configuration

**CRITICAL**: Update the Cambrian API to allow requests from your Vercel frontend domain.

### In `src/Cambrian.Api/Program.cs`

Update the CORS policy to include your Vercel domain:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",     // Local dev
            "http://localhost:5174",     // Local dev alt
            "https://cambrian-app.vercel.app",           // Vercel production
            "https://cambrian-app-*.vercel.app"          // Vercel preview deployments
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();  // If using cookies/auth
    });
});
```

**Note**: Replace `cambrian-app` with your actual Vercel project name.

For production, consider using environment variables:

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

---

## Step 6: Deploy

1. Click **"Deploy"** in Vercel
2. Vercel will:
   - Clone your repository
   - Run `npm install`
   - Run `npm run build`
   - Deploy the `dist` folder
   - Provide a URL like `https://cambrian-app.vercel.app`

### Monitor Deployment

- Watch the build logs in real-time
- Vercel shows each step: Install → Build → Deploy
- First deployment takes 1-3 minutes

---

## Step 7: Post-Deployment Verification

### Test the Deployment

1. **Visit the site**: `https://cambrian-app.vercel.app`
2. **Test API connectivity**: Open browser DevTools → Network tab
3. **Verify API calls**: Ensure requests to backend succeed
4. **Check routing**: Navigate between pages, refresh on a nested route

### Common Issues

#### 1. API Connection Fails (CORS Error)

**Error**: `Access to fetch at 'https://api.example.com' from origin 'https://cambrian-app.vercel.app' has been blocked by CORS policy`

**Solution**: Update backend CORS configuration (see Step 5)

#### 2. 404 on Page Refresh

**Error**: Page works on direct navigation but 404 on refresh

**Solution**: Add `vercel.json` with rewrites (see Step 4)

#### 3. Environment Variables Not Working

**Error**: API calls going to undefined or localhost

**Solution**: 
- Verify env vars are set in Vercel dashboard
- Ensure they're prefixed with `VITE_`
- Redeploy after adding env vars (they're only applied on build)

#### 4. Build Fails

**Error**: "Command failed: npm run build"

**Solution**:
- Check build logs for specific error
- Verify `package.json` has `build` script
- Test build locally: `npm run build`
- Check Node.js version compatibility

---

## Step 8: Custom Domain (Optional)

### Add Custom Domain

1. Go to **Project Settings** → **Domains**
2. Click **"Add Domain"**
3. Enter your domain: `app.cambrian.com`
4. Follow DNS configuration instructions
5. Vercel will auto-provision SSL certificate

### Update CORS

Don't forget to add your custom domain to the backend CORS configuration!

---

## Automatic Deployments

Vercel automatically deploys:

- **Production**: On every push to `main` branch
- **Preview**: On every pull request
- **Commits**: Get unique preview URLs

### Configure Deployment Branches

In **Project Settings** → **Git**:
- Production Branch: `main` (or `master`)
- Enable/disable preview deployments for PRs

---

## Sample vercel.json for cambrian-app

Create this file in the root of your `cambrian-app` repository:

```json
{
  "$schema": "https://openapi.vercel.sh/vercel.json",
  "version": 2,
  "buildCommand": "npm run build",
  "outputDirectory": "dist",
  "framework": "vite",
  "rewrites": [
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ],
  "headers": [
    {
      "source": "/assets/(.*)",
      "headers": [
        {
          "key": "Cache-Control",
          "value": "public, max-age=31536000, immutable"
        }
      ]
    }
  ],
  "env": {
    "VITE_API_URL": "@api_url"
  }
}
```

---

## Performance Optimization

### 1. Enable Asset Caching

Static assets (JS, CSS, images) are automatically cached by Vercel CDN.

### 2. Code Splitting

Vite automatically splits your code. Verify in build output:

```bash
npm run build

# Should see output like:
# dist/assets/index-abc123.js   45.2 kB
# dist/assets/vendor-def456.js  123.4 kB
```

### 3. Image Optimization

Use Vercel's built-in image optimization:

```javascript
// Instead of:
<img src="/image.jpg" />

// Use Next.js Image (if using Next.js) or:
<img src="/_vercel/image?url=/image.jpg&w=800&q=80" />
```

### 4. Enable Gzip/Brotli

Vercel automatically enables compression. Verify with:

```bash
curl -H "Accept-Encoding: gzip" https://cambrian-app.vercel.app -I
# Should see: Content-Encoding: gzip or br
```

---

## Security Best Practices

### 1. Don't Commit Secrets

Never commit API keys or secrets. Use environment variables.

### 2. Use HTTPS Only

Vercel provides free SSL. Ensure your API also uses HTTPS.

### 3. Implement CSP Headers

Add Content Security Policy in `vercel.json`:

```json
{
  "headers": [
    {
      "source": "/(.*)",
      "headers": [
        {
          "key": "Content-Security-Policy",
          "value": "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline';"
        }
      ]
    }
  ]
}
```

---

## Monitoring & Analytics

### Vercel Analytics

Enable in **Project Settings** → **Analytics**:
- Real User Monitoring (RUM)
- Web Vitals tracking
- Performance insights

### Integration with Backend

Send frontend errors to backend:

```javascript
window.addEventListener('error', (event) => {
  fetch(`${API_URL}/api/logs/frontend-error`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      message: event.message,
      stack: event.error?.stack,
      url: window.location.href
    })
  });
});
```

---

## Development Workflow

### Local Development

```bash
# In cambrian-app directory
npm install
npm run dev

# Runs on http://localhost:5173
# Connects to local API at http://localhost:3000
```

### Preview Deployments

Every PR gets a unique preview URL:
```
https://cambrian-app-git-feature-branch-username.vercel.app
```

Share with team for testing before merging.

---

## Rollback & Versioning

### Rollback to Previous Deployment

1. Go to **Deployments** tab
2. Find the deployment you want to rollback to
3. Click **"..."** → **"Promote to Production"**

### Deployment History

Vercel keeps all deployments. You can:
- View any previous deployment
- Compare performance metrics
- Inspect build logs

---

## Cost Considerations

### Free Tier (Hobby)

Perfect for personal projects:
- Unlimited deployments
- 100 GB bandwidth/month
- Serverless function execution: 100 GB-hours/month

### Pro Tier

For production apps:
- $20/month per user
- 1 TB bandwidth/month
- Priority support
- Team collaboration features

---

## Troubleshooting Checklist

- [ ] Verify `package.json` has correct scripts
- [ ] Ensure `vite.config.js` is properly configured
- [ ] Check environment variables are set and prefixed with `VITE_`
- [ ] Confirm backend CORS includes Vercel domain
- [ ] Add `vercel.json` for SPA routing
- [ ] Test build locally: `npm run build && npm run preview`
- [ ] Check Vercel deployment logs for errors
- [ ] Verify API endpoints are accessible from browser
- [ ] Test on different browsers/devices

---

## Additional Resources

- [Vercel Documentation](https://vercel.com/docs)
- [Vite Documentation](https://vitejs.dev/)
- [Vercel CLI](https://vercel.com/docs/cli) - Deploy from terminal
- [Vercel GitHub Integration](https://vercel.com/docs/git)

---

## Quick Reference Commands

```bash
# Install Vercel CLI (optional)
npm install -g vercel

# Deploy from terminal
cd cambrian-app
vercel

# Deploy to production
vercel --prod

# View deployment logs
vercel logs

# List deployments
vercel ls
```

---

## Summary

Your architecture after deployment:

```
┌─────────────────┐         ┌──────────────────┐
│  Vercel CDN     │         │  Azure/AWS/etc   │
│  (Frontend)     │ ──────► │  (Backend API)   │
│  cambrian-app   │  HTTPS  │  Cambrian-api    │
└─────────────────┘         └──────────────────┘
      ↓ Serves                    ↓ Provides
   Static files               RESTful APIs
   (HTML/CSS/JS)              (JSON responses)
```

**Frontend URL**: `https://cambrian-app.vercel.app`  
**Backend URL**: `https://your-api-domain.com`

Good luck with your deployment! 🚀
