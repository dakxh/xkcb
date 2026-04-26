using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace XKCB
{
    public class ApiClient
    {
        // Pulled directly from your frontend code
        private static readonly HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("https://xkca.dadalapathy756.workers.dev")
        };

        public async Task<CatalogResponse?> GetCatalogAsync(int cursor = 0)
        {
            var response = await _httpClient.GetAsync($"/api/catalog?cursor={cursor}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Native, high-performance JSON parsing
            return JsonSerializer.Deserialize<CatalogResponse>(json);
        }

        public async Task<MediaDetails?> GetDetailsAsync(string id)
        {
            // Safely encode the ID just in case it contains special characters
            var response = await _httpClient.GetAsync($"/api/details/{Uri.EscapeDataString(id)}");

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<MediaDetails>(json);
        }

        public async Task<List<MediaItem>?> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<MediaItem>();

            // Uri.EscapeDataString ensures spaces and special characters are safely sent
            var response = await _httpClient.GetAsync($"/api/search?q={Uri.EscapeDataString(query)}");

            if (!response.IsSuccessStatusCode) return new List<MediaItem>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<MediaItem>>(json);
        }

        public async Task<WatchResponse?> GetStreamAsync(string id)
        {
            var response = await _httpClient.GetAsync($"/api/watch/{Uri.EscapeDataString(id)}");

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<WatchResponse>(json);
        }
    }
}