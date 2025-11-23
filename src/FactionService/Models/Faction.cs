using System.ComponentModel.DataAnnotations;

namespace FactionService.Models;

public class Faction
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Type { get; set; } = string.Empty; // Trading Company, Faction, etc.

    [MaxLength(200)]
    public string Headquarters { get; set; } = string.Empty;

    public int MaxReputationLevel { get; set; } = 75;

    public bool IsActive { get; set; } = true;

    public DateTime? IntroducedDate { get; set; }
}
