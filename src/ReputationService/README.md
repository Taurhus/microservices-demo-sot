# Reputation Service

This service manages player reputation levels with different factions in Sea of Thieves.

## Features

- CRUD for reputations via `/api/reputations`
- Track player reputation levels (0-75) with different factions
- Monitor total reputation points
- Get reputations by player ID

## Endpoints

- `GET /api/reputations` - Get all reputations (paginated)
- `GET /api/reputations/{id}` - Get reputation by ID
- `GET /api/reputations/player/{playerId}` - Get reputations by player ID
- `POST /api/reputations` - Create new reputation
- `PUT /api/reputations/{id}` - Update reputation
- `DELETE /api/reputations/{id}` - Delete reputation

