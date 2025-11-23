namespace EmissaryService.DTOs;

public record EmissaryDto(
    int Id, 
    int PlayerId, 
    string FactionName, 
    int Level, 
    bool IsActive, 
    DateTime? RaisedDate, 
    DateTime? LoweredDate, 
    int Value, 
    string Notes);

public record CreateEmissaryDto(
    int PlayerId, 
    string FactionName, 
    int Level = 1, 
    bool IsActive = false, 
    int Value = 0, 
    string Notes = "");

public record UpdateEmissaryDto(
    int Id, 
    int PlayerId, 
    string FactionName, 
    int Level, 
    bool IsActive, 
    DateTime? RaisedDate, 
    DateTime? LoweredDate, 
    int Value, 
    string Notes);

