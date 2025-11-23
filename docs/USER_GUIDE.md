# User Guide

This guide explains how to use the Sea of Thieves Microservices Demo once it's running.

## 📚 Table of Contents

1. [Understanding the System](#understanding-the-system)
2. [Accessing the API](#accessing-the-api)
3. [Making API Calls](#making-api-calls)
4. [Viewing Events](#viewing-events)
5. [Monitoring Services](#monitoring-services)
6. [Common Tasks](#common-tasks)

---

## Understanding the System

### What is This?

This is a **microservices demonstration** that simulates a game backend system. It consists of:

- **12 Microservices** - Each handles a specific type of data (players, ships, quests, etc.)
- **API Gateway** - A single entry point that routes requests to the right service
- **Event System** - Services communicate by sending events
- **Database** - Each service has its own database

### Key Concepts

**API Gateway (Port 5000)**
- This is your main entry point
- All requests go through here
- It automatically routes to the correct service

**Services (Ports 5001-5012)**
- Each service handles one type of data
- You can access them directly, but it's easier to use the API Gateway

**Events**
- When you create, update, or delete something, an event is sent
- Other services can react to these events
- You can watch events in real-time

---

## Accessing the API

### Method 1: Web Browser (Read-Only)

You can view data directly in your browser:

**Get All Players:**
```
http://localhost:5000/api/players
```

**Get a Specific Player:**
```
http://localhost:5000/api/players/1
```

**Get All Ships:**
```
http://localhost:5000/api/ships
```

**Try These URLs:**
- Players: `http://localhost:5000/api/players`
- Ships: `http://localhost:5000/api/ships`
- Quests: `http://localhost:5000/api/quests`
- Factions: `http://localhost:5000/api/factions`
- Events: `http://localhost:5000/api/events`
- Items: `http://localhost:5000/api/items`
- Locations: `http://localhost:5000/api/locations`
- Shops: `http://localhost:5000/api/shops`
- Emissaries: `http://localhost:5000/api/emissaries`
- Reputations: `http://localhost:5000/api/reputations`
- Crews: `http://localhost:5000/api/crews`
- Achievements: `http://localhost:5000/api/achievements`

### Method 2: PowerShell (Full Access)

PowerShell allows you to create, update, and delete data. See [Making API Calls](#making-api-calls) below.

---

## Making API Calls

### Prerequisites

- Services must be running (see [Getting Started Guide](GETTING_STARTED.md))
- PowerShell (included with Windows)

### Basic Operations

#### 1. Get All Items (GET)

**Get all players:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get
```

**Get a specific player:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/players/1" -Method Get
```

#### 2. Create an Item (POST)

**Create a new player:**
```powershell
$newPlayer = @{
    Name = "Captain Sparrow"
    Gamertag = "Sparrow123"
    Gold = 10000
    Renown = 75
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $newPlayer -ContentType "application/json"
```

**Create a new ship:**
```powershell
$newShip = @{
    Name = "Black Pearl"
    Type = "Galleon"
    Description = "A legendary galleon"
    MaxCrewSize = 4
    CannonCount = 8
    MastCount = 3
    IsActive = $true
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Post -Body $newShip -ContentType "application/json"
```

#### 3. Update an Item (PUT)

**Update a player:**
```powershell
$updatedPlayer = @{
    Id = 1
    Name = "Updated Name"
    Gamertag = "NewGamertag"
    Gold = 20000
    Renown = 100
    IsPirateLegend = $true
    Platform = "Xbox"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/players/1" -Method Put -Body $updatedPlayer -ContentType "application/json"
```

#### 4. Delete an Item (DELETE)

**Delete a player:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/players/1" -Method Delete
```

### Complete Examples

#### Example 1: Create and View a Player

```powershell
# Create a player
$player = @{
    Name = "Test Player"
    Gamertag = "TestPlayer123"
    Gold = 5000
    Renown = 25
    IsPirateLegend = $false
    Platform = "Steam"
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"

# View the created player
Write-Host "Created Player ID: $($created.id)"
Write-Host "Name: $($created.name)"
```

#### Example 2: Get All Ships and Display

```powershell
$ships = Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Get

foreach ($ship in $ships) {
    Write-Host "Ship: $($ship.name) - Type: $($ship.type)"
}
```

---

## Viewing Events

When you create, update, or delete data, events are automatically sent. Here's how to watch them:

### Method 1: View Event Consumer Logs

```powershell
docker-compose logs -f event-consumer
```

This shows events in real-time. Press `Ctrl+C` to stop.

### Method 2: RabbitMQ Management UI

1. Open browser: `http://localhost:15672`
2. Login: `guest` / `guest`
3. Go to **Queues** → `event-consumer-queue`
4. Click **"Get messages"** to see recent events

### Method 3: Trigger Events and Watch

1. Open two PowerShell windows
2. In the first window, start watching logs:
   ```powershell
   docker-compose logs -f event-consumer
   ```
3. In the second window, create something:
   ```powershell
   $player = @{ Name = "Event Test"; Gamertag = "EventTest"; Gold = 100; Renown = 1; IsPirateLegend = $false; Platform = "PC" } | ConvertTo-Json
   Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
   ```
4. Watch the first window - you should see an event appear!

---

## Monitoring Services

### Check Service Status

**View all services:**
```powershell
docker-compose ps
```

**Check a specific service:**
```powershell
docker-compose ps player-service
```

### View Service Logs

**View logs for a service:**
```powershell
docker-compose logs player-service
```

**View last 50 lines:**
```powershell
docker-compose logs --tail=50 player-service
```

**Follow logs in real-time:**
```powershell
docker-compose logs -f player-service
```

### Check Service Health

**Check API Gateway health:**
```powershell
curl http://localhost:5000/health
```

**Check individual service health:**
```powershell
curl http://localhost:5001/health
curl http://localhost:5002/health
# etc.
```

---

## Common Tasks

### Task 1: Create a Complete Player Profile

```powershell
# 1. Create a player
$player = @{
    Name = "Captain Hook"
    Gamertag = "Hook123"
    Gold = 15000
    Renown = 50
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

$createdPlayer = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
$playerId = $createdPlayer.id

# 2. Create a reputation for the player
$reputation = @{
    PlayerId = $playerId
    FactionName = "Gold Hoarders"
    Level = 10
    TotalReputation = 5000
    Notes = "New member"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/reputations" -Method Post -Body $reputation -ContentType "application/json"

# 3. Create an achievement
$achievement = @{
    PlayerId = $playerId
    Name = "First Voyage"
    Description = "Completed first quest"
    Category = "Quest"
    Rarity = "Common"
    Progress = 100
    RequiredProgress = 100
    Notes = "Completed"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/achievements" -Method Post -Body $achievement -ContentType "application/json"

Write-Host "Created complete profile for player ID: $playerId"
```

### Task 2: Find All Players with High Gold

```powershell
$players = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get

$richPlayers = $players | Where-Object { $_.gold -gt 10000 }

Write-Host "Players with more than 10,000 gold:"
foreach ($player in $richPlayers) {
    Write-Host "  $($player.name): $($player.gold) gold"
}
```

### Task 3: Update Multiple Items

```powershell
# Get all players
$players = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get

# Update each player's gold
foreach ($player in $players) {
    $updated = @{
        Id = $player.id
        Name = $player.name
        Gamertag = $player.gamertag
        Gold = $player.gold + 1000
        Renown = $player.renown
        IsPirateLegend = $player.isPirateLegend
        Platform = $player.platform
    } | ConvertTo-Json
    
    Invoke-RestMethod -Uri "http://localhost:5000/api/players/$($player.id)" -Method Put -Body $updated -ContentType "application/json"
    Write-Host "Updated $($player.name)"
}
```

### Task 4: Clean Up Test Data

```powershell
# Get all players
$players = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get

# Delete players created for testing (adjust condition as needed)
foreach ($player in $players) {
    if ($player.name -like "*Test*" -or $player.name -like "*Demo*") {
        Invoke-RestMethod -Uri "http://localhost:5000/api/players/$($player.id)" -Method Delete
        Write-Host "Deleted: $($player.name)"
    }
}
```

---

## API Endpoints Reference

### Players
- `GET /api/players` - Get all players
- `GET /api/players/{id}` - Get player by ID
- `POST /api/players` - Create a player
- `PUT /api/players/{id}` - Update a player
- `DELETE /api/players/{id}` - Delete a player

### Ships
- `GET /api/ships` - Get all ships
- `GET /api/ships/{id}` - Get ship by ID
- `POST /api/ships` - Create a ship
- `PUT /api/ships/{id}` - Update a ship
- `DELETE /api/ships/{id}` - Delete a ship

*(Similar patterns for all other services: quests, factions, events, items, locations, shops, emissaries, reputations, crews, achievements)*

---

## Tips and Best Practices

1. **Always use the API Gateway** (port 5000) instead of individual services
2. **Check service status** before making requests if something fails
3. **Watch event logs** to understand what's happening behind the scenes
4. **Use PowerShell variables** to store responses for easier manipulation
5. **Handle errors** - wrap API calls in try-catch blocks

### Error Handling Example

```powershell
try {
    $result = Invoke-RestMethod -Uri "http://localhost:5000/api/players/999" -Method Get
    Write-Host "Success: $($result.name)"
} catch {
    Write-Host "Error: Player not found or service unavailable"
}
```

---

## Testing the System

### Running Integration Tests

The solution includes 120 comprehensive integration tests. To run them:

```powershell
# Ensure services are running
docker-compose up -d

# Wait for services to be healthy
docker-compose ps

# Run all tests
dotnet test src/MicroservicesDemoSot.Tests/MicroservicesDemoSot.Tests.csproj
```

### Test Coverage

- **120 Total Tests**: 60 positive (success scenarios) + 60 negative (error scenarios)
- **All Services Tested**: Every microservice has comprehensive test coverage
- **End-to-End Validation**: Tests run through API Gateway for complete validation

For detailed testing information, see [Testing Guide](TESTING.md).

---

**Ready to explore? Check out the [Demonstration Guide](DEMONSTRATIONS.md) for step-by-step examples!** ⚓

