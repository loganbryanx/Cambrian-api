# CORS Configuration for Frontend Deployment

When you deploy the frontend to Vercel, you need to update the CORS configuration in the backend API to allow requests from your Vercel domain.

## Current Configuration

The API currently allows requests from:
- `http://localhost:5173` (Vite dev server)
- `http://localhost:5174` (Vite dev server alternate port)

## Updated Configuration for Vercel

Update `src/Cambrian.Api/Program.cs`:

### Option 1: Hardcoded Origins (Quick Start)

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173",
            "http://localhost:5174",
            "https://cambrian-app.vercel.app",           // Your Vercel production domain
            "https://cambrian-app-*.vercel.app"          // Vercel preview deployments (won't work with wildcards in .WithOrigins)
        )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();  // Add if using cookies/authentication
    });
});
```

**Note**: .NET's `WithOrigins()` doesn't support wildcards. For preview deployments, see Option 2.

### Option 2: Environment Variable Configuration (Recommended)

In `appsettings.json`:

```json
{
  "AllowedOrigins": [
    "http://localhost:5173",
    "http://localhost:5174"
  ]
}
```

In `appsettings.Production.json` (or environment variables):

```json
{
  "AllowedOrigins": [
    "https://cambrian-app.vercel.app",
    "https://www.cambrian-app.com"
  ]
}
```

In `Program.cs`:

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

### Option 3: Dynamic CORS with Pattern Matching (Advanced)

For handling Vercel preview deployments (e.g., `cambrian-app-git-branch-*.vercel.app`):

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
        {
            // Always allow localhost for development
            if (origin.StartsWith("http://localhost"))
                return true;
            
            // Allow your production domain
            if (origin == "https://cambrian-app.vercel.app")
                return true;
            
            // Allow Vercel preview deployments
            if (origin.StartsWith("https://cambrian-app-") && origin.EndsWith(".vercel.app"))
                return true;
            
            // Allow custom domain
            if (origin == "https://www.cambrian-app.com" || origin == "https://cambrian-app.com")
                return true;
            
            return false;
        })
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
```

## Environment Variables for Vercel Domains

If deploying to Azure App Service, AWS, or similar, set these environment variables:

```bash
ALLOWED_ORIGINS__0=https://cambrian-app.vercel.app
ALLOWED_ORIGINS__1=https://www.cambrian-app.com
```

Or in Azure App Service Configuration:

```
AllowedOrigins:0 = https://cambrian-app.vercel.app
AllowedOrigins:1 = https://www.cambrian-app.com
```

## Testing CORS

### Test from Frontend

In your frontend app (cambrian-app), make an API call:

```javascript
fetch('https://your-api.azurewebsites.net/auth/health', {
  method: 'GET',
  headers: {
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log('API Health:', data))
.catch(error => console.error('CORS Error:', error));
```

### Test with cURL

```bash
curl -H "Origin: https://cambrian-app.vercel.app" \
     -H "Access-Control-Request-Method: GET" \
     -H "Access-Control-Request-Headers: Content-Type" \
     -X OPTIONS \
     --verbose \
     https://your-api.azurewebsites.net/auth/health
```

Look for these headers in the response:
- `Access-Control-Allow-Origin: https://cambrian-app.vercel.app`
- `Access-Control-Allow-Methods: GET, POST, ...`
- `Access-Control-Allow-Headers: Content-Type, ...`

## Common CORS Errors

### Error 1: "No 'Access-Control-Allow-Origin' header"

**Cause**: Backend isn't configured to allow requests from your domain.

**Solution**: Add your Vercel domain to allowed origins.

### Error 2: "CORS policy: Response to preflight request doesn't pass"

**Cause**: OPTIONS request (CORS preflight) isn't handled correctly.

**Solution**: Ensure `AllowAnyMethod()` is included in CORS policy.

### Error 3: "Credentials flag is 'true', but 'Access-Control-Allow-Credentials' header is ''"

**Cause**: Frontend sends credentials, but backend doesn't allow them.

**Solution**: Add `.AllowCredentials()` to CORS policy.

### Error 4: Wildcard in 'Access-Control-Allow-Origin' not allowed with credentials

**Cause**: Using `AllowAnyOrigin()` with `AllowCredentials()`.

**Solution**: Use specific origins with `WithOrigins()` or `SetIsOriginAllowed()`.

## Security Considerations

### 1. Don't Use `AllowAnyOrigin()` in Production

```csharp
// ❌ BAD - Allows requests from any domain
policy.AllowAnyOrigin()
```

```csharp
// ✅ GOOD - Specific domains only
policy.WithOrigins("https://cambrian-app.vercel.app")
```

### 2. Validate Origins Carefully

If using pattern matching, be specific:

```csharp
// ❌ TOO PERMISSIVE - Allows ANY subdomain
if (origin.EndsWith(".vercel.app"))

// ✅ BETTER - Specific to your project
if (origin.StartsWith("https://cambrian-app-") && origin.EndsWith(".vercel.app"))
```

### 3. Use HTTPS Only in Production

```csharp
policy.SetIsOriginAllowed(origin =>
{
    // In production, only allow HTTPS
    if (builder.Environment.IsProduction() && !origin.StartsWith("https://"))
        return false;
    
    // ... rest of validation
});
```

## Quick Reference

| Environment | Allowed Origins |
|-------------|-----------------|
| Local Dev | `http://localhost:5173` |
| Vercel Production | `https://cambrian-app.vercel.app` |
| Vercel Preview | `https://cambrian-app-git-*.vercel.app` |
| Custom Domain | `https://www.cambrian-app.com` |

## Deployment Checklist

- [ ] Update CORS configuration in backend API
- [ ] Deploy backend changes
- [ ] Set `VITE_API_URL` environment variable in Vercel
- [ ] Deploy frontend to Vercel
- [ ] Test API connectivity from deployed frontend
- [ ] Verify no CORS errors in browser console
- [ ] Test on different browsers (Chrome, Firefox, Safari)
- [ ] Test with authentication/credentials if applicable

---

**Next Steps**: After updating CORS, deploy the backend, then deploy the frontend following the instructions in `FRONTEND_VERCEL_DEPLOYMENT.md`.
