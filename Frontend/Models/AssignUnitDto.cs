namespace Frontend.Models
{
    public class AssignUnitDto
    {
        public string MacAddress { get; set; } = "";
        public Guid AccountId { get; set; }
        public string UnitName { get; set; } = "";
        public string Location { get; set; } = "";
    }
}