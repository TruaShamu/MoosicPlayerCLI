using System.Net;
using System.Net.Http.Headers;

namespace MyApp
{
    public class SpotifyClient
    {
        private string _accessToken;
        private readonly Func<Task<string>> _refreshAccessToken;
        private HttpClient _httpClient;

        public SpotifyClient(string accessToken, Func<Task<string>> refreshAccessToken)
        {
            _accessToken = accessToken;
            _refreshAccessToken = refreshAccessToken;
            InitHttpClient();
        }

        private void InitHttpClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.spotify.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        private async Task<HttpResponseMessage> SendAsyncWithRefresh(Func<Task<HttpResponseMessage>> requestFunc)
        {
            var response = await requestFunc();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _accessToken = await _refreshAccessToken();
                InitHttpClient();
                response = await requestFunc();
            }

            return response;
        }

        public async Task<string> GetCurrentlyPlayingRawJsonAsync()
        {
            var response = await SendAsyncWithRefresh(() => _httpClient.GetAsync("me/player/currently-playing"));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
    }
}
