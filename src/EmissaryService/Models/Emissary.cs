using System.ComponentModel.DataAnnotations;

namespace EmissaryService.Models;

public class Emissary
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PlayerId { get; set; }

    [Required]
    [MaxLength(200)]
    public string FactionName { get; set; } = string.Empty;

    [Required]
    public int Level { get; set; } = 1; // Emissary level 1-5

    public bool IsActive { get; set; } = false;

    public DateTime? RaisedDate { get; set; }

    public DateTime? LoweredDate { get; set; }

    public int Value { get; set; } = 0; // Emissary value/grade

    [MaxLength(500)]
    public string Notes { get; set; } = string.Empty;
}

