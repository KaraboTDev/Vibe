using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using VibeApi.Data;
using VibeApi.Services;
using VibeApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext
builder.Services.AddDbContext<VibeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
// Register BizDataProvider with an injected HttpClient
builder.Services.AddHttpClient<IVenueDataProvider, BizDataProvider>();
builder.Services.AddScoped<IVenueService, VenueService>();

// Authentication - JWT
// Read configuration using both common keys to be tolerant of different config styles.
// Preferred configuration (appsettings.json or environment variables):
//   "Jwt": { "Key": "...", "Issuer": "...", "Audience": "..." }
// Environment variable form for nested keys: Jwt__Key, Jwt__Issuer, Jwt__Audience
var jwtKey = builder.Configuration["Jwt:Key"] ?? builder.Configuration["JwtKey"] ?? string.Empty;
var issuer = builder.Configuration["Jwt:Issuer"] ?? builder.Configuration["JwtIssuer"];
var audience = builder.Configuration["Jwt:Audience"] ?? builder.Configuration["JwtAudience"];

// Fail fast with clear errors so missing/empty configuration is caught on startup.
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException(
        "Missing configuration: Jwt:Key (or JwtKey). Set a non-empty signing key. For environment variables use 'Jwt__Key'.");

var key = Encoding.UTF8.GetBytes(jwtKey);
if (key.Length == 0)
    throw new InvalidOperationException("Jwt:Key produced zero-length bytes. Ensure Jwt:Key is not empty.");

if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
    throw new InvalidOperationException("Missing configuration: Jwt:Issuer and Jwt:Audience must be set.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

var app = builder.Build();

// Apply pending migrations automatically on startup (optional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VibeDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
