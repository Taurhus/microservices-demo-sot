namespace ShipService.DTOs;

public record ShipDto(
    int Id, 
    string Name, 
    string Type, 
    string Description, 
    int MaxCrewSize, 
    int CannonCount, 
    int MastCount, 
    bool IsActive);

public record CreateShipDto(
    string Name, 
    string Type, 
    string Description, 
    int MaxCrewSize, 
    int CannonCount, 
    int MastCount, 
    bool IsActive = true);

public record UpdateShipDto(
    int Id, 
    string Name, 
    string Type, 
    string Description, 
    int MaxCrewSize, 
    int CannonCount, 
    int MastCount, 
    bool IsActive);

