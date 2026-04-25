using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class UserSettings
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid AccountId { get; set; }

    [ForeignKey(nameof(AccountId))]
    public Account? Account { get; set; }

    public bool DarkMode { get; set; } = false;

    public bool AutoEmailWhenLow { get; set; } = false;

    public int LowLevelThreshold { get; set; } = 50;
}