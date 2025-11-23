using System.ComponentModel.DataAnnotations;

namespace QuestService.Models;

public class Quest
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FactionName { get; set; } = string.Empty; // Which trading company offers this quest

    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Voyage, Tall Tale, etc.

    public int RequiredReputationLevel { get; set; } = 0;

    public int EstimatedDurationMinutes { get; set; } = 30;

    public decimal? GoldReward { get; set; }

    public bool IsActive { get; set; } = true;
}
