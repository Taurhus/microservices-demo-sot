namespace EventService.DTOs;

public record EventDto(
    int Id, 
    string Name, 
    string Description, 
    string Type, 
    string Difficulty, 
    int? MinPlayers, 
    int? MaxPlayers, 
    int EstimatedDurationMinutes, 
    bool IsActive, 
    DateTime? IntroducedDate);

public record CreateEventDto(
    string Name, 
    string Description, 
    string Type, 
    string Difficulty, 
    int? MinPlayers, 
    int? MaxPlayers, 
    int EstimatedDurationMinutes, 
    bool IsActive = true, 
    DateTime? IntroducedDate = null);

public record UpdateEventDto(
    int Id, 
    string Name, 
    string Description, 
    string Type, 
    string Difficulty, 
    int? MinPlayers, 
    int? MaxPlayers, 
    int EstimatedDurationMinutes, 
    bool IsActive, 
    DateTime? IntroducedDate);

