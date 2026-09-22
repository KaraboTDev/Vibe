using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VibeApi.Models;
using VibeApi.Services.Interfaces;

namespace VibeApi.Services
{
    public class BizDataProvider : IVenueDataProvider
    {
        private readonly HttpClient _http;
        private readonly ILogger<BizDataProvider>? _logger;

        public BizDataProvider(HttpClient http, ILogger<BizDataProvider>? logger = null)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
            _logger = logger;
            _http.BaseAddress = new System.Uri("https://bizdata-web.vercel.app/");
        }

        public async Task<Venue?> FetchVenueByExternalRefAsync(string externalRefId)
        {
            // BizData doesn't support lookup-by-id directly, so this is a light
            // placeholder for the interface — real lookups happen via SearchByCategoryAsync.
            return null;
        }

        public async Task<List<Venue>> SearchByCategoryAsync(string category, decimal lat, decimal lng)
        {
            var results = new List<Venue>();

            // Simple retry policy
            const int maxRetries = 3;
            const int baseDelayMs = 500;

            // Ensure we request South African results by adding country=ZA
            var requestUri = $"api/businesses?location=Pretoria&category={Uri.EscapeDataString(category)}";

            var rnd = new Random();
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var response = await _http.GetAsync(requestUri);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);

                        if (!doc.RootElement.TryGetProperty("businesses", out var businesses))
                            return results;

                        foreach (var b in businesses.EnumerateArray())
                        {
                            results.Add(new Venue
                            {
                                ExternalRefId = b.GetProperty("osm_id").ToString(),
                                Name = b.GetProperty("name").GetString() ?? "Unknown",
                                Latitude = b.GetProperty("lat").GetDecimal(),
                                Longitude = b.GetProperty("lon").GetDecimal(),
                                Address = b.TryGetProperty("address", out var a) ? a.GetString() : null,
                                OpeningHours = b.TryGetProperty("opening_hours", out var h) ? h.GetString() : null
                            });
                        }

                        return results;
                    }

                    // Non-success status - log details and possibly retry
                    var body = await response.Content.ReadAsStringAsync();
                    if (_logger != null)
                        _logger.LogWarning("External provider returned {StatusCode} {ReasonPhrase} on attempt {Attempt}. Body: {Body}", (int)response.StatusCode, response.ReasonPhrase, attempt, body);
                    else
                        Console.Error.WriteLine($"External provider returned {(int)response.StatusCode} {response.ReasonPhrase} on attempt {attempt}. Body: {body}");
                }
                catch (HttpRequestException ex)
                {
                    if (_logger != null)
                        _logger.LogWarning(ex, "Http request failed on attempt {Attempt}", attempt);
                    else
                        Console.Error.WriteLine($"Http request failed on attempt {attempt}: {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    if (_logger != null)
                        _logger.LogWarning(ex, "Request timed out on attempt {Attempt}", attempt);
                    else
                        Console.Error.WriteLine($"Request timed out on attempt {attempt}: {ex.Message}");
                }

                // If not last attempt, wait with exponential backoff + jitter
                if (attempt < maxRetries)
                {
                    var jitter = rnd.Next(0, 100);
                    var delay = baseDelayMs * (int)Math.Pow(2, attempt - 1) + jitter;
                    await Task.Delay(delay);
                }
            }

            // After retries failed, return empty results (fallback)
            return results;
        }
    }
}