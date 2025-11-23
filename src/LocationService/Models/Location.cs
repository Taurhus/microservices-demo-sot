using System.ComponentModel.DataAnnotations;

namespace LocationService.Models;

public class Location
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Outpost, Island, Seapost, etc.

    [MaxLength(100)]
    public string Region { get; set; } = string.Empty; // The Wilds, The Shores of Plenty, etc.

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public bool HasMerchant { get; set; }

    public bool HasShipwright { get; set; }

    public bool HasWeaponsmith { get; set; }

    public bool HasTavern { get; set; }

    public bool IsActive { get; set; } = true;
}
