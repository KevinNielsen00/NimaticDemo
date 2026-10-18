namespace Backend.DTOs.Units;

public class UnitDto
{
    public Guid Id { get; set; }
    public string MacAddress { get; set; } = "";
    public string? UnitName { get; set; }
    public string? Location { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
}