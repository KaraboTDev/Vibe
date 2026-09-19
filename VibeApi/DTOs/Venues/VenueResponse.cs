namespace VibeApi.DTOs.Venues
{
    public class VenueResponse
    {
        public string ExternalRefId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? Address { get; set; }
    }
}
