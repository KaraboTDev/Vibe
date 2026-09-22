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

        [HttpGet]
        public async Task<ActionResult<List<VenueResultDto>>> Search(string mood, decimal lat, decimal lng)
        {
            // 1. Search external provider for "cafe" category venues
            var externalResults = await _provider.SearchByCategoryAsync("cafe", lat, lng);

            // 2. For each result, ensure it exists in the Venues table (match on ExternalRefId)
            foreach (var ext in externalResults)
            {
                var exists = await _db.Venues.AnyAsync(v => v.ExternalRefId == ext.ExternalRefId);
                if (!exists)
                {
                    var newVenue = new Venue
                    {
                        ExternalRefId = ext.ExternalRefId,
                        Name = ext.Name,
                        Latitude = ext.Latitude,
                        Longitude = ext.Longitude,
                        Address = ext.Address,
                        OpeningHours = ext.OpeningHours
                    };
                    _db.Venues.Add(newVenue);
                }
            }

            // 3. Save changes to the database
            await _db.SaveChangesAsync();

            // 4. Query Venues table for venues that have a VenueTag where Tag.Name equals mood (case-insensitive)
            var moodTrimmed = (mood ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(moodTrimmed)) return Ok(new List<VenueResultDto>());

            var moodLower = moodTrimmed.ToLower();

            var matchedVenues = await _db.Venues
                .Include(v => v.VenueTags)
                    .ThenInclude(vt => vt.Tag)
                .Where(v => v.VenueTags.Any(vt => vt.Tag.Name.ToLower() == moodLower))
                .ToListAsync();

            // 5. Map to VenueResultDto
            var results = matchedVenues.Select(v => new VenueResultDto
            {
                Id = v.Id,
                Name = v.Name,
                Latitude = v.Latitude,
                Longitude = v.Longitude,
                Address = v.Address,
                OpeningHours = v.OpeningHours,
                Tags = v.VenueTags.Select(vt => vt.Tag.Name).ToList()
            }).ToList();

            // 6. Sort by approximate planar distance from provided lat/lng
            results = results.OrderBy(r =>
            {
                var dLat = (double)(r.Latitude - lat);
                var dLng = (double)(r.Longitude - lng);
                return Math.Sqrt(dLat * dLat + dLng * dLng);
            }).ToList();

            // 7. Return the sorted list
            return Ok(results);
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
