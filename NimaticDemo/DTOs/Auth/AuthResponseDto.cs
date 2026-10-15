namespace Backend.DTOs.Auth
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public bool MustChangePassword { get; set; } = false;
    }
}
