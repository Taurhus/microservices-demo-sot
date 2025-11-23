# Achievement Service

This service manages player achievements in Sea of Thieves.

## Features

- CRUD for achievements via `/api/achievements`
- Track player achievements and progress
- Monitor achievement categories and rarities
- Get achievements by player ID

## Endpoints

- `GET /api/achievements` - Get all achievements (paginated)
- `GET /api/achievements/{id}` - Get achievement by ID
- `GET /api/achievements/player/{playerId}` - Get achievements by player ID
- `POST /api/achievements` - Create new achievement
- `PUT /api/achievements/{id}` - Update achievement
- `DELETE /api/achievements/{id}` - Delete achievement

