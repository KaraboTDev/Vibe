using Microsoft.EntityFrameworkCore;
using VibeApi.Data;
using VibeApi.DTOs.Venues;
using VibeApi.Models;
using VibeApi.Services.Interfaces;

namespace VibeApi.Services
{
    public class VenueService : IVenueService
    {
        private readonly VibeDbContext _db;
        private readonly IVenueDataProvider _provider;

        public VenueService(VibeDbContext db, IVenueDataProvider provider)
        {
            _db = db;
            _provider = provider;
        }

        public async Task<List<VenueResponse>> GetAllAsync(string? tag = null)
        {
            var query = _db.Venues.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tag))
            {
                var t = tag.Trim();
                query = query.Where(v => v.VenueTags.Any(vt => vt.Tag.Name == t));
            }

            var list = await query.Take(100).ToListAsync();
            return list.Select(v => new VenueResponse
            {
                ExternalRefId = v.ExternalRefId,
                Name = v.Name,
                Latitude = v.Latitude,
                Longitude = v.Longitude,
                Address = v.Address
            }).ToList();
        }

        public async Task<VenueDetailResponse?> GetByExternalRefAsync(string externalRefId)
        {
            var v = await _db.Venues.Include(vv => vv.VenueTags).ThenInclude(vt => vt.Tag)
                .SingleOrDefaultAsync(x => x.ExternalRefId == externalRefId);
            if (v == null) return null;

            return new VenueDetailResponse
            {
                ExternalRefId = v.ExternalRefId,
                Name = v.Name,
                Latitude = v.Latitude,
                Longitude = v.Longitude,
                Address = v.Address,
                OpeningHours = v.OpeningHours,
                Tags = v.VenueTags.Select(t => t.Tag.Name).ToList()
            };
        }

        public async Task<Venue> GetOrCreateByExternalRefAsync(string externalRefId)
        {
            var v = await _db.Venues.SingleOrDefaultAsync(x => x.ExternalRefId == externalRefId);
            if (v != null) return v;

            var fetched = await _provider.FetchVenueByExternalRefAsync(externalRefId);
            if (fetched == null)
                throw new InvalidOperationException("Unable to fetch venue");

            var newVenue = new Venue
            {
                ExternalRefId = fetched.ExternalRefId,
                Name = fetched.Name,
                Latitude = fetched.Latitude,
                Longitude = fetched.Longitude,
                Address = fetched.Address,
                OpeningHours = fetched.OpeningHours
            };

            _db.Venues.Add(newVenue);
            await _db.SaveChangesAsync();
            return newVenue;
        }
    }
}
