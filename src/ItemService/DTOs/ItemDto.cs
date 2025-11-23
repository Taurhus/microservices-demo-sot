namespace ItemService.DTOs;

public record ItemDto(
    int Id, 
    string Name, 
    string Description, 
    string Category, 
    string Rarity, 
    decimal? BaseValue, 
    bool IsStackable, 
    int? MaxStackSize, 
    bool IsActive);

public record CreateItemDto(
    string Name, 
    string Description, 
    string Category, 
    string Rarity, 
    decimal? BaseValue = null, 
    bool IsStackable = false, 
    int? MaxStackSize = null, 
    bool IsActive = true);

public record UpdateItemDto(
    int Id, 
    string Name, 
    string Description, 
    string Category, 
    string Rarity, 
    decimal? BaseValue, 
    bool IsStackable, 
    int? MaxStackSize, 
    bool IsActive);

