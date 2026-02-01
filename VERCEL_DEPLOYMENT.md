# Vercel Deployment Guide for Cambrian API

This guide provides step-by-step instructions for deploying the Cambrian ASP.NET Core API to Vercel.

## Important Note

Vercel is primarily optimized for frontend applications and serverless functions (Node.js, Python, Go, Ruby). For ASP.NET Core APIs, you have a few options:

### Option 1: Deploy to Vercel using Docker (Recommended for Enterprise)
Requires Vercel Pro or Enterprise plan with Docker support.

### Option 2: Alternative Platform (Recommended)
Consider using platforms better suited for .NET:
- **Azure App Service** - Native .NET support
- **AWS Elastic Beanstalk** - Container support
- **Render** - Docker support on free tier
- **Railway** - Docker support with generous free tier
- **Fly.io** - Docker support with free tier

## Vercel Deployment Steps (If Using Docker Support)

### 1. Create Project in Vercel

1. Go to [Vercel](https://vercel.com) → **New Project**
2. Click **Import Git Repository**
3. Select the repository: `loganbryanx/Cambrian-api`
4. Authenticate with GitHub if needed

### 2. Configure Build Settings

Since this is a .NET application, configure the following:

- **Framework Preset**: Other
- **Root Directory**: Leave as root (`.`)
- **Build Command**: 
  ```bash
  dotnet publish src/Cambrian.Api/Cambrian.Api.csproj -c Release -o out
  ```
- **Output Directory**: `out`
- **Install Command**: 
  ```bash
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0
  ```

### 3. Environment Variables

Add the following environment variables under **Project Settings** → **Environment Variables**:

#### Production Environment
- `ASPNETCORE_ENVIRONMENT` = `Production`
- `ASPNETCORE_URLS` = `http://+:8080`
- `ConnectionStrings__DefaultConnection` = `[Your database connection string]` (if using database)

#### Preview/Development Environment  
- `ASPNETCORE_ENVIRONMENT` = `Development`
- `ASPNETCORE_URLS` = `http://+:8080`

### 4. Docker Deployment (Pro/Enterprise)

If you have access to Vercel's Docker deployment:

The included `Dockerfile` is already configured:
- Uses .NET 8.0 SDK for build
- Uses .NET 8.0 runtime for production
- Exposes port 3000
- Optimized multi-stage build

Note: Update the Dockerfile to use .NET 10.0 if needed (the project currently targets .NET 10.0).

### 5. Deploy

1. Click **Deploy** to trigger the initial build
2. Vercel will attempt to build and deploy the project
3. Monitor the deployment logs for any issues

### 6. Post-Deployment

After successful deployment:

1. **Verify API endpoints**: Test the `/auth/health` endpoint
2. **Configure CORS**: Update the CORS policy in `Program.cs` to include your Vercel domain
3. **Custom Domain** (optional): Add under Project Settings → Domains
4. **Database**: Ensure your database is accessible from Vercel's infrastructure

## Configuration Files

This repository includes:

- `vercel.json` - Vercel project configuration
- `.vercelignore` - Files to exclude from deployment
- `Dockerfile` - For containerized deployment
- `docker-compose.yml` - For local development with database

## API Endpoints

Once deployed, your API will be available at:
```
https://[your-project-name].vercel.app
```

Health check endpoint:
```
https://[your-project-name].vercel.app/auth/health
```

## Troubleshooting

### Common Issues

1. **.NET Runtime Not Available**
   - Vercel doesn't natively support .NET runtime
   - Consider using Docker deployment or alternative platforms

2. **Build Timeout**
   - .NET builds can be slow
   - Optimize by caching NuGet packages
   - Consider using a different platform

3. **Database Connectivity**
   - Ensure database allows connections from Vercel IPs
   - Use connection pooling
   - Consider using serverless databases (Azure SQL, AWS RDS, PlanetScale)

4. **CORS Issues**
   - Update the CORS policy in `src/Cambrian.Api/Program.cs`
   - Add your Vercel domain to the allowed origins

## Alternative: Deploy Frontend to Vercel, API Elsewhere

A common pattern:
1. Deploy your frontend (React/Vue/Vite app) to Vercel
2. Deploy your .NET API to Azure App Service, AWS, or Railway
3. Configure the frontend to call the API via environment variables

This is often the most practical approach as it leverages each platform's strengths.

## Development

For local development:

```bash
# Install dependencies
dotnet restore

# Build
dotnet build

# Run
dotnet run --project src/Cambrian.Api

# Run with Docker
docker compose up -d
```

## Additional Resources

- [Vercel Documentation](https://vercel.com/docs)
- [ASP.NET Core Deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [Docker Multi-stage Builds](https://docs.docker.com/build/building/multi-stage/)
