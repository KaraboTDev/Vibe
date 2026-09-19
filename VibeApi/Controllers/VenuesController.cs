using Microsoft.AspNetCore.Mvc;
using VibeApi.Services.Interfaces;

namespace VibeApi.Controllers
{
    [ApiController]
    [Route("venues")]
    public class VenuesController : ControllerBase
    {
        private readonly IVenueService _venueService;

        public VenuesController(IVenueService venueService)
        {
            _venueService = venueService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? tag)
        {
            var list = await _venueService.GetAllAsync(tag);
            return Ok(list);
        }

        [HttpGet("{externalRefId}")]
        public async Task<IActionResult> Get(string externalRefId)
        {
            var v = await _venueService.GetByExternalRefAsync(externalRefId);
            if (v == null) return NotFound();
            return Ok(v);
        }
    }
}
