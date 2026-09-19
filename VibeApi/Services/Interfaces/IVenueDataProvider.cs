using VibeApi.Models;

namespace VibeApi.Services.Interfaces
{
    public interface IVenueDataProvider
    {
        Task<Venue?> FetchVenueByExternalRefAsync(string externalRefId);
    }
}
