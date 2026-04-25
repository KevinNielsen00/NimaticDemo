namespace Backend.DTOs.Units;

public class CreateUnitDto
{
    public string MacAddress { get; set; } = "";
    public string? UnitName { get; set; }
    public string? Location { get; set; }
}