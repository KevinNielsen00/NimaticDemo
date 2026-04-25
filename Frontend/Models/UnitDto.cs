namespace Frontend.Models;

public class UnitDto
{
    public Guid Id { get; set; }
    public string MacAddress { get; set; } = "";
    public string? UnitName { get; set; }
    public string? Location { get; set; }
}