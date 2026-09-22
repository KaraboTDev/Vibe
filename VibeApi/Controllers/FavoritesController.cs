using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VibeApi.Data;
using VibeApi.DTOs.Favorites;
using VibeApi.Services.Interfaces;

namespace VibeApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly VibeDbContext _db;
        private readonly IVenueService _venueService;

        public FavoritesController(VibeDbContext db, IVenueService venueService)
        {
            _db = db;
            _venueService = venueService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFavoriteRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var venueExists = await _db.Venues.AnyAsync(v => v.Id == request.VenueId);
            if (!venueExists) return NotFound(new { message = "Venue not found" });

            var exists = await _db.Favorites.AnyAsync(f => f.UserId == userId && f.VenueId == request.VenueId);
            if (exists) return Conflict(new { message = "Favorite already exists" });

            var fav = new Models.Favorite
            {
                UserId = userId,
                VenueId = request.VenueId,
                SavedAt = DateTime.UtcNow
            };

            _db.Favorites.Add(fav);
            await _db.SaveChangesAsync();

            return Created(string.Empty, new { fav.Id, fav.SavedAt });
        }
    }
}
