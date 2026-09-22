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
        public async Task<IActionResult> SearchVenues(string mood, decimal lat, decimal lng)
        {
            var moodTrimmed = (mood ?? string.Empty).Trim().ToLower();

            // 1. Fetch from external provider
            var providerVenues = await _provider.SearchByCategoryAsync("cafe", lat, lng);

            if (providerVenues != null && providerVenues.Any())
            {
                // 2. Ensure provider venues are persisted (only add new ones)
                var externalIds = providerVenues.Select(v => v.ExternalRefId).Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

                var existing = await _db.Venues.Where(v => externalIds.Contains(v.ExternalRefId)).ToListAsync();
                var existingByExt = existing.ToDictionary(v => v.ExternalRefId, v => v);

                foreach (var pv in providerVenues)
                {
                    if (string.IsNullOrWhiteSpace(pv.ExternalRefId))
                        continue;

                    if (!existingByExt.ContainsKey(pv.ExternalRefId))
                    {
                        var newVenue = new Venue
                        {
                            ExternalRefId = pv.ExternalRefId,
                            Name = pv.Name,
                            Latitude = pv.Latitude,
                            Longitude = pv.Longitude,
                            Address = pv.Address,
                            OpeningHours = pv.OpeningHours
                        };

                        _db.Venues.Add(newVenue);
                        // keep the dictionary so further iterations won't duplicate
                        existingByExt[pv.ExternalRefId] = newVenue;
                    }
                }

                // 3. Persist any new venues
                await _db.SaveChangesAsync();
            }

            // 4. Query DB for venues matching the mood (case-insensitive)
            var matched = await _db.Venues
                .Include(v => v.VenueTags).ThenInclude(vt => vt.Tag)
                .Where(v => v.VenueTags.Any(vt => EF.Functions.ILike(vt.Tag.Name, moodTrimmed)))
                .ToListAsync();

            // 5. Map to DTOs
            var dtoList = matched.Select(v => new VenueResultDto
            {
                Id = v.Id,
                Name = v.Name,
                Latitude = v.Latitude,
                Longitude = v.Longitude,
                Address = v.Address,
                OpeningHours = v.OpeningHours,
                Tags = v.VenueTags.Select(vt => vt.Tag.Name).ToList()
            })
            // 6. Sort by approximate distance (Euclidean on lat/lng degrees)
            .OrderBy(v => GetDistanceSquared(v.Latitude, v.Longitude, lat, lng))
            .ToList();

            return Ok(dtoList);
        }

        // Simple approximate distance squared (no Earth curvature) for ordering
        private static double GetDistanceSquared(decimal lat1, decimal lng1, decimal lat2, decimal lng2)
        {
            var dy = (double)(lat1 - lat2);
            var dx = (double)(lng1 - lng2);
            return dx * dx + dy * dy;
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
