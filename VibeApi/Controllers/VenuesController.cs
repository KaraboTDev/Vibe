using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VibeApi.Data;
using VibeApi.DTOs.Venues;
using VibeApi.Models;
using VibeApi.Services.Interfaces;

namespace VibeApi.Controllers
{
    [ApiController]
    [Route("venues")]
    public class VenuesController : ControllerBase
    {
        private readonly VibeDbContext _db;
        private readonly IVenueService _venueService;
        private readonly IVenueDataProvider _provider;

        public VenuesController(VibeDbContext db, IVenueService venueService, IVenueDataProvider provider)
        {
            _db = db;
            _venueService = venueService;
            _provider = provider;
        }

        [HttpGet("venues")]
public async Task<IActionResult> SearchVenues(string mood, decimal lat, decimal lng)
{
    var moodTrimmed = (mood ?? string.Empty).Trim().ToLower();

    // 1. Try BizData first
    var venues = await _provider.SearchByCategoryAsync("cafe", lat, lng);

    // 2. If BizData returns nothing, fall back to DB
    if (!venues.Any())
    {
        venues = await _db.Venues
            .Include(v => v.VenueTags)
                .ThenInclude(vt => vt.Tag)
            .Where(v => v.VenueTags.Any(vt => vt.Tag.Name == moodTrimmed))
            .ToListAsync();
    }
    else
    {
        // 3. If BizData returns venues, filter them by mood tags
        var venueIds = venues.Select(v => v.Id).ToList();

        venues = await _db.Venues
            .Include(v => v.VenueTags)
                .ThenInclude(vt => vt.Tag)
            .Where(v => venueIds.Contains(v.Id) &&
                        v.VenueTags.Any(vt => vt.Tag.Name == moodTrimmed))
            .ToListAsync();
    }

    return Ok(venues);
}


        [HttpGet("{id}")]
        public async Task<ActionResult<VenueResultDto>> GetById(int id)
        {
            var venue = await _db.Venues
                .Include(v => v.VenueTags).ThenInclude(vt => vt.Tag)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venue is null) return NotFound();

            return Ok(new VenueResultDto
            {
                Id = venue.Id,
                Name = venue.Name,
                Latitude = venue.Latitude,
                Longitude = venue.Longitude,
                Address = venue.Address,
                OpeningHours = venue.OpeningHours,
                Tags = venue.VenueTags.Select(vt => vt.Tag.Name).ToList()
            });
        }
    }
}
