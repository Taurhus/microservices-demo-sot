namespace FactionService.DTOs;

public record FactionDto(
    int Id, 
    string Name, 
    string Description, 
    string Type, 
    string Headquarters, 
    int MaxReputationLevel, 
    bool IsActive, 
    DateTime? IntroducedDate);

public record CreateFactionDto(
    string Name, 
    string Description, 
    string Type, 
    string Headquarters, 
    int MaxReputationLevel, 
    bool IsActive = true, 
    DateTime? IntroducedDate = null);

public record UpdateFactionDto(
    int Id, 
    string Name, 
    string Description, 
    string Type, 
    string Headquarters, 
    int MaxReputationLevel, 
    bool IsActive, 
    DateTime? IntroducedDate);

