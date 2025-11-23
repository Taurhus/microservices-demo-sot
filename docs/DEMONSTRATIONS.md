# Demonstration Guide

Step-by-step demonstrations to showcase the microservices architecture and features.

## 📚 Table of Contents

1. [Demonstration 1: Basic API Operations](#demonstration-1-basic-api-operations)
2. [Demonstration 2: Event-Driven Architecture](#demonstration-2-event-driven-architecture)
3. [Demonstration 3: API Gateway Routing](#demonstration-3-api-gateway-routing)
4. [Demonstration 4: Complete Workflow](#demonstration-4-complete-workflow)
5. [Demonstration 5: Monitoring and Health Checks](#demonstration-5-monitoring-and-health-checks)

---

## Prerequisites

Before starting, ensure:
- ✅ All services are running (`docker-compose ps` shows services as "Up")
- ✅ API Gateway is accessible at http://localhost:5000
- ✅ PowerShell is open in the project directory

---

## Demonstration 1: Basic API Operations

**Goal**: Show how to create, read, update, and delete data through the API Gateway.

### Step 1: View Existing Data

```powershell
# Get all players
Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get

# Get all ships
Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Get
```

**Expected Result**: You should see JSON arrays (may be empty or contain sample data)

### Step 2: Create a New Player

```powershell
$newPlayer = @{
    Name = "Demo Captain"
    Gamertag = "DemoCap123"
    Gold = 5000
    Renown = 25
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

$created = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $newPlayer -ContentType "application/json"

Write-Host "Created Player:"
Write-Host "  ID: $($created.id)"
Write-Host "  Name: $($created.name)"
Write-Host "  Gold: $($created.gold)"
```

**Expected Result**: A new player object with an ID

### Step 3: Retrieve the Created Player

```powershell
$playerId = $created.id
$retrieved = Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Get

Write-Host "Retrieved Player: $($retrieved.name)"
```

### Step 4: Update the Player

```powershell
$updated = @{
    Id = $playerId
    Name = "Updated Captain"
    Gamertag = "UpdatedCap123"
    Gold = 10000
    Renown = 50
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Put -Body $updated -ContentType "application/json"

Write-Host "Player updated successfully"
```

### Step 5: Verify the Update

```powershell
$verify = Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Get
Write-Host "Updated Name: $($verify.name)"
Write-Host "Updated Gold: $($verify.gold)"
```

### Step 6: Delete the Player

```powershell
Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Delete
Write-Host "Player deleted"

# Verify deletion
try {
    Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Get
} catch {
    Write-Host "Confirmed: Player no longer exists"
}
```

**Summary**: You've demonstrated the full CRUD (Create, Read, Update, Delete) cycle!

---

## Demonstration 2: Event-Driven Architecture

**Goal**: Show how events are automatically published when data changes.

### Step 1: Start Watching Events

Open a **second PowerShell window** and run:

```powershell
docker-compose logs -f event-consumer
```

This will show events in real-time. **Keep this window open**.

### Step 2: Create Data to Trigger Events

In your **first PowerShell window**, create several items:

```powershell
# Create a player (triggers player.created event)
$player = @{
    Name = "Event Test Player"
    Gamertag = "EventTest"
    Gold = 1000
    Renown = 10
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

$createdPlayer = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
$playerId = $createdPlayer.id

Start-Sleep -Seconds 2

# Create a ship (triggers ship.created event)
$ship = @{
    Name = "Event Test Ship"
    Type = "Sloop"
    Description = "Test ship for events"
    MaxCrewSize = 2
    CannonCount = 2
    MastCount = 1
    IsActive = $true
} | ConvertTo-Json

$createdShip = Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Post -Body $ship -ContentType "application/json"
$shipId = $createdShip.id

Start-Sleep -Seconds 2

# Update the player (triggers player.updated event)
$update = @{
    Id = $playerId
    Name = "Updated Event Player"
    Gamertag = "EventTest"
    Gold = 2000
    Renown = 20
    IsPirateLegend = $false
    Platform = "PC"
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Put -Body $update -ContentType "application/json"

Start-Sleep -Seconds 2

# Delete the ship (triggers ship.deleted event)
Invoke-RestMethod -Uri "http://localhost:5000/api/ships/$shipId" -Method Delete
```

### Step 3: Observe Events

**Look at the second PowerShell window** - you should see events appearing:

```
Received event: player.created | EventType: player | Action: created
Received event: ship.created | EventType: ship | Action: created
Received event: player.updated | EventType: player | Action: updated
Received event: ship.deleted | EventType: ship | Action: deleted
```

**Summary**: Every create, update, and delete operation automatically publishes an event!

---

## Demonstration 3: API Gateway Routing

**Goal**: Show how the API Gateway routes requests to the correct service.

### Step 1: Access Through API Gateway

```powershell
# All requests go through port 5000 (API Gateway)
$players = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get
$ships = Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Get
$quests = Invoke-RestMethod -Uri "http://localhost:5000/api/quests" -Method Get

Write-Host "Players: $($players.Count) items"
Write-Host "Ships: $($ships.Count) items"
Write-Host "Quests: $($quests.Count) items"
```

### Step 2: Compare Direct Access vs Gateway

```powershell
# Through API Gateway (recommended)
Write-Host "=== Through API Gateway ==="
$viaGateway = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Get
Write-Host "Players via Gateway: $($viaGateway.Count)"

# Direct access (not recommended, but possible)
Write-Host "`n=== Direct Access ==="
$direct = Invoke-RestMethod -Uri "http://localhost:5001/api/players" -Method Get
Write-Host "Players direct: $($direct.Count)"
```

**Note**: Both should return the same data, but using the API Gateway is the recommended approach.

### Step 3: Test All Services Through Gateway

```powershell
$services = @(
    "players", "ships", "quests", "factions", "events",
    "items", "locations", "shops", "emissaries",
    "reputations", "crews", "achievements"
)

foreach ($service in $services) {
    try {
        $result = Invoke-RestMethod -Uri "http://localhost:5000/api/$service" -Method Get
        Write-Host "✅ $service : $($result.Count) items"
    } catch {
        Write-Host "❌ $service : Error - $($_.Exception.Message)"
    }
}
```

**Summary**: The API Gateway successfully routes requests to all 12 microservices!

---

## Demonstration 4: Complete Workflow

**Goal**: Create a complete game scenario with multiple related entities.

### Step 1: Create a Player

```powershell
$player = @{
    Name = "Captain Morgan"
    Gamertag = "CaptainMorgan"
    Gold = 25000
    Renown = 100
    IsPirateLegend = $true
    Platform = "Xbox"
} | ConvertTo-Json

$playerResult = Invoke-RestMethod -Uri "http://localhost:5000/api/players" -Method Post -Body $player -ContentType "application/json"
$playerId = $playerResult.id
Write-Host "Created Player: $($playerResult.name) (ID: $playerId)"
```

### Step 2: Create a Ship for the Player

```powershell
$ship = @{
    Name = "Morgan's Revenge"
    Type = "Galleon"
    Description = "Captain Morgan's flagship"
    MaxCrewSize = 4
    CannonCount = 8
    MastCount = 3
    IsActive = $true
} | ConvertTo-Json

$shipResult = Invoke-RestMethod -Uri "http://localhost:5000/api/ships" -Method Post -Body $ship -ContentType "application/json"
$shipId = $shipResult.id
Write-Host "Created Ship: $($shipResult.name) (ID: $shipId)"
```

### Step 3: Create a Crew

```powershell
$crew = @{
    Name = "Morgan's Crew"
    ShipId = $shipId
    MaxMembers = 4
    CurrentMembers = 1
    Status = "Active"
    Notes = "Captain Morgan's crew"
} | ConvertTo-Json

$crewResult = Invoke-RestMethod -Uri "http://localhost:5000/api/crews" -Method Post -Body $crew -ContentType "application/json"
$crewId = $crewResult.id
Write-Host "Created Crew: $($crewResult.name) (ID: $crewId)"
```

### Step 4: Add Reputation

```powershell
$reputation = @{
    PlayerId = $playerId
    FactionName = "Gold Hoarders"
    Level = 50
    TotalReputation = 250000
    Notes = "High reputation with Gold Hoarders"
} | ConvertTo-Json

$repResult = Invoke-RestMethod -Uri "http://localhost:5000/api/reputations" -Method Post -Body $reputation -ContentType "application/json"
Write-Host "Created Reputation: Level $($repResult.level) with $($repResult.factionName)"
```

### Step 5: Add an Achievement

```powershell
$achievement = @{
    PlayerId = $playerId
    Name = "Pirate Legend"
    Description = "Achieved Pirate Legend status"
    Category = "Prestige"
    Rarity = "Legendary"
    Progress = 100
    RequiredProgress = 100
    Notes = "Ultimate achievement"
} | ConvertTo-Json

$achResult = Invoke-RestMethod -Uri "http://localhost:5000/api/achievements" -Method Post -Body $achievement -ContentType "application/json"
Write-Host "Created Achievement: $($achResult.name)"
```

### Step 6: View Complete Profile

```powershell
Write-Host "`n=== Complete Player Profile ===" -ForegroundColor Cyan

# Get player
$player = Invoke-RestMethod -Uri "http://localhost:5000/api/players/$playerId" -Method Get
Write-Host "Player: $($player.name) - $($player.gold) gold"

# Get reputations
$reputations = Invoke-RestMethod -Uri "http://localhost:5000/api/reputations/player/$playerId" -Method Get
Write-Host "Reputations: $($reputations.Count)"

# Get achievements
$achievements = Invoke-RestMethod -Uri "http://localhost:5000/api/achievements" -Method Get
$playerAchievements = $achievements | Where-Object { $_.playerId -eq $playerId }
Write-Host "Achievements: $($playerAchievements.Count)"

# Get crew
$crew = Invoke-RestMethod -Uri "http://localhost:5000/api/crews/$crewId" -Method Get
Write-Host "Crew: $($crew.name) on ship ID $($crew.shipId)"
```

**Summary**: You've created a complete game profile with player, ship, crew, reputation, and achievement!

---

## Demonstration 5: Monitoring and Health Checks

**Goal**: Show how to monitor service health and status.

### Step 1: Check All Service Status

```powershell
Write-Host "=== Service Status ===" -ForegroundColor Cyan
docker-compose ps
```

### Step 2: Check API Gateway Health

```powershell
Write-Host "`n=== API Gateway Health ===" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "http://localhost:5000/health" -Method Get
    Write-Host "✅ API Gateway: Healthy" -ForegroundColor Green
} catch {
    Write-Host "❌ API Gateway: Unhealthy" -ForegroundColor Red
}
```

### Step 3: Check Individual Service Health

```powershell
Write-Host "`n=== Individual Service Health ===" -ForegroundColor Cyan
$services = @(5001, 5002, 5003, 5004, 5005, 5006, 5007, 5008, 5009, 5010, 5011, 5012)

foreach ($port in $services) {
    try {
        $health = Invoke-RestMethod -Uri "http://localhost:$port/health" -Method Get -TimeoutSec 2
        Write-Host "✅ Port $port : Healthy" -ForegroundColor Green
    } catch {
        Write-Host "❌ Port $port : Unhealthy or unreachable" -ForegroundColor Red
    }
}
```

### Step 4: View Recent Logs

```powershell
Write-Host "`n=== Recent API Gateway Logs ===" -ForegroundColor Cyan
docker-compose logs --tail=10 api-gateway
```

### Step 5: Monitor Event Consumer

```powershell
Write-Host "`n=== Event Consumer Status ===" -ForegroundColor Cyan
docker-compose logs --tail=20 event-consumer | Select-String -Pattern "Received event|Connected|Error"
```

### Step 6: Check RabbitMQ Status

```powershell
Write-Host "`n=== RabbitMQ Status ===" -ForegroundColor Cyan
docker-compose ps rabbitmq

Write-Host "`nAccess RabbitMQ Management UI at: http://localhost:15672"
Write-Host "Login: guest / guest"
```

**Summary**: You can monitor all aspects of the system to ensure everything is running correctly!

---

## Quick Reference: All Demonstrations

| Demo | Description | Time |
|------|-------------|------|
| [Demo 1](#demonstration-1-basic-api-operations) | Basic CRUD operations | 5 min |
| [Demo 2](#demonstration-2-event-driven-architecture) | Event publishing and consumption | 5 min |
| [Demo 3](#demonstration-3-api-gateway-routing) | API Gateway routing | 3 min |
| [Demo 4](#demonstration-4-complete-workflow) | Complete game scenario | 10 min |
| [Demo 5](#demonstration-5-monitoring-and-health-checks) | Monitoring and health | 5 min |

---

## Tips for Presentations

1. **Prepare in advance**: Run through each demo once before presenting
2. **Have two terminals ready**: One for commands, one for watching logs
3. **Use browser**: Show API responses in browser for visual appeal
4. **Explain as you go**: Describe what's happening at each step
5. **Show events**: The event-driven demo is particularly impressive!

---

**Ready to demonstrate? Start with Demo 1 and work your way through!** ⚓

