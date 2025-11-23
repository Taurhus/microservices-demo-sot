namespace QuestService.DTOs;

public record QuestDto(
    int Id, 
    string Name, 
    string Description, 
    string FactionName, 
    string Type, 
    int RequiredReputationLevel, 
    int EstimatedDurationMinutes, 
    decimal? GoldReward, 
    bool IsActive);

public record CreateQuestDto(
    string Name, 
    string Description, 
    string FactionName, 
    string Type, 
    int RequiredReputationLevel = 0, 
    int EstimatedDurationMinutes = 30, 
    decimal? GoldReward = null, 
    bool IsActive = true);

public record UpdateQuestDto(
    int Id, 
    string Name, 
    string Description, 
    string FactionName, 
    string Type, 
    int RequiredReputationLevel, 
    int EstimatedDurationMinutes, 
    decimal? GoldReward, 
    bool IsActive);

