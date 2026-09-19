using Microsoft.AspNetCore.Mvc;
using VibeApi.DTOs.Auth;
using VibeApi.Services.Interfaces;

namespace VibeApi.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _auth.RegisterAsync(request);
                return Created(string.Empty, result);
            }
            catch (InvalidOperationException ex) when (ex.Message == "EmailExists")
            {
                return Conflict(new { message = "Email already registered" });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _auth.LoginAsync(request);
            if (result == null) return Unauthorized(new { message = "Invalid credentials" });
            return Ok(result);
        }
    }
}
