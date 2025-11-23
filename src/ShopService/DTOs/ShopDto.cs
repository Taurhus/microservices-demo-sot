namespace ShopService.DTOs;

public record ShopDto(
    int Id, 
    string Name, 
    string Type, 
    string LocationName, 
    string Description, 
    bool IsActive);

public record CreateShopDto(
    string Name, 
    string Type, 
    string LocationName, 
    string Description, 
    bool IsActive = true);

public record UpdateShopDto(
    int Id, 
    string Name, 
    string Type, 
    string LocationName, 
    string Description, 
    bool IsActive);

