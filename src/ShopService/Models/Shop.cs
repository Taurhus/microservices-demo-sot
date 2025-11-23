using System.ComponentModel.DataAnnotations;

namespace ShopService.Models;

public class Shop
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Weaponsmith, Shipwright, Clothing Shop, etc.

    [MaxLength(200)]
    public string LocationName { get; set; } = string.Empty; // Which outpost/island

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
