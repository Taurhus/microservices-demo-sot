namespace AchievementService.DTOs;

public record AchievementDto(
    int Id, 
    int PlayerId, 
    string Name, 
    string Description, 
    string Category, 
    string Rarity, 
    DateTime UnlockedDate, 
    int Progress, 
    int RequiredProgress, 
    string Notes);

public record CreateAchievementDto(
    int PlayerId, 
    string Name, 
    string Description, 
    string Category, 
    string Rarity = "Common", 
    int Progress = 0, 
    int RequiredProgress = 100, 
    string Notes = "");

public record UpdateAchievementDto(
    int Id, 
    int PlayerId, 
    string Name, 
    string Description, 
    string Category, 
    string Rarity, 
    DateTime UnlockedDate, 
    int Progress, 
    int RequiredProgress, 
    string Notes);

