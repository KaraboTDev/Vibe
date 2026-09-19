namespace VibeApi.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int VenueId { get; set; }
        public Venue Venue { get; set; } = null!;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        public string SyncStatus { get; set; } = "synced";
    }
}
