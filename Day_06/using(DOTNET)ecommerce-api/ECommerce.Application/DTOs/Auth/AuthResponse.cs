namespace ECommerce.Application.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string? PhoneNumber { get; set; }
        public IList<string> Roles { get; set; }
    }
}
