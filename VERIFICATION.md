# Deployment Verification Checklist

This guide provides a step-by-step checklist to verify your Cambrian API deployment on Render and frontend integration with Vercel.

## Prerequisites

Before starting verification:
- [ ] API deployed to Render
- [ ] Frontend deployed to Vercel
- [ ] You have your Render API URL (e.g., `https://cambrian-api-xxxxx.onrender.com`)
- [ ] You have your Vercel frontend URL (e.g., `https://your-app.vercel.app`)

---

## Step 1: Verify Render API Health Check

The health check endpoint confirms your API is running correctly.

### Test Command:
```bash
curl https://YOUR-RENDER-API.onrender.com/auth/health
```

**Replace `YOUR-RENDER-API` with your actual Render service name.**

### Expected Response:
```json
{"status":"ok"}
```

### ✅ Verification Checklist:
- [ ] Health check returns `{"status":"ok"}`
- [ ] Response time is reasonable (< 5 seconds for first request after sleep)
- [ ] No error messages in response

### ❌ If Health Check Fails:

**Check Render Dashboard:**
1. Log in to [Render Dashboard](https://dashboard.render.com/)
2. Click on your `cambrian-api` service
3. Verify service status is "Live" (green)
4. Check "Logs" tab for any error messages
5. Verify last deploy was successful

**Common Issues:**
- **Service is deploying:** Wait for deployment to complete
- **Service crashed:** Check logs for errors, redeploy if needed
- **Wrong URL:** Verify URL in Render dashboard matches what you're testing
- **Free tier sleeping:** First request takes 30-60 seconds, try again

---

## Step 2: Verify Vercel Environment Variables

Ensure your Vercel frontend is configured to use the correct Render API URL.

### Check Vercel Configuration:

1. **Log in to [Vercel Dashboard](https://vercel.com/dashboard)**
2. **Click on your frontend project**
3. **Go to Settings → Environment Variables**
4. **Verify the following variables are set:**

   ```
   Variable Name: VITE_API_URL (or NEXT_PUBLIC_API_URL, or similar)
   Value: https://YOUR-RENDER-API.onrender.com
   ```

   **Important:** Use your actual Render API URL, including `https://`

### ✅ Verification Checklist:
- [ ] API URL environment variable exists
- [ ] API URL points to your Render deployment (not localhost)
- [ ] API URL includes `https://` protocol
- [ ] API URL does NOT have trailing slash
- [ ] Variable is set for Production environment

### Update and Redeploy:

If you made any changes to environment variables:

1. **Save the environment variable changes**
2. **Go to Deployments tab**
3. **Click "Redeploy" on the latest deployment**
4. **Select "Use existing Build Cache" (optional for faster deploy)**
5. **Click "Redeploy"**
6. **Wait for deployment to complete** (check status turns to "Ready")

---

## Step 3: Verify CORS Configuration on Render

The API must allow requests from your Vercel frontend URL.

### Check CORS Configuration:

1. **Log in to [Render Dashboard](https://dashboard.render.com/)**
2. **Click on your `cambrian-api` service**
3. **Go to Environment tab**
4. **Verify `Cors__AllowedOrigins` is set:**

   ```
   Key: Cors__AllowedOrigins
   Value: https://your-app.vercel.app
   ```

   **Important:** Must match your exact Vercel URL (production URL)

### Test CORS with curl:
```bash
curl -i -H "Origin: https://your-app.vercel.app" \
     -H "Access-Control-Request-Method: POST" \
     -X OPTIONS \
     https://YOUR-RENDER-API.onrender.com/auth/login
```

### Expected Response Headers:
```
HTTP/1.1 204 No Content
Access-Control-Allow-Origin: https://your-app.vercel.app
Access-Control-Allow-Methods: POST
Access-Control-Allow-Headers: Content-Type
```

### ✅ Verification Checklist:
- [ ] `Cors__AllowedOrigins` environment variable is set on Render
- [ ] Value includes your Vercel URL with `https://`
- [ ] Multiple URLs separated by commas (if applicable)
- [ ] CORS preflight test returns `Access-Control-Allow-Origin` header
- [ ] Origin in response matches your Vercel URL

### Update CORS Configuration:

If CORS is not configured correctly:

1. **In Render Dashboard → Environment**
2. **Add or update `Cors__AllowedOrigins`:**
   ```
   Key: Cors__AllowedOrigins
   Value: https://your-app.vercel.app
   ```
3. **Click "Save Changes"**
4. **Service will automatically redeploy**
5. **Wait for redeploy to complete** (check Logs tab)

---

## Step 4: Test Frontend Integration

Now test the complete user flow in your frontend application.

### Open Your Vercel URL in Browser:

1. **Navigate to:** `https://your-app.vercel.app`
2. **Open Browser Developer Tools:**
   - Chrome/Edge: Press `F12` or `Ctrl+Shift+I` (Win/Linux) / `Cmd+Option+I` (Mac)
   - Firefox: Press `F12` or `Ctrl+Shift+K` (Win/Linux) / `Cmd+Option+K` (Mac)
3. **Click on the "Console" tab**
4. **Keep it open to watch for errors**

### Test Flow: Signup → Discover → Save → Library

#### 4.1: Test Signup
- [ ] Click "Sign Up" or similar button
- [ ] Fill in email and password
- [ ] Submit the form
- [ ] **✅ Success:** User is signed up and logged in
- [ ] **✅ Success:** No CORS errors in console
- [ ] **❌ Check console for errors if signup fails**

#### 4.2: Test Discover
- [ ] Navigate to "Discover" page/section
- [ ] **✅ Success:** Page loads with content
- [ ] **✅ Success:** No CORS errors in console
- [ ] **✅ Success:** API requests succeed (check Network tab)
- [ ] **❌ Check console for errors if discover fails**

#### 4.3: Test Save
- [ ] Find an item to save (track, content, etc.)
- [ ] Click "Save" or similar action
- [ ] **✅ Success:** Item is saved
- [ ] **✅ Success:** UI updates to show saved state
- [ ] **✅ Success:** No CORS errors in console
- [ ] **❌ Check console for errors if save fails**

#### 4.4: Test Library
- [ ] Navigate to "Library" page/section
- [ ] **✅ Success:** Previously saved items appear
- [ ] **✅ Success:** Page loads correctly
- [ ] **✅ Success:** No CORS errors in console
- [ ] **❌ Check console for errors if library fails**

---

## Step 5: Verify No CORS Errors

Check the browser console for any CORS-related errors.

### ✅ Success - No CORS Errors:

Console should show successful API requests like:
```
POST https://your-api.onrender.com/auth/login 200 OK
GET https://your-api.onrender.com/catalog 200 OK
```

### ❌ CORS Error Found:

If you see an error like:
```
Access to fetch at 'https://your-api.onrender.com/auth/login' from origin 
'https://your-app.vercel.app' has been blocked by CORS policy: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

**This means CORS is not configured correctly. Do the following:**

1. **Verify Vercel URL in error message matches your CORS configuration**
2. **Go back to Step 3** and verify CORS configuration
3. **Ensure `Cors__AllowedOrigins` on Render includes the exact URL from the error**
4. **Redeploy Render service after changing CORS config**
5. **Hard refresh your browser** (Ctrl+Shift+R or Cmd+Shift+R)
6. **Test again**

### Share CORS Error for Help:

If CORS errors persist after verifying configuration:

1. **Copy the complete error message from console**
2. **Check Network tab:**
   - Click on the failed request
   - Copy Request URL and Status Code
   - Copy Response headers
3. **Create a [deployment issue](https://github.com/loganbryanx/Cambrian-api/issues/new?template=deployment-issue.md)**
4. **Include:**
   - Your Vercel URL (from browser address bar)
   - Your Render API URL
   - Complete CORS error from console
   - `Cors__AllowedOrigins` value from Render

---

## Quick Verification Script

For quick automated verification of API health and CORS:

```bash
#!/bin/bash

# Set your URLs
RENDER_API_URL="https://YOUR-RENDER-API.onrender.com"
VERCEL_URL="https://your-app.vercel.app"

echo "=== Cambrian API Deployment Verification ==="
echo ""

# Test 1: Health Check
echo "1. Testing Health Check..."
HEALTH_RESPONSE=$(curl -s "$RENDER_API_URL/auth/health")
if [[ $HEALTH_RESPONSE == *"ok"* ]]; then
  echo "   ✅ Health check passed: $HEALTH_RESPONSE"
else
  echo "   ❌ Health check failed: $HEALTH_RESPONSE"
fi
echo ""

# Test 2: CORS Preflight
echo "2. Testing CORS Configuration..."
CORS_RESPONSE=$(curl -s -I -H "Origin: $VERCEL_URL" \
  -H "Access-Control-Request-Method: POST" \
  -X OPTIONS "$RENDER_API_URL/auth/login" | grep -i "access-control-allow-origin")

if [[ $CORS_RESPONSE == *"$VERCEL_URL"* ]]; then
  echo "   ✅ CORS configured correctly: $CORS_RESPONSE"
else
  echo "   ❌ CORS not configured for $VERCEL_URL"
  echo "   Response: $CORS_RESPONSE"
fi
echo ""

# Test 3: API Request
echo "3. Testing API Request..."
API_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$RENDER_API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -H "Origin: $VERCEL_URL" \
  -d '{"email":"test@test.com","password":"test"}')

HTTP_CODE=$(echo "$API_RESPONSE" | tail -n1)
if [[ $HTTP_CODE == "401" ]] || [[ $HTTP_CODE == "200" ]]; then
  echo "   ✅ API responding (status: $HTTP_CODE)"
else
  echo "   ❌ API error (status: $HTTP_CODE)"
fi
echo ""

echo "=== Verification Complete ==="
echo ""
echo "Next Steps:"
echo "1. If all checks passed, test frontend at: $VERCEL_URL"
echo "2. If any checks failed, see TROUBLESHOOTING.md"
echo "3. Test user flow: Signup → Discover → Save → Library"
```

**To use this script:**
1. Save it as `verify-deployment.sh`
2. Make it executable: `chmod +x verify-deployment.sh`
3. Edit the URLs at the top
4. Run: `./verify-deployment.sh`

---

## Verification Summary

### ✅ All Checks Passed:

If all verification steps passed:
- ✅ Render API is healthy
- ✅ Vercel is configured with correct API URL
- ✅ CORS is configured correctly
- ✅ Frontend integration works
- ✅ User flow (signup → discover → save → library) works

**🎉 Your deployment is successful!**

### ❌ Some Checks Failed:

If any verification failed:

1. **Review the specific step that failed**
2. **Follow the troubleshooting steps in that section**
3. **Check [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for detailed help**
4. **Create a [deployment issue](https://github.com/loganbryanx/Cambrian-api/issues/new?template=deployment-issue.md) if needed**

---

## Additional Resources

- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Initial deployment instructions
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Detailed troubleshooting guide
- **[Render Documentation](https://render.com/docs)** - Render platform help
- **[Vercel Documentation](https://vercel.com/docs)** - Vercel platform help

---

## Need Help?

If you've followed all verification steps and still have issues:

1. **Gather information** using [TROUBLESHOOTING.md](TROUBLESHOOTING.md)
2. **Create an issue** using the [deployment issue template](https://github.com/loganbryanx/Cambrian-api/issues/new?template=deployment-issue.md)
3. **Include all verification results** and error messages
