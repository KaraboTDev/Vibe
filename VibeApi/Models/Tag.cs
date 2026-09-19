using System.ComponentModel.DataAnnotations;

namespace VibeApi.Models
{
    public class Tag
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        public List<VenueTag> VenueTags { get; set; } = new();
    }
}
