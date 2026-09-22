using System.ComponentModel.DataAnnotations;

namespace VibeApi.DTOs.Favorites
{
    public class CreateFavoriteRequest
    {
        public int VenueId { get; set; }
    }
}
