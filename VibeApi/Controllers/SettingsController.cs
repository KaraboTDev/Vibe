using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VibeApi.Data;
using VibeApi.DTOs.Settings;

namespace VibeApi.Controllers
{
    [ApiController]
    [Authorize]
    [Route("settings")]
    public class SettingsController : ControllerBase
    {
        private readonly VibeDbContext _db;

        public SettingsController(VibeDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var resp = new SettingsResponse
            {
                PreferredLanguage = user.PreferredLanguage,
                DefaultArea = user.DefaultArea,
                NotificationsEnabled = user.NotificationsEnabled
            };

            return Ok(resp);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest req)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized();
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var user = await _db.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.PreferredLanguage)) user.PreferredLanguage = req.PreferredLanguage!;
            if (req.DefaultArea != null) user.DefaultArea = req.DefaultArea;
            if (req.NotificationsEnabled.HasValue) user.NotificationsEnabled = req.NotificationsEnabled.Value;

            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
