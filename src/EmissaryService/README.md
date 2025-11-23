# Emissary Service

This service manages emissary flags in the Sea of Thieves game.

## Features

- CRUD for emissaries via `/api/emissaries`
- Track emissary flags raised by players for different factions
- Monitor emissary levels (1-5) and values
- Track active/inactive status and timestamps

## Endpoints

- `GET /api/emissaries` - Get all emissaries (paginated)
- `GET /api/emissaries/{id}` - Get emissary by ID
- `GET /api/emissaries/player/{playerId}` - Get emissaries by player ID
- `POST /api/emissaries` - Create new emissary
- `PUT /api/emissaries/{id}` - Update emissary
- `DELETE /api/emissaries/{id}` - Delete emissary

