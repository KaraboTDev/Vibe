namespace VibeApi.DTOs.Settings
{
    public class SettingsResponse
    {
        public string PreferredLanguage { get; set; } = null!;
        public string? DefaultArea { get; set; }
        public bool NotificationsEnabled { get; set; }
    }
}
