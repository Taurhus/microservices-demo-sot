using System.ComponentModel.DataAnnotations;

namespace AchievementService.Models;

public class Achievement
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty; // Combat, Exploration, Trading, etc.

    [MaxLength(50)]
    public string Rarity { get; set; } = "Common"; // Common, Rare, Epic, Legendary

    public DateTime UnlockedDate { get; set; } = DateTime.UtcNow;

    public int Progress { get; set; } = 100; // Progress percentage (0-100)

    public int RequiredProgress { get; set; } = 100; // Required progress to unlock

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

