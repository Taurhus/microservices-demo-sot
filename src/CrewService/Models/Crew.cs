using System.ComponentModel.DataAnnotations;

namespace CrewService.Models;

public class Crew
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int ShipId { get; set; }

    [Required]
    public int MaxMembers { get; set; } = 4;

    public int CurrentMembers { get; set; } = 0;

    [MaxLength(50)]
    public string Status { get; set; } = "Active"; // Active, Inactive, Sunk

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastActivityDate { get; set; }

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

