using System.ComponentModel.DataAnnotations;

namespace EventService.Models;

public class EventEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // World Event, Encounter, etc.

    [MaxLength(100)]
    public string Difficulty { get; set; } = string.Empty; // Easy, Medium, Hard, Legendary

    public int? MinPlayers { get; set; }

    public int? MaxPlayers { get; set; }

    public int EstimatedDurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? IntroducedDate { get; set; }
}
