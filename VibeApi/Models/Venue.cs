using System.ComponentModel.DataAnnotations;

namespace VibeApi.Models
{
    public class Venue
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string ExternalRefId { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(500)]
        public string? OpeningHours { get; set; }

        public List<VenueTag> VenueTags { get; set; } = new();

        public List<Favorite> Favorites { get; set; } = new();
    }
}
