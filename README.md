# Sea of Thieves Microservices Demo

A complete microservices-based architecture demonstration inspired by the Sea of Thieves game. This solution showcases modern software architecture patterns using 12 independent microservices, an API Gateway, event-driven messaging, and containerized deployment.

## 📚 Documentation Index

- **[Getting Started Guide](docs/GETTING_STARTED.md)** - Complete installation and setup instructions from scratch
- **[User Guide](docs/USER_GUIDE.md)** - How to use the system and interact with services
- **[Architecture Overview](docs/ARCHITECTURE.md)** - Technical architecture and design patterns
- **[Testing Guide](docs/TESTING.md)** - Test coverage, running tests, and test strategies
- **[Demonstration Guide](docs/DEMONSTRATIONS.md)** - Step-by-step demonstrations
- **[Troubleshooting Guide](docs/TROUBLESHOOTING.md)** - Common issues and solutions
- **[Documentation Index](docs/DOCUMENTATION_INDEX.md)** - Complete guide to all documentation

## 🚀 Quick Start

If you already have Docker Desktop installed:

```powershell
# 1. Clone or download this repository
# 2. Open PowerShell in the repository folder
# 3. Start all services
docker-compose up -d

# 4. Wait for services to start (about 1-2 minutes)
# 5. Access the API Gateway at http://localhost:5000
```

**For complete setup instructions, see [Getting Started Guide](docs/GETTING_STARTED.md)**

## 🏗️ Architecture Overview

### Services

This solution consists of **12 microservices**, each handling a specific domain:

| Service | Port | Description |
|---------|------|-------------|
| **API Gateway** | 5000 | Single entry point for all API requests |
| Player Service | 5001 | Manages player information and profiles |
| Ship Service | 5002 | Handles ship types and configurations |
| Quest Service | 5003 | Manages quests and voyages |
| Faction Service | 5004 | Trading companies and factions |
| Event Service | 5005 | World events and activities |
| Item Service | 5006 | Items, treasure, and resources |
| Location Service | 5007 | Islands, outposts, and locations |
| Shop Service | 5008 | Shops and merchants |
| Emissary Service | 5009 | Emissary flags and bonuses |
| Reputation Service | 5010 | Player reputation with factions |
| Crew Service | 5011 | Crew management |
| Achievement Service | 5012 | Player achievements |

### Infrastructure

- **API Gateway** (Ocelot) - Routes requests to appropriate services
- **RabbitMQ** - Message broker for event-driven communication
- **Azure SQL Edge** - Database for all services
- **Event Consumer** - Processes events from all services

## 🎯 Key Features

- ✅ **Microservices Architecture** - 12 independent, scalable services
- ✅ **API Gateway** - Unified access point with routing
- ✅ **Event-Driven Communication** - Services communicate via events
- ✅ **Transactional Outbox Pattern** - Guaranteed atomicity between database and events
- ✅ **Containerized** - Everything runs in Docker containers
- ✅ **Health Checks** - Automatic monitoring of service health
- ✅ **Distributed Tracing** - OpenTelemetry for observability
- ✅ **Circuit Breaker** - Resilience patterns for reliability
- ✅ **Comprehensive Testing** - 120 integration tests (60 positive + 60 negative)

## 📖 What You Can Do

1. **Explore the API** - Use the API Gateway to interact with all services
2. **View Events** - Watch real-time event processing
3. **Run Tests** - Execute automated integration tests
4. **Monitor Services** - Check health and status of all services

## 🔗 Access Points

- **API Gateway**: http://localhost:5000
- **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
- **Individual Services**: http://localhost:5001-5012 (for direct access)

## 📝 Example API Calls

### Get All Players
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get
```

### Create a Player
```powershell
$player = @{
    Name = "Captain Jack"
    Gamertag = "CaptainJack"
    Gold = 5000
    Renown = 50
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
```

## 🧪 Testing

The solution includes comprehensive integration tests:

- **Total Tests**: 120 (60 positive + 60 negative)
- **Test Coverage**: All 12 services
- **Test Categories**:
  - Happy path tests (successful operations)
  - Negative tests (error handling)
  - Integration tests (end-to-end through API Gateway)

**Run Tests**:
```powershell
dotnet test src/MicroservicesDemoSot.Tests/MicroservicesDemoSot.Tests.csproj
```

See [Testing Guide](docs/TESTING.md) for detailed test coverage and strategies.

## 🛠️ Requirements

- **Windows 10/11** or **Windows Server**
- **Docker Desktop** (see [Getting Started Guide](docs/GETTING_STARTED.md) for installation)
- **PowerShell** (included with Windows)
- **.NET 8.0 SDK** (for running tests)
- **8GB RAM minimum** (16GB recommended)
- **10GB free disk space**

## 📚 Next Steps

1. **New to this?** → Start with [Getting Started Guide](docs/GETTING_STARTED.md)
2. **Want to use it?** → Read [User Guide](docs/USER_GUIDE.md)
3. **Want to see demos?** → Check [Demonstration Guide](docs/DEMONSTRATIONS.md)
4. **Having issues?** → See [Troubleshooting Guide](docs/TROUBLESHOOTING.md)

## 🎓 Learning Resources

This project demonstrates:
- Microservices architecture patterns
- API Gateway pattern
- Event-driven architecture
- Container orchestration
- RESTful API design
- Database per service pattern
- Distributed systems concepts

## ⚠️ Important Notes

- This is a **demonstration and educational project**
- Services use development settings (not production-ready)
- Data is stored in containers (will be lost when containers are removed)
- Requires Docker Desktop to be running

## 📞 Support

For issues or questions:
1. Check the [Troubleshooting Guide](docs/TROUBLESHOOTING.md)
2. Review service logs: `docker-compose logs [service-name]`
3. Verify Docker Desktop is running
4. Ensure ports 5000-5012 and 15672 are not in use

---

**Happy Sailing! ⚓**
