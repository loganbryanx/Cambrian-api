# Cambrian API - Quick Reference

## 🔗 API Base URL
```
https://cambrian-api.onrender.com
```

## 📍 Available Endpoints

### Public Endpoints (No Auth Required)

#### Health Check
```http
GET /auth/health
Response: {"status":"ok"}
```

#### Get Catalog
```http
GET /catalog
Response: [
  {
    "id": "abc123",
    "title": "Aurora Run",
    "artist": "Skyline Audio",
    "genre": "Ambient",
    "price": 29,
    "rights": "Creator-owned"
  },
  ...
]
```

#### Get Plans
```http
GET /plans
Response: [
  {
    "plan": "Listener",
    "price": 9,
    "features": ["Browse catalog", "Stream music", ...]
  },
  ...
]
```

### Authentication Endpoints

#### Register
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123",
  "plan": "Listener"  // or "Creator"
}

Response: {"token": "base64-encoded-token"}
```

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123"
}

Response: {"token": "base64-encoded-token"}
```

### Account Endpoints

#### Get Account Data
```http
GET /data/account
x-email: user@example.com

Response: {
  "id": "abc123",
  "email": "user@example.com",
  "plan": "Listener",
  "region": "US",
  "status": "Active",
  "membership": "Listener"
}
```

## 🧪 PowerShell Test Examples

### Health Check
```powershell
Invoke-WebRequest https://cambrian-api.onrender.com/auth/health
```

### Register User
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
    plan = "Listener"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/auth/register" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"
```

### Login
```powershell
$body = @{
    email = "test@example.com"
    password = "Test123!"
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/auth/login" `
    -Method POST `
    -Body $body `
    -ContentType "application/json"

$loginData = $response.Content | ConvertFrom-Json
$token = $loginData.token
```

### Get Account
```powershell
$headers = @{ "x-email" = "test@example.com" }
Invoke-WebRequest -Uri "https://cambrian-api.onrender.com/data/account" `
    -Headers $headers
```

## 🌐 JavaScript/Fetch Examples

### Register User
```javascript
const response = await fetch('https://cambrian-api.onrender.com/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'test@example.com',
    password: 'Test123!',
    plan: 'Listener'
  })
});
const data = await response.json();
console.log(data.token);
```

### Get Catalog
```javascript
const response = await fetch('https://cambrian-api.onrender.com/catalog');
const tracks = await response.json();
console.log(tracks);
```

## ⚙️ Required Render Environment Variables

### Minimum (Already Set)
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://0.0.0.0:$PORT
```

### For Your Frontend
```bash
CORS_ORIGINS=https://your-app.vercel.app
```

### For Database Persistence (Optional)
```bash
BILLING_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
PLAY_EVENTS_CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
```

### For Stripe Payments (Optional)
```bash
STRIPE_SECRET_KEY=sk_test_...
STRIPE_LISTENER_PRICE_ID=price_...
STRIPE_CREATOR_PRICE_ID=price_...
STRIPE_SUCCESS_URL=https://your-app.vercel.app/account?status=success
STRIPE_CANCEL_URL=https://your-app.vercel.app/account?status=cancel
STRIPE_WEBHOOK_SECRET=whsec_...
```

## 🚀 Frontend Integration

### React/Next.js Example
```javascript
// config.js
export const API_URL = 'https://cambrian-api.onrender.com';

// api.js
import { API_URL } from './config';

export async function register(email, password, plan = 'Listener') {
  const response = await fetch(`${API_URL}/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password, plan })
  });
  return response.json();
}

export async function getCatalog() {
  const response = await fetch(`${API_URL}/catalog`);
  return response.json();
}
```

## 📊 Current Status

✅ **Working:**
- Health checks
- User registration & login
- Catalog browsing
- Account management
- In-memory data storage

⚠️ **Needs Configuration:**
- CORS for your frontend domain
- PostgreSQL for data persistence
- Stripe for payment processing

## 🔍 Troubleshooting

### CORS Error
Add your frontend URL to `CORS_ORIGINS` in Render environment variables.

### Connection Closed
Check Render logs for port binding. Should see: `Now listening on: http://0.0.0.0:10000`

### 404 Not Found
Verify the endpoint exists in the API. Use `/auth/health` to test connectivity.

### Data Lost After Restart
Normal with in-memory storage. Add PostgreSQL connection strings for persistence.

## 📞 Testing Script
Run comprehensive tests:
```powershell
.\test-api-endpoints.ps1
```

---

**Last Updated:** Deployment successful on February 2, 2026
**API Version:** .NET 8.0
**Hosting:** Render.com
