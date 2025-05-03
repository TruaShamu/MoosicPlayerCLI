using System.Net;
using System.Net.Http.Headers;

namespace MyApp
{
    public class SpotifyClient
    {
        private string _accessToken;
        private readonly HttpClient _httpClient;

        public SpotifyClient(string accessToken)
        {
            _accessToken = accessToken;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.spotify.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        public async Task<string> GetCurrentlyPlayingRawJsonAsync()
        {
            var response = await _httpClient.GetAsync("me/player/currently-playing");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
