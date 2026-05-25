namespace Backend.DTOs.Units;

public class CreateUnitDto
{
    public string MacAddress { get; set; } = string.Empty;
    public string? UnitName { get; set; }
    public string? Location { get; set; }

    public Guid? AccountId { get; set; }
}