using System.Collections.Generic;

namespace VibeApi.DTOs.Venues
{
    public class VenueDetailResponse
    {
        public string ExternalRefId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
        public string? OpeningHours { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
