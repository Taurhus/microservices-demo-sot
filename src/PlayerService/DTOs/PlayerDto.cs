namespace PlayerService.DTOs;

public record PlayerDto(
    int Id, 
    string Name, 
    string Gamertag, 
    long Gold, 
    int Renown, 
    bool IsPirateLegend, 
    DateTime? LastLoginDate, 
    DateTime CreatedDate, 
    string Platform);

public record CreatePlayerDto(
    string Name, 
    string Gamertag, 
    long Gold = 0, 
    int Renown = 0, 
    bool IsPirateLegend = false, 
    string Platform = "");

public record UpdatePlayerDto(
    int Id, 
    string Name, 
    string Gamertag, 
    long Gold, 
    int Renown, 
    bool IsPirateLegend, 
    DateTime? LastLoginDate, 
    string Platform);

