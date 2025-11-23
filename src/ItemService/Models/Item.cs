using System.ComponentModel.DataAnnotations;

namespace ItemService.Models;

public class Item
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty; // Equipment, Consumable, Treasure, etc.

    [MaxLength(50)]
    public string Rarity { get; set; } = string.Empty; // Common, Rare, Legendary, etc.

    public decimal? BaseValue { get; set; } // Gold value

    public bool IsStackable { get; set; }

    public int? MaxStackSize { get; set; }

    public bool IsActive { get; set; } = true;
}
