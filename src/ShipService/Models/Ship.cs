using System.ComponentModel.DataAnnotations;

namespace ShipService.Models;

public class Ship
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // Sloop, Brigantine, Galleon

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int MaxCrewSize { get; set; } = 1;

    public int CannonCount { get; set; } = 0;

    public int MastCount { get; set; } = 1;

    public bool IsActive { get; set; } = true;
}
