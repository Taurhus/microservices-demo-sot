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

---
This project is for demonstration and educational purposes only.
