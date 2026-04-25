namespace Backend.DTOs.Auth
{
    public class RegisterDto
    {
        public Guid CustomerId { get; set; }
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
