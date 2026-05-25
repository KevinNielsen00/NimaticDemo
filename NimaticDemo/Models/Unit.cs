namespace Backend.Models;

public class Unit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public string MacAddress { get; set; } = string.Empty;
    public string? UnitName { get; set; }
    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Measurement> Measurements { get; set; } = new();
}