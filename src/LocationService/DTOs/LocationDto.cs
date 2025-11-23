namespace LocationService.DTOs;

public record LocationDto(
    int Id, 
    string Name, 
    string Type, 
    string Region, 
    decimal? Latitude, 
    decimal? Longitude, 
    bool HasMerchant, 
    bool HasShipwright, 
    bool HasWeaponsmith, 
    bool HasTavern, 
    bool IsActive);

public record CreateLocationDto(
    string Name, 
    string Type, 
    string Region, 
    decimal? Latitude = null, 
    decimal? Longitude = null, 
    bool HasMerchant = false, 
    bool HasShipwright = false, 
    bool HasWeaponsmith = false, 
    bool HasTavern = false, 
    bool IsActive = true);

public record UpdateLocationDto(
    int Id, 
    string Name, 
    string Type, 
    string Region, 
    decimal? Latitude, 
    decimal? Longitude, 
    bool HasMerchant, 
    bool HasShipwright, 
    bool HasWeaponsmith, 
    bool HasTavern, 
    bool IsActive);

