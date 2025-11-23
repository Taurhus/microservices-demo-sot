namespace CrewService.DTOs;

public record CrewDto(
    int Id, 
    string Name, 
    int ShipId, 
    int MaxMembers, 
    int CurrentMembers, 
    string Status, 
    DateTime CreatedDate, 
    DateTime? LastActivityDate, 
    string Notes);

public record CreateCrewDto(
    string Name, 
    int ShipId, 
    int MaxMembers = 4, 
    int CurrentMembers = 0, 
    string Status = "Active", 
    string Notes = "");

public record UpdateCrewDto(
    int Id, 
    string Name, 
    int ShipId, 
    int MaxMembers, 
    int CurrentMembers, 
    string Status, 
    DateTime? LastActivityDate, 
    string Notes);

