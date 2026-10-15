namespace Frontend.Models
{
    public class CreateAccountDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string TempPassword { get; set; } = "";
        public string Role { get; set; } = "Customer"; // "Admin", "Dealer", "Customer"
        public Guid? DealerId { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
