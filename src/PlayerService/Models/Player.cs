using System.ComponentModel.DataAnnotations;

namespace PlayerService.Models;

public class Player
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Gamertag { get; set; } = string.Empty;

    public long Gold { get; set; } = 0;

    public int Renown { get; set; } = 0;

    public bool IsPirateLegend { get; set; } = false;

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [MaxLength(50)]
    public string Platform { get; set; } = string.Empty; // Xbox, Steam, PlayStation, etc.
}
