using VibeApi.Models;

namespace VibeApi.Services.Interfaces
{
    public interface IVenueDataProvider
    {
        Task<Venue?> FetchVenueByExternalRefAsync(string externalRefId);
        Task<List<VibeApi.Models.Venue>> SearchByCategoryAsync(string category, decimal lat, decimal lng);
    }
}
