# Crew Service

This service manages crews (sessions) in Sea of Thieves.

## Features

- CRUD for crews via `/api/crews`
- Track crew membership and ship assignments
- Monitor crew status (Active, Inactive, Sunk)
- Track crew activity timestamps

## Endpoints

- `GET /api/crews` - Get all crews (paginated)
- `GET /api/crews/{id}` - Get crew by ID
- `POST /api/crews` - Create new crew
- `PUT /api/crews/{id}` - Update crew
- `DELETE /api/crews/{id}` - Delete crew

