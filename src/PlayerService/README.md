# PlayerService

This service manages player data for the Sea of Thieves microservices demo.

## Endpoints
- CRUD for players via `/api/player`

## Database
- Uses Azure SQL (or compatible) via EF Core

## Running Locally
- `dotnet run` or use Docker Compose

## Environment Variables
- `ConnectionStrings__DefaultConnection`

## Swagger
- Visit `/swagger` when running to view and test the API.

---
See root README for full stack orchestration.
