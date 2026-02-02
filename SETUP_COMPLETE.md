# Setup Complete - Next Steps

## ✅ Completed

### Frontend Configuration
- **Location:** `C:\Users\logan\Dev\Cambrian\cambrian-app`
- **Files Updated:**
  - `.env` → Production API (https://cambrian-api.onrender.com)
  - `.env.local` → Local development (http://localhost:3000)
  - `.env.production` → Vercel deployment (https://cambrian-api.onrender.com)

### API Status
- **URL:** https://cambrian-api.onrender.com
- **Status:** ✅ OPERATIONAL
- **Endpoints Working:**
  - `/auth/health`
  - `/catalog`
  - `/auth/register`
  - `/auth/login`
  - `/data/account`

## 🔧 Configuration Needed

### 1. PostgreSQL Database (Recommended)

**Why:** Currently using in-memory storage. Data is lost on restart.

**Quick Setup - Render PostgreSQL:**
1. Go to https://dashboard.render.com
2. Click "New +" → "PostgreSQL"
3. Settings:
   - Name: `cambrian-db`
   - Database: `cambrian`
   - Region: Same as your API service
4. Click "Create Database"
5. Once created, copy the "Internal Database URL"
6. Go to your API service → Environment tab
7. Add these variables:
   ```
   BILLING_CONNECTION_STRING=<paste-internal-url>
   PLAY_EVENTS_CONNECTION_STRING=<paste-internal-url>
   ```
8. Click "Save Changes" (will automatically redeploy)

**Connection String Format:**
```
postgresql://user:password@host:5432/dbname
```

Or ADO.NET format:
```
Host=your-host.render.com;Database=cambrian;Username=user;Password=pass;SslMode=Require
```

**Alternative Providers:**
- **Neon** (https://neon.tech) - Serverless PostgreSQL
- **Supabase** (https://supabase.com) - PostgreSQL + extras
- **AWS RDS** - For AWS deployments

### 2. Stripe Integration (Optional)

**Why:** Enable subscription payments for Listener ($9/mo) and Creator ($19/mo) plans.

**Setup Steps:**

#### A. Get API Keys
1. Go to https://dashboard.stripe.com/apikeys
2. Copy your **Secret key** (starts with `sk_test_` for testing or `sk_live_` for production)

#### B. Create Products & Prices
1. Go to https://dashboard.stripe.com/products
2. Click "Add product"
3. Create **Listener Plan:**
   - Name: Listener Plan
   - Description: Stream and discover music
   - Price: $9.00 USD
   - Billing: Monthly recurring
   - Click "Save product"
   - Copy the **Price ID** (starts with `price_`)
4. Create **Creator Plan:**
   - Name: Creator Plan
   - Description: Upload tracks and manage royalties
   - Price: $19.00 USD
   - Billing: Monthly recurring
   - Click "Save product"
   - Copy the **Price ID**

#### C. Set Up Webhook
1. Go to https://dashboard.stripe.com/webhooks
2. Click "Add endpoint"
3. Endpoint URL: `https://cambrian-api.onrender.com/webhook/stripe`
4. Description: Cambrian API Webhook
5. Events to send: Select `checkout.session.completed`
6. Click "Add endpoint"
7. Copy the **Signing secret** (starts with `whsec_`)

#### D. Add to Render Environment
Go to your API service → Environment → Add variables:
```
STRIPE_SECRET_KEY=sk_test_xxxxxxxxxxxxxxxxxxxxx
STRIPE_LISTENER_PRICE_ID=price_xxxxxxxxxxxxxxxxxxxxx
STRIPE_CREATOR_PRICE_ID=price_xxxxxxxxxxxxxxxxxxxxx
STRIPE_SUCCESS_URL=https://your-vercel-app.vercel.app/account?status=success
STRIPE_CANCEL_URL=https://your-vercel-app.vercel.app/account?status=cancel
STRIPE_WEBHOOK_SECRET=whsec_xxxxxxxxxxxxxxxxxxxxx
```

Replace `your-vercel-app.vercel.app` with your actual Vercel frontend URL.

### 3. CORS Configuration (Required for Frontend)

**Add to Render Environment:**
```
CORS_ORIGINS=https://your-vercel-app.vercel.app
```

**Multiple origins:**
```
CORS_ORIGINS=https://your-app.vercel.app,https://staging-your-app.vercel.app
```

**Include localhost for local testing:**
```
CORS_ORIGINS=http://localhost:5173,http://localhost:5174,https://your-vercel-app.vercel.app
```

## 🚀 Deploy Frontend

### Commit Changes
```bash
cd C:\Users\logan\Dev\Cambrian\cambrian-app
git status
git add .env .env.local .env.production
git commit -m "feat: configure production API endpoint"
git push origin main
```

### Deploy to Vercel
If your Vercel project is already connected to GitHub:
1. Push will automatically trigger deployment
2. Vercel will use `.env.production` values
3. Check deployment at https://vercel.com/dashboard

### Manual Vercel Deployment
```bash
cd C:\Users\logan\Dev\Cambrian\cambrian-app
vercel --prod
```

## 🧪 Testing

### Test API Directly
```powershell
# Health check
Invoke-WebRequest https://cambrian-api.onrender.com/auth/health

# Get catalog
Invoke-WebRequest https://cambrian-api.onrender.com/catalog

# Register user
$body = @{
    email = "test@example.com"
    password = "SecurePass123"
    plan = "Listener"
} | ConvertTo-Json

Invoke-WebRequest https://cambrian-api.onrender.com/auth/register `
    -Method POST -Body $body -ContentType "application/json"
```

### Test Frontend Locally
```bash
cd C:\Users\logan\Dev\Cambrian\cambrian-app
npm install
npm run dev
```
Visit http://localhost:5173 and test:
- Registration
- Login
- Browse catalog
- Account page

### Test Frontend on Vercel
After deployment, visit your Vercel URL and test the same flows.

## 📋 Environment Variables Checklist

### Required (Already Set)
- ✅ `ASPNETCORE_ENVIRONMENT=Production`
- ✅ `ASPNETCORE_URLS=http://0.0.0.0:$PORT`

### Recommended
- [ ] `CORS_ORIGINS=https://your-vercel-app.vercel.app`
- [ ] `BILLING_CONNECTION_STRING=...` (PostgreSQL)
- [ ] `PLAY_EVENTS_CONNECTION_STRING=...` (PostgreSQL)

### Optional (Stripe)
- [ ] `STRIPE_SECRET_KEY=sk_test_...`
- [ ] `STRIPE_LISTENER_PRICE_ID=price_...`
- [ ] `STRIPE_CREATOR_PRICE_ID=price_...`
- [ ] `STRIPE_SUCCESS_URL=...`
- [ ] `STRIPE_CANCEL_URL=...`
- [ ] `STRIPE_WEBHOOK_SECRET=whsec_...`

## 🔍 Troubleshooting

### Frontend Can't Connect to API
1. Check CORS_ORIGINS includes your frontend URL
2. Verify API is running: `Invoke-WebRequest https://cambrian-api.onrender.com/auth/health`
3. Check browser console for CORS errors
4. Ensure `.env.production` is committed to git

### Data Disappears After API Restart
- This is expected with in-memory storage
- Solution: Configure PostgreSQL connection strings

### Stripe Checkout Not Working
1. Verify all 6 Stripe environment variables are set
2. Check Stripe dashboard for API key status
3. Ensure webhook endpoint is added and active
4. Test mode: Use test keys and test card (4242 4242 4242 4242)

### CORS Errors in Browser
Add your Vercel URL to CORS_ORIGINS:
```
CORS_ORIGINS=https://your-actual-vercel-url.vercel.app
```

## 📖 Documentation

- **API Reference:** `API_REFERENCE.md`
- **Configuration Guide:** `CONFIGURATION_GUIDE.md`
- **Deployment Guide:** `DEPLOYMENT.md`
- **Testing Script:** `test-api-endpoints.ps1`

## ✅ Quick Checklist

- [x] API deployed and operational
- [x] Frontend .env files configured
- [ ] CORS configured for frontend URL
- [ ] PostgreSQL database connected (optional)
- [ ] Stripe integration configured (optional)
- [ ] Frontend deployed to Vercel
- [ ] End-to-end testing complete

---

**Ready to go live!** 🎉

Your API is working and your frontend is configured. Add the optional configurations as needed for your production deployment.
