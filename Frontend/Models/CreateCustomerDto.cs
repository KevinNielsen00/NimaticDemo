namespace Frontend.Models
{
    public class CreateCustomerDto
    {
        public string Name { get; set; } = "";
        public Guid? DealerId { get; set; }
    }
}
