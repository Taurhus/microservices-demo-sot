namespace ReputationService.DTOs;

public record ReputationDto(
    int Id, 
    int PlayerId, 
    string FactionName, 
    int Level, 
    long TotalReputation, 
    DateTime LastUpdated, 
    string Notes);

public record CreateReputationDto(
    int PlayerId, 
    string FactionName, 
    int Level = 0, 
    long TotalReputation = 0, 
    string Notes = "");

public record UpdateReputationDto(
    int Id, 
    int PlayerId, 
    string FactionName, 
    int Level, 
    long TotalReputation, 
    DateTime LastUpdated, 
    string Notes);

