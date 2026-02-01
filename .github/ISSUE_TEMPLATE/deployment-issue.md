---
name: Deployment Issue
about: Report issues with Render API deployment or Vercel frontend integration
title: '[DEPLOYMENT] '
labels: deployment, help wanted
assignees: ''

---

## Deployment Information

**Render API URL:**
<!-- Example: https://cambrian-api-xxxxx.onrender.com -->


**Health Check Status:**
<!-- Paste the output of: curl https://YOUR-RENDER-API.onrender.com/auth/health -->
```

```

**Vercel Frontend URL:**
<!-- Example: https://your-app.vercel.app -->


**CORS Configuration (from Render Dashboard → Environment):**
<!-- Example: Cors__AllowedOrigins=https://your-app.vercel.app -->
```

```

## Error Details

**Browser Console Errors:**
<!-- Open Developer Tools → Console and paste any red error messages -->
```

```

**Network Tab Details:**
<!-- Open Developer Tools → Network, click on failed request -->
- Request URL: 
- Status Code: 
- Response Body: 
```

```

**Render Server Logs:**
<!-- If applicable, paste relevant logs from Render Dashboard → Logs -->
```

```

## Steps to Reproduce

1. 
2. 
3. 

## Expected Behavior

<!-- What should happen? -->


## Actual Behavior

<!-- What actually happens? -->


## Screenshots

<!-- If applicable, add screenshots of console errors or network tab -->


## Additional Context

<!-- Any other information that might be helpful -->


---

**Before submitting, please verify:**
- [ ] I've checked [TROUBLESHOOTING.md](../../TROUBLESHOOTING.md) for common solutions
- [ ] I've verified my Render API health check works: `/auth/health` returns `{"status":"ok"}`
- [ ] I've confirmed my Vercel URL is added to `Cors__AllowedOrigins` in Render
- [ ] I've included complete error messages from browser console
- [ ] I've checked that my service is deployed and running in Render Dashboard
