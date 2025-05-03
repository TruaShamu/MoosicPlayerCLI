using Spectre.Console;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using DotNetEnv;
using Microsoft.VisualBasic;

namespace MyApp
{
    public static class Program
    {

        static string clientId;
        static string clientSecret;
        static string redirectUri;
        static string tokenEndpoint = "https://accounts.spotify.com/api/token";
        static string authEndpoint = "https://accounts.spotify.com/authorize";
        static string accessToken = "";
        static string refreshToken = "";
        
        public static async Task Main(string[] args)
        {

            // Pull secrets from .env
            Env.Load();
            clientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? throw new ArgumentNullException("CLIENT_ID is not set in the environment variables.");
            clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? throw new ArgumentNullException("CLIENT_SECRET is not set in the environment variables.");
            redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? throw new ArgumentNullException("REDIRECT_URI is not set in the environment variables.");
            
            
            AnsiConsole.MarkupLine("[green]Starting Spotify CLI...[/]");

            (accessToken, refreshToken) = TokenStore.Load();

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                await Authorize();
                TokenStore.Save(accessToken, refreshToken);
            }

            // Pull the currently playing song
            SpotifyClient client = new SpotifyClient(accessToken, async () =>
            {
                accessToken = await RefreshAccessToken();
                return accessToken;
            });

            var nowPlaying = await client.GetCurrentlyPlayingRawJsonAsync();
            var nowPlayingJson = JsonDocument.Parse(nowPlaying);
            var nowPlayingSong = nowPlayingJson.RootElement.GetProperty("item").GetProperty("name").GetString();
            var nowPlayingArtist = nowPlayingJson.RootElement.GetProperty("item").GetProperty("artists")[0].GetProperty("name").GetString();
            AnsiConsole.MarkupLine($"Now Playing: [green]{nowPlayingSong}[/] by [yellow]{nowPlayingArtist}[/]");
        }

        private static async Task Authorize()
        {
            /* Refer here: https://developer.spotify.com/documentation/web-api/tutorials/code-flow */
            var queryParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "response_type", "code" },
                { "redirect_uri", redirectUri },
                { "state", Guid.NewGuid().ToString() },
                { "scope", "user-read-playback-state user-modify-playback-state" },
            };

            var queryString = await new FormUrlEncodedContent(queryParams).ReadAsStringAsync();
            var url = $"{authEndpoint}?{queryString}";

            // Listen for the redirect URI and get the auth code.
            var listener = new HttpListener();
            listener.Prefixes.Add("http://127.0.0.1:8888/");
            listener.Start();
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            var context = await listener.GetContextAsync();
            var code = context.Request.QueryString["code"];

            // Write the response 
            var response = context.Response;
            var buffer = System.Text.Encoding.UTF8.GetBytes("Authorization complete. You may close this window.");
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
            response.OutputStream.Close();
            listener.Stop();

            // Exchange the auth code for an access token and refresh token.
            using var client = new HttpClient();
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
            });
            var tokenResp = await client.PostAsync(tokenEndpoint, content);
            var json = await tokenResp.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            accessToken = doc.RootElement.GetProperty("access_token").GetString();
            refreshToken = doc.RootElement.GetProperty("refresh_token").GetString();
        }

        private static async Task<string> RefreshAccessToken()
        {
            using var client = new HttpClient();
            var auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("refresh_token", refreshToken),
            });

            var response = await client.PostAsync(tokenEndpoint, content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                AnsiConsole.MarkupLine($"[red]Failed to refresh token: {json}[/]");
                throw new Exception("Token refresh failed.");
            }
            AnsiConsole.MarkupLine($"[green]Token refreshed successfully.[/]");

            var doc = JsonDocument.Parse(json);
            var newAccessToken = doc.RootElement.GetProperty("access_token").GetString();

            if (doc.RootElement.TryGetProperty("refresh_token", out var newRefreshTokenProp))
            {
                refreshToken = newRefreshTokenProp.GetString();
                TokenStore.Save(accessToken, refreshToken);

            }
            accessToken = newAccessToken;
            return newAccessToken;
        }
    }
}