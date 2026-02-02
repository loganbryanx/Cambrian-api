# Cambrian API

ASP.NET Core API organized by clean architecture layers.

## 🚀 Quick Start

```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test

# Start API
dotnet run --project src/Cambrian.Api

# Health check
curl http://localhost:3000/auth/health
```

## 📁 Project Structure

```
Cambrian-api/
├── docs/                          # 📚 Documentation
│   ├── API_REFERENCE.md
│   ├── DEPLOYMENT.md
│   ├── TROUBLESHOOTING.md
│   └── ...
├── scripts/                       # 🔧 Utility scripts
│   ├── health-check.sh
│   ├── integration-test.sh
│   └── ...
├── src/                           # 💻 Source code
│   ├── Cambrian.Api/             # Web API
│   ├── music/Cambrian.Application/    # Application layer
│   ├── payments/Cambrian.Infrastructure/  # Infrastructure layer
│   └── users/Cambrian.Domain/    # Domain layer
├── tests/                         # 🧪 Tests
│   ├── Cambrian.Api.Tests/
│   └── Cambrian.Application.Tests/
└── docker/                        # 🐳 Docker configs
    ├── docker-compose.yml
    └── Dockerfile
```

## 🏗️ Architecture

This project follows **Clean Architecture** principles:

- **Domain Layer** (`Cambrian.Domain`): Core business entities and rules
- **Application Layer** (`Cambrian.Application`): Use cases and business logic
- **Infrastructure Layer** (`Cambrian.Infrastructure`): External concerns (DB, APIs)
- **API Layer** (`Cambrian.Api`): REST API endpoints and controllers

## 🗄️ Database

Start PostgreSQL with Docker:

```bash
docker compose -f docker/docker-compose.yml up -d
```

Default connection:
- Host: localhost
- Port: 5432
- Database: cambrian
- User: cambrian
- Password: cambrian

## 📖 Documentation

- [API Reference](docs/API_REFERENCE.md) - Complete API endpoint documentation
- [Deployment Guide](docs/DEPLOYMENT.md) - AWS, Render, and Docker deployment
- [Configuration](docs/CONFIGURATION_GUIDE.md) - Environment variables and settings
- [Troubleshooting](docs/TROUBLESHOOTING.md) - Common issues and solutions
- [Audio Player Support](docs/AUDIO_PLAYER_BACKEND_SUPPORT.md) - Audio streaming features
- [Testing Layers](docs/TESTING_LAYERS.md) - Testing strategy and scripts

## 🔧 Development Scripts

### Health Checks
```bash
# Run health check
./scripts/health-check.sh

# Test API endpoints
./scripts/test-api-endpoints.ps1
```

### Integration Tests
```bash
# Run integration tests
./scripts/integration-test.sh dev $JWT_TOKEN
```

### Infrastructure Validation
```bash
# Validate infrastructure
./scripts/validate-infra.sh dev
```

## 🐳 Docker

### Local Development
```bash
# Start all services
docker compose -f docker/docker-compose.yml up

# Build API image
docker build -f docker/Dockerfile -t cambrian-api .

# Run API container
docker run -p 3000:3000 cambrian-api
```

## 🚀 Deployment

### Render.com (Recommended)
See [Render Setup Guide](docs/RENDER_SETUP.md)

### AWS Fargate
See [Deployment Guide](docs/DEPLOYMENT.md)

### Environment Variables

Required for production:

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:3000
CORS_ORIGINS=https://your-app.vercel.app
CONNECTION_STRING=Host=...;Database=...;Username=...;Password=...
STRIPE_SECRET_KEY=sk_live_...
STRIPE_LISTENER_PRICE_ID=price_...
STRIPE_CREATOR_PRICE_ID=price_...
STRIPE_SUCCESS_URL=https://your-app.vercel.app/account?status=success
STRIPE_CANCEL_URL=https://your-app.vercel.app/account?status=cancel
STRIPE_WEBHOOK_SECRET=whsec_...
```

See [Configuration Guide](docs/CONFIGURATION_GUIDE.md) for details.

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/Cambrian.Api.Tests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## 🔄 API Endpoints

### Authentication
- `POST /auth/register` - Register new user
- `POST /auth/login` - Login user
- `POST /auth/password` - Change password
- `GET /auth/health` - Health check

### Catalog & Discovery
- `GET /catalog` - Get music catalog
- `GET /discover` - Discover new tracks
- `POST /tracks/upload` - Upload track

### Subscriptions & Billing
- `GET /subscriptions/current` - Current subscription
- `GET /subscriptions/plans` - Available plans
- `POST /billing/checkout-session` - Create Stripe checkout
- `GET /billing/invoices` - Invoice history

### Streaming & Playback
- `POST /stream/start` - Start stream
- `POST /stream/stop` - Stop stream
- `POST /playback/request` - Request playback
- `POST /play/events` - Log play events

See [API Reference](docs/API_REFERENCE.md) for complete documentation.

## 🛠️ Technology Stack

- **Runtime**: .NET 8.0
- **Database**: PostgreSQL 15
- **Payment**: Stripe
- **Deployment**: Docker, Render, AWS Fargate
- **Testing**: xUnit
- **ORM**: Npgsql (PostgreSQL driver)

## 🤝 Contributing

1. Create a feature branch
2. Make your changes
3. Run tests: `dotnet test`
4. Submit a pull request

## 📝 License

See [LICENSE](LICENSE) file for details.

## 🆘 Support

- [Troubleshooting Guide](docs/TROUBLESHOOTING.md)
- [GitHub Issues](https://github.com/loganbryanx/Cambrian-api/issues)

## 🔗 Related Projects

- **Frontend**: [Cambrian Web App](https://cambrian-blush.vercel.app)
- **Deployment**: [Render Dashboard](https://dashboard.render.com)

---

**Production API**: https://cambrian-api.onrender.com  
**Health Check**: https://cambrian-api.onrender.com/auth/health
