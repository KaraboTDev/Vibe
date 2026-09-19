using System.ComponentModel.DataAnnotations;

namespace VibeApi.DTOs.Settings
{
    public class UpdateSettingsRequest
    {
        [MaxLength(10)]
        public string? PreferredLanguage { get; set; }

        [MaxLength(200)]
        public string? DefaultArea { get; set; }

        public bool? NotificationsEnabled { get; set; }
    }
}
