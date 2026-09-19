using System.ComponentModel.DataAnnotations;

namespace VibeApi.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        [Required]
        [MaxLength(10)]
        public string PreferredLanguage { get; set; } = "en";

        [MaxLength(200)]
        public string? DefaultArea { get; set; }

        public bool NotificationsEnabled { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Favorite> Favorites { get; set; } = new();
    }
}
