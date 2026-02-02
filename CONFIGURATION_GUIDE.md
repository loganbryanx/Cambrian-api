# Cambrian API Configuration Guide

## ✅ Currently Working
- ✅ Health checks (`/auth/health`)
- ✅ User registration (`/auth/register`)
- ✅ User login (`/auth/login`)
- ✅ Account data (`/data/account`)
- ✅ Catalog browsing (`/catalog`)
- ✅ In-memory data storage

## 🔧 Configuration Steps

### 1. Configure CORS for Your Frontend

**Current Status:** Default CORS allows `localhost:5173` and `localhost:5174`

**To Update:**
1. Go to Render Dashboard → Your Service → Environment
2. Add or update:
   ```
   CORS_ORIGINS=https://your-app.vercel.app,https://your-app-staging.vercel.app
   ```
3. Save and redeploy

**Test CORS:**
```powershell
$headers = @{ Origin = "https://your-app.vercel.app" }
Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/catalog" -Headers $headers
```

---

### 2. Set Up PostgreSQL Database (Optional but Recommended)

**Why:** Currently using in-memory storage (data lost on restart)

**Steps:**

#### Option A: Use Render PostgreSQL
1. In Render Dashboard, create a new PostgreSQL database
2. Copy the **Internal Database URL**
3. Add to your service Environment:
   ```
   BILLING_CONNECTION_STRING=postgresql://user:password@host:5432/dbname
   PLAY_EVENTS_CONNECTION_STRING=postgresql://user:password@host:5432/dbname
   ```

#### Option B: Use External PostgreSQL (Neon, Supabase, etc.)
1. Create a PostgreSQL database
2. Get the connection string
3. Add to Render environment variables (same format as above)

**Connection String Format:**
```
Host=your-host.region.provider.com;Database=cambrian;Username=user;Password=pass;SslMode=Require
```

**Test Database Connection:**
After setting environment variables, check Render logs for:
- `Ensuring PlayEvents table exists...`
- `Ensuring Billing tables exist...`

---

### 3. Configure Stripe Integration (Optional)

**Why:** Enable subscription payments and checkout

**Steps:**

1. **Get Stripe API Keys:**
   - Go to https://dashboard.stripe.com/apikeys
   - Copy your **Secret key** (starts with `sk_test_` or `sk_live_`)

2. **Create Stripe Products & Prices:**
   - Go to https://dashboard.stripe.com/products
   - Create two products:
     - **Listener Plan** ($9/month) → Copy Price ID
     - **Creator Plan** ($19/month) → Copy Price ID

3. **Set Up Webhook:**
   - Go to https://dashboard.stripe.com/webhooks
   - Add endpoint: `https://cambrian-api.onrender.com/webhook/stripe`
   - Select events: `checkout.session.completed`
   - Copy **Signing secret** (starts with `whsec_`)

4. **Add to Render Environment:**
   ```bash
   STRIPE_SECRET_KEY=sk_test_xxxxxxxxxxxxx
   STRIPE_LISTENER_PRICE_ID=price_xxxxxxxxxxxxx
   STRIPE_CREATOR_PRICE_ID=price_xxxxxxxxxxxxx
   STRIPE_SUCCESS_URL=https://your-app.vercel.app/account?status=success
   STRIPE_CANCEL_URL=https://your-app.vercel.app/account?status=cancel
   STRIPE_WEBHOOK_SECRET=whsec_xxxxxxxxxxxxx
   ```

5. **Save and Redeploy**

**Test Stripe:**
```powershell
$body = @{ email = "test@example.com"; plan = "Listener" } | ConvertTo-Json
Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/checkout/create-session" `
  -Method POST -Body $body -ContentType "application/json"
```

---

### 4. Connect Frontend to API

**Update your Vercel frontend environment variables:**

```bash
# In Vercel Dashboard → Your Project → Settings → Environment Variables
VITE_API_URL=https://cambrian-api.onrender.com
# or
NEXT_PUBLIC_API_URL=https://cambrian-api.onrender.com
```

**Frontend Code Example:**
```javascript
// In your frontend config
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:3000';

// Register user
const response = await fetch(`${API_URL}/auth/register`, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email, password, plan: 'Listener' })
});

// Get catalog
const catalog = await fetch(`${API_URL}/catalog`);
const tracks = await catalog.json();
```

---

### 5. Environment Variables Summary

**Required (Already Set):**
- ✅ `ASPNETCORE_ENVIRONMENT=Production`
- ✅ `ASPNETCORE_URLS=http://0.0.0.0:$PORT`

**Recommended:**
```bash
# CORS - Update for your domain
CORS_ORIGINS=https://your-app.vercel.app

# Database - For data persistence
BILLING_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
PLAY_EVENTS_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
```

**Optional (For Stripe payments):**
```bash
STRIPE_SECRET_KEY=sk_test_...
STRIPE_LISTENER_PRICE_ID=price_...
STRIPE_CREATOR_PRICE_ID=price_...
STRIPE_SUCCESS_URL=https://your-app.vercel.app/account?status=success
STRIPE_CANCEL_URL=https://your-app.vercel.app/account?status=cancel
STRIPE_WEBHOOK_SECRET=whsec_...
```

---

## 🧪 Quick Test Commands

```powershell
# Health check
Invoke-WebRequest https://cambrian-api.onrender.com/auth/health

# Get catalog
Invoke-WebRequest https://cambrian-api.onrender.com/catalog

# Register
$body = @{ email="test@example.com"; password="Test123"; plan="Listener" } | ConvertTo-Json
Invoke-WebRequest https://cambrian-api.onrender.com/auth/register -Method POST -Body $body -ContentType "application/json"

# Login
$body = @{ email="test@example.com"; password="Test123" } | ConvertTo-Json
Invoke-WebRequest https://cambrian-api.onrender.com/auth/login -Method POST -Body $body -ContentType "application/json"
```

---

## 📊 Monitoring & Logs

**View Logs:**
- Render Dashboard → Your Service → Logs tab
- Look for errors, database connections, request logs

**Check Service Health:**
- Render Dashboard → Your Service → Events tab
- Should show "Deploy live" with no errors

**Monitor Performance:**
- Render provides basic metrics in the dashboard
- Consider adding Application Insights or Datadog for production

---

## 🔒 Security Checklist

- [ ] Use strong database passwords
- [ ] Enable SSL for database connections (`SslMode=Require`)
- [ ] Use production Stripe keys for live environment
- [ ] Restrict CORS to your actual frontend domain(s)
- [ ] Rotate Stripe webhook secret if exposed
- [ ] Use environment variables (never commit secrets to git)

---

## 🚀 Next Steps

1. **Update CORS** for your frontend domain
2. **Optional:** Set up PostgreSQL for data persistence
3. **Optional:** Configure Stripe if you need payments
4. **Deploy frontend** and update API_URL
5. **Test end-to-end** user flows

Your API is ready to use! 🎉
