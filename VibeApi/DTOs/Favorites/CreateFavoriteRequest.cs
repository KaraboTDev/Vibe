using System.ComponentModel.DataAnnotations;

namespace VibeApi.DTOs.Favorites
{
    public class CreateFavoriteRequest
    {
        [Required]
        public string ExternalRefId { get; set; } = null!;
    }
}
