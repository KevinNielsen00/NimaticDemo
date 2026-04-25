using Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Measurement
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid UnitId { get; set; }

    [ForeignKey(nameof(UnitId))]
    public Unit? Unit { get; set; }

    public int Depth { get; set; }          // L
    public int Restarts { get; set; }       // C
    public decimal Battery { get; set; }    // P
    public decimal Temperature { get; set; } // T

    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;
}