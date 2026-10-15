using Backend.Models;
namespace Backend.DTOs.Admin
{
    public class CreateAccountDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string TempPassword { get; set; } = "";
        public AccountRole Role { get; set; } = AccountRole.Customer;
        public Guid? DealerId { get; set; }
        public Guid? CustomerId { get; set; }
    }
}