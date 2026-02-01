# Deployment Guide

## Render Deployment

This API is configured to deploy to Render using Docker. The configuration is defined in `render.yaml`.

### CORS Configuration for Production

The API uses a configurable CORS policy that allows requests from specific origins. To configure allowed origins in production:

1. **On Render Dashboard:**
   - Go to your service settings
   - Find the "Environment Variables" section
   - Add or update the `Cors__AllowedOrigins` environment variable
   - Set the value to a comma-separated list of allowed origins

   Example:
   ```
   Cors__AllowedOrigins=https://your-vercel-app.vercel.app,https://another-domain.com
   ```

2. **Important Notes:**
   - Use the double underscore `__` syntax for nested configuration in environment variables
   - Include the full URL with protocol (https:// or http://)
   - Separate multiple origins with commas (no spaces)
   - The default origins (localhost:5173, localhost:5174) are configured in `appsettings.json` for development

### Health Check

The API exposes a health check endpoint at `/auth/health` that returns:
```json
{"status": "ok"}
```

This endpoint is configured in `render.yaml` and is used by Render to monitor the service health.

### Testing the Deployment

1. **Health Check:**
   ```bash
   curl https://YOUR-RENDER-API.onrender.com/auth/health
   ```
   Should return: `{"status":"ok"}`

2. **CORS Headers:**
   ```bash
   curl -i -H "Origin: https://your-vercel-app.vercel.app" \
        -H "Access-Control-Request-Method: POST" \
        -X OPTIONS \
        https://YOUR-RENDER-API.onrender.com/auth/login
   ```
   Should include `Access-Control-Allow-Origin` header in the response.

### Frontend Integration

When integrating with a Vercel-deployed frontend:

1. Deploy your API to Render
2. Note your Render API URL (e.g., `https://cambrian-api.onrender.com`)
3. Add your Vercel frontend URL to the `Cors__AllowedOrigins` environment variable in Render
4. Configure your frontend to use the Render API URL
5. Test the signup → Discover → Save → Library flow

If you encounter CORS errors, ensure:
- The Vercel URL is added to `Cors__AllowedOrigins` in Render
- The URL includes the correct protocol (https://)
- There are no trailing slashes in the URLs
- The service has been redeployed after changing environment variables
