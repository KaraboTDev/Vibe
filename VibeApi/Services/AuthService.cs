using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VibeApi.Data;
using VibeApi.DTOs.Auth;
using VibeApi.Models;
using VibeApi.Services.Interfaces;

namespace VibeApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly VibeDbContext _db;
        private readonly PasswordHasher<User> _hasher;
        private readonly IConfiguration _config;

        public AuthService(VibeDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new InvalidOperationException("EmailExists");

            var user = new User
            {
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _hasher.HashPassword(user, request.Password);

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var token = GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiresMinutes"))
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed) return null;

            var token = GenerateToken(user);

            return new AuthResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiresMinutes"))
            };
        }

        private string GenerateToken(User user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not configured");
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var expiresMinutes = _config.GetValue<int>("Jwt:ExpiresMinutes");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var signingKey = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
