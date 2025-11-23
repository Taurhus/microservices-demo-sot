# Architecture Overview

This document explains the technical architecture of the Sea of Thieves Microservices Demo.

## 📚 Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Microservices](#microservices)
3. [Infrastructure Components](#infrastructure-components)
4. [Communication Patterns](#communication-patterns)
5. [Data Architecture](#data-architecture)
6. [Design Patterns](#design-patterns)
7. [Testing Architecture](#testing-architecture)

---

## Architecture Overview

### High-Level Architecture

```
┌────────────────────────────────────────────────────────────┐
│                   Client Applications                      │
│            (Web Browser, PowerShell, etc.)                 │
└───────────────────────────┬────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────────┐
│                    API Gateway (Ocelot)                    │
│                         Port: 5000                         │
│                  • Request Routing                         │
│                  • Load Balancing                          │
│                  • Health Checks                           │
└───────────────────────────┬────────────────────────────────┘
                            │
              ┌─────────────┼──────────────┐
              │             │              │
              ▼             ▼              ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │   Player     │ │    Ship      │ │    Quest     │
    │   Service    │ │   Service    │ │   Service    │
    │   :5001      │ │   :5002      │ │   :5003      │
    └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
           │                │                │
           └────────────────┼────────────────┘
                            │
              ┌─────────────┼──────────────┐
              │             │              │
              ▼             ▼              ▼
    ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │  RabbitMQ    │ │  Azure SQL   │ │    Event     │
    │  Message     │ │    Edge      │ │   Consumer   │
    │   Broker     │ │   Database   │ │   Service    │
    │   :5672      │ │   :1433      │ │              │
    └──────────────┘ └──────────────┘ └──────────────┘
```

### Key Principles

1. **Microservices Architecture**: Each service is independent and handles one domain
2. **API Gateway Pattern**: Single entry point for all client requests
3. **Event-Driven Communication**: Services communicate asynchronously via events
4. **Database per Service**: Each service has its own database
5. **Containerization**: All services run in Docker containers

---

## Microservices

### Service List

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| **API Gateway** | 5000 | - | Routes requests to services |
| **Player Service** | 5001 | PlayerDb | Player profiles and information |
| **Ship Service** | 5002 | ShipDb | Ship types and configurations |
| **Quest Service** | 5003 | QuestDb | Quests and voyages |
| **Faction Service** | 5004 | FactionDb | Trading companies |
| **Event Service** | 5005 | EventDb | World events |
| **Item Service** | 5006 | ItemDb | Items and treasure |
| **Location Service** | 5007 | LocationDb | Islands and locations |
| **Shop Service** | 5008 | ShopDb | Shops and merchants |
| **Emissary Service** | 5009 | EmissaryDb | Emissary flags |
| **Reputation Service** | 5010 | ReputationDb | Player reputation |
| **Crew Service** | 5011 | CrewDb | Crew management |
| **Achievement Service** | 5012 | AchievementDb | Player achievements |
| **Event Consumer** | - | - | Processes events |

### Service Responsibilities

Each microservice follows these principles:

- **Single Responsibility**: Each service handles one business domain
- **Independent Deployment**: Services can be updated independently
- **Own Database**: Each service has its own database schema
- **API Endpoints**: RESTful API with standard CRUD operations
- **Event Publishing**: Publishes events on data changes

---

## Infrastructure Components

### API Gateway (Ocelot)

**Purpose**: Single entry point for all API requests

**Features**:
- Request routing to appropriate services
- Load balancing (future enhancement)
- Health check aggregation
- CORS configuration
- Request/response transformation

**Configuration**: `src/ApiGateway/ocelot.json`

### RabbitMQ

**Purpose**: Message broker for event-driven communication

**Features**:
- Topic-based exchange (`seaofthieves.events`)
- Durable queues
- Message persistence
- Management UI (port 15672)

**Usage**:
- Services publish events when data changes
- Event Consumer subscribes to all events
- Events are routed by type (e.g., `player.created`, `ship.updated`)

### Azure SQL Edge

**Purpose**: Database for all microservices

**Features**:
- SQL Server compatible
- Containerized
- Multiple databases (one per service)
- Automatic initialization

**Database Naming**:
- Each service has its own database (e.g., `PlayerDb`, `ShipDb`)

### Event Consumer Service

**Purpose**: Process events from all services

**Features**:
- Subscribes to all event types
- Logs events for monitoring
- Can be extended for business logic
- Demonstrates event-driven patterns

---

## Communication Patterns

### Synchronous Communication (Request-Response)

**Pattern**: REST API calls through API Gateway

```
Client → API Gateway → Microservice → Database
         ↓
    Response
```

**Use Cases**:
- Getting data (GET requests)
- Creating data (POST requests)
- Updating data (PUT requests)
- Deleting data (DELETE requests)

### Asynchronous Communication (Events)

**Pattern**: Event-driven messaging via RabbitMQ with Transactional Outbox Pattern

```
Microservice → Database Transaction → Outbox Table → RabbitMQ Exchange → Event Consumer
                ↓ (atomic)                              ↓
          Business Data + Event                    Other Services (future)
```

**Event Flow (Transactional Outbox Pattern)**:
1. Service begins database transaction
2. Service saves business entity (e.g., Player, Ship, Quest)
3. Service saves event to OutboxEvents table (same transaction)
4. Transaction commits atomically (both business data and event)
5. Background process reads from outbox and publishes to RabbitMQ
6. Event Consumer receives and processes event
7. Other services can subscribe (future enhancement)

**Key Benefits**:
- **Atomicity**: Database and event saved together or both rolled back
- **Reliability**: Events persist in database even if RabbitMQ is down
- **Consistency**: No partial states where data exists but event doesn't
- **Resilience**: Events can be retried if publishing fails

**Event Types**:
- `{entity}.created` - When new entity is created
- `{entity}.updated` - When entity is updated
- `{entity}.deleted` - When entity is deleted

**Example Events**:
- `player.created`
- `ship.updated`
- `achievement.deleted`

---

## Data Architecture

### Database per Service Pattern

Each microservice has its own database:

```
Player Service     → PlayerDb
Ship Service       → ShipDb
Quest Service      → QuestDb
Faction Service    → FactionDb
Event Service      → EventDb
Item Service       → ItemDb
Location Service   → LocationDb
Shop Service       → ShopDb
Emissary Service   → EmissaryDb
Reputation Service → ReputationDb
Crew Service       → CrewDb
Achievement Service → AchievementDb
```

### Benefits

- **Independence**: Services can use different database technologies
- **Scalability**: Each database can be scaled independently
- **Isolation**: Failures in one database don't affect others
- **Team Autonomy**: Teams can work independently

### Data Relationships

Services maintain relationships through:
- **Foreign Key References**: IDs stored in other services
- **Event Communication**: Services notify each other of changes
- **Eventual Consistency**: Data may be temporarily inconsistent

**Example**:
- `Reputation` service stores `PlayerId` (references Player service)
- When a player is deleted, events notify other services
- Services can react to events to maintain consistency

### Outbox Pattern Implementation

Each service database includes an `OutboxEvents` table:

**OutboxEvent Schema**:
- `Id` (Guid) - Unique identifier
- `Exchange` (string) - RabbitMQ exchange name
- `RoutingKey` (string) - Event routing key
- `MessageBody` (string) - Serialized event data
- `CreatedAt` (DateTime) - When event was created
- `PublishedAt` (DateTime?) - When event was published
- `RetryCount` (int) - Number of retry attempts
- `ErrorMessage` (string?) - Error message if publishing failed

**Transaction Flow**:
```
BEGIN TRANSACTION
  INSERT INTO BusinessTable (...) VALUES (...)
  INSERT INTO OutboxEvents (...) VALUES (...)
COMMIT TRANSACTION
```

This ensures both operations succeed or both fail together.

---

## Design Patterns

### 1. API Gateway Pattern

**Problem**: Clients need to know about multiple services

**Solution**: Single entry point that routes requests

**Benefits**:
- Simplified client code
- Centralized authentication (future)
- Request aggregation
- Protocol translation

### 2. Circuit Breaker Pattern

**Implementation**: Polly library in Shared.Infrastructure

**Purpose**: Prevent cascading failures

**Behavior**:
- Opens circuit after 5 failures
- Stays open for 30 seconds
- Allows retry after cooldown

### 3. Event Sourcing (Partial)

**Implementation**: Events published for all data changes

**Purpose**: Track all changes to data

**Benefits**:
- Audit trail
- Event replay capability
- Decoupled services

### 4. CQRS (Command Query Responsibility Segregation)

**Implementation**: Separate read and write operations

**Commands**: POST, PUT, DELETE (write operations)
**Queries**: GET (read operations)

### 5. Health Check Pattern

**Implementation**: Built into each service

**Purpose**: Monitor service availability

**Endpoints**: `/health` on each service

### 6. Transactional Outbox Pattern

**Implementation**: OutboxEvents table in each service database

**Purpose**: Guarantee atomicity between database operations and event publishing

**How It Works**:
1. Business operation and event are saved in same database transaction
2. Event stored in `OutboxEvents` table with business data
3. Transaction commits atomically (both succeed or both fail)
4. Background process reads from outbox and publishes to RabbitMQ
5. Published events marked as processed in outbox

**Benefits**:
- **Atomicity**: Database and event always in sync
- **No Lost Events**: Events persist even if RabbitMQ is unavailable
- **Reliable**: Events can be retried if publishing fails
- **Consistent**: No partial states

**Components**:
- `OutboxEvent` entity in each service database
- `ITransactionalMessagePublisher` interface
- `TransactionalMessagePublisher` implementation
- Database transactions wrapping all CRUD operations

---

## Technology Stack

### Backend
- **.NET 8.0** - Framework for all services
- **ASP.NET Core** - Web API framework
- **Entity Framework Core 9.0** - ORM for database access
- **Ocelot** - API Gateway
- **RabbitMQ.Client** - Message broker client

### Infrastructure
- **Docker** - Containerization
- **Docker Compose** - Orchestration
- **Azure SQL Edge** - Database
- **RabbitMQ** - Message broker

### Observability
- **OpenTelemetry** - Distributed tracing
- **Health Checks** - Service monitoring
- **Structured Logging** - Application logging

### Resilience
- **Polly** - Circuit breaker and retry policies
- **HttpClient** - HTTP client with resilience

---

## Scalability Considerations

### Horizontal Scaling

Each service can be scaled independently:

```yaml
# Example: Scale player service
docker-compose up -d --scale player-service=3
```

### Load Balancing

API Gateway can distribute load across multiple instances.

### Database Scaling

Each database can be scaled independently based on service needs.

---

## Security Considerations

**Current State**: Development mode (no authentication)

**Production Considerations**:
- API Gateway authentication
- Service-to-service authentication
- Encrypted connections (HTTPS)
- Input validation
- Rate limiting

---

## Future Enhancements

1. **Service Mesh**: For advanced routing and security
2. **Distributed Tracing**: Full request tracing across services
3. **Metrics Collection**: Prometheus integration
4. **Authentication**: OAuth2/JWT implementation
5. **API Versioning**: Version management for APIs
6. **Caching**: Redis for performance
7. **Service Discovery**: Automatic service registration

---

## Architecture Diagrams

### Request Flow

```
1. Client Request
   ↓
2. API Gateway (routing)
   ↓
3. Target Microservice
   ↓
4. Database Query
   ↓
5. Response
   ↓
6. Event Published (if write operation)
   ↓
7. Event Consumer processes event
```

### Event Flow (With Outbox Pattern)

```
1. Data Change in Service
   ↓
2. Begin Database Transaction
   ↓
3. Save Business Entity + Save Event to Outbox (atomic)
   ↓
4. Commit Transaction (both succeed or both fail)
   ↓
5. Background Process Reads from Outbox
   ↓
6. Event Published to RabbitMQ
   ↓
7. RabbitMQ Routes to Queue
   ↓
8. Event Consumer Receives Event
   ↓
9. Event Processed/Logged
   ↓
10. (Future: Other services react)
```

---

## Testing Architecture

### Test Strategy

The solution includes comprehensive integration tests:

- **Total Tests**: 120 (60 positive + 60 negative)
- **Coverage**: All 12 microservices
- **Approach**: End-to-end tests through API Gateway
- **Validation**: Both success and error scenarios

### Test Categories

1. **Positive Tests** (60 tests)
   - Successful CRUD operations
   - Data validation
   - Event publishing verification

2. **Negative Tests** (60 tests)
   - Error handling validation
   - Invalid request rejection
   - Proper HTTP status codes

### Test Execution

- Tests run against actual running services
- All requests route through API Gateway
- Tests validate complete request/response cycle
- Atomicity pattern verified through successful operations

For detailed testing information, see [Testing Guide](TESTING.md).

---

**This architecture demonstrates modern microservices patterns suitable for production systems!** ⚓

