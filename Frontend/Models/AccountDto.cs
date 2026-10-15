namespace Frontend.Models
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string Role { get; set; } = "";
        public Guid? DealerId { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
