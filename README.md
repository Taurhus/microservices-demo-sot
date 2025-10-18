# Microservices Demo: Sea of Thieves Architecture

This solution demonstrates a modern microservices-based architecture inspired by the "Sea of Thieves" game structure. It uses C# for APIs, Azure SQL/NoSQL for data storage, and RabbitMQ or Azure Service Bus for event-based messaging. The architecture is designed for clear business/data domain separation and event-driven communication.

## Architecture Overview

**Domains as Microservices:**
- Player Service
- Ship Service
- Quest Service
- Faction Service
- Event Service
- Item Service
- Location Service
- Shop Service

**Core Features:**
- Each domain is a separate C# API (ASP.NET Core)
- Event-based messaging via RabbitMQ or Azure Service Bus
- Azure SQL/NoSQL for persistent storage
- Containerized with Docker
- API Gateway for unified access

## Getting Started

1. Each service is in its own folder with its own solution/project.
2. Messaging and database configuration are per-service.
3. Use Docker Compose to orchestrate services, message broker, and databases.

## References
- [Model Context Protocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [MCP Documentation](https://modelcontextprotocol.io/)
- [Sea of Thieves Wiki](https://seaofthieves.wiki.gg/)

## Run Instructions
- See each service's README for local run instructions.
- Use `docker-compose up` to start the full stack.

## Running the integration tests

1. Start the Docker Compose stack from the repository root. This will launch the database, message broker, and all services:

```powershell
docker-compose up --build -d
```

2. The test project expects each service to be reachable on specific host ports mapped by the compose file (the defaults used by the tests are listed below). Tests include a startup wait helper that probes these ports and will fail early with a helpful message if services are not available within 30s.

Default ports used by the tests (host):

- Player: 5001
- Ship: 5002
- Quest: 5003
- Faction: 5004
- Event: 5005
- Item: 5006
- Location: 5007
- Shop: 5008

3. Run the tests from the repo root:

```powershell
dotnet test src/MicroservicesDemoSot.Tests/MicroservicesDemoSot.Tests.csproj --logger "console;verbosity=detailed"
```

If a test run fails because a service did not start in time, check container logs and re-run after services report they are "Application started".

4. Optional: If you want to re-run only the integration tests quickly while developing, you can use the `--filter` flag with `dotnet test` to run a subset.

Environment variables that control the test startup wait fixture

- `SERVICE_WAIT_TIMEOUT_SECONDS` — number of seconds to wait per service before giving up (default: 30).
- `SERVICE_PORTS` — comma-separated list of host:port entries to probe (default: `127.0.0.1:5001,127.0.0.1:5002,127.0.0.1:5003,127.0.0.1:5004,127.0.0.1:5005,127.0.0.1:5006,127.0.0.1:5007,127.0.0.1:5008`).

Example (PowerShell):

```powershell
$env:SERVICE_WAIT_TIMEOUT_SECONDS = "60"
$env:SERVICE_PORTS = "127.0.0.1:5001,127.0.0.1:5002,127.0.0.1:5003"
dotnet test src/MicroservicesDemoSot.Tests/MicroservicesDemoSot.Tests.csproj
```


---
This project is for demonstration and educational purposes only.
