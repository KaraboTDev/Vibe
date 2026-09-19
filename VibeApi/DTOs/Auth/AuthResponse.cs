namespace VibeApi.DTOs.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public int UserId { get; set; }
        public string Email { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}
