using VibeApi.DTOs.Venues;

namespace VibeApi.Services.Interfaces
{
    public interface IVenueService
    {
        Task<List<VenueResponse>> GetAllAsync(string? tag = null);
        Task<VenueDetailResponse?> GetByExternalRefAsync(string externalRefId);
        Task<Models.Venue> GetOrCreateByExternalRefAsync(string externalRefId);
    }
}
