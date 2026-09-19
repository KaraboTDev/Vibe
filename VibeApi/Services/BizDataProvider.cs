using System.Threading.Tasks;
using VibeApi.Models;
using VibeApi.Services.Interfaces;

namespace VibeApi.Services
{
    // Simple provider that simulates fetching venue details from an external service.
    public class BizDataProvider : IVenueDataProvider
    {
        public Task<Venue?> FetchVenueByExternalRefAsync(string externalRefId)
        {
            // For a portfolio project we simulate external data. In production this would call an API.
            var v = new Venue
            {
                ExternalRefId = externalRefId,
                Name = $"Venue {externalRefId}",
                Latitude = 0.0m,
                Longitude = 0.0m,
                Address = null,
                OpeningHours = null
            };

            return Task.FromResult<Venue?>(v);
        }
    }
}
