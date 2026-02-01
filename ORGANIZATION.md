# Cambrian Organization Structure

This repository is part of the **Cambrian** organization, which consists of three independent but interconnected repositories.

## The Three Repositories

### 🎨 [cambrian-frontend](https://github.com/Cambrian/cambrian-frontend)
**Domain**: User Interface & Client-Side Logic
- React/Next.js application
- User interface components
- Client-side state management
- Consumes APIs from cambrian-api
- Deployed independently to Vercel/similar platform

### 🔌 [cambrian-api](https://github.com/Cambrian/cambrian-api) (This Repository)
**Domain**: Backend Services & Business Logic
- ASP.NET Core Web API
- RESTful API endpoints
- Authentication & authorization
- Business logic and data processing
- Database management via Entity Framework Core
- Deployed independently to Render/similar platform

### 🏗️ [cambrian-infra](https://github.com/Cambrian/cambrian-infra)
**Domain**: Infrastructure & DevOps
- Infrastructure as Code (Terraform/CloudFormation)
- CI/CD pipelines
- Monitoring and logging configuration
- Shared environment configuration
- Deployment scripts and automation

## Architecture Principles

Each repository:
- **Owns its domain** - Clear separation of concerns
- **Has its own README** - Self-contained documentation
- **Deploys independently** - Separate deployment pipelines
- **Maintains its own tests** - Independent quality gates
- **Has clear interfaces** - Well-defined APIs between services

## Getting Started

To work on the full Cambrian ecosystem:

1. **Clone all three repositories**:
   ```bash
   git clone https://github.com/Cambrian/cambrian-frontend.git
   git clone https://github.com/Cambrian/cambrian-api.git
   git clone https://github.com/Cambrian/cambrian-infra.git
   ```

2. **Set up each repository independently** (refer to each repo's README)

3. **Configure environment variables** to connect the services

## Repository Relationships

```
┌─────────────────────┐
│  cambrian-frontend  │
│   (User Interface)  │
└──────────┬──────────┘
           │
           │ HTTP/REST
           │
           ▼
┌─────────────────────┐
│   cambrian-api      │
│  (Backend Services) │
└──────────┬──────────┘
           │
           │ Managed by
           │
           ▼
┌─────────────────────┐
│  cambrian-infra     │
│ (Infrastructure)    │
└─────────────────────┘
```

## Contributing

When contributing to the Cambrian ecosystem:

1. **Identify the right repository** for your change
2. **Follow the repository's contribution guidelines**
3. **Consider cross-repository impacts**
4. **Update documentation** in affected repositories
5. **Test integration points** between services

## Support

For questions about:
- **Frontend issues** → Open an issue in cambrian-frontend
- **API/backend issues** → Open an issue in cambrian-api
- **Infrastructure/deployment issues** → Open an issue in cambrian-infra
- **Cross-cutting concerns** → Open an issue in the primary affected repository and reference others

## Organization Home

Visit the [Cambrian Organization](https://github.com/Cambrian) on GitHub to see all repositories and organizational resources.
