namespace Backend.Models;

public class Unit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public string MacAddress { get; set; } = string.Empty;
    public string? UnitName { get; set; }
    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<Measurement> Measurements { get; set; } = new();
}