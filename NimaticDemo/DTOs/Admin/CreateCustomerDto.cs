namespace Backend.DTOs.Admin
{
    public class CreateCustomerDto 
    { 
        public string Name { get; set; } = ""; 
        public Guid DealerId { get; set; } 
    }
}