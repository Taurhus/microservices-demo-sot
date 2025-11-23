using System.ComponentModel.DataAnnotations;

namespace ReputationService.Models;

public class Reputation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FactionName { get; set; } = string.Empty;

    [Required]
    public int Level { get; set; } = 0; // Reputation level 0-75

    public long TotalReputation { get; set; } = 0; // Total reputation points

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

