using System.Text.Json;

namespace MyApp
{
    public class TokenStore
    {
        private const string TokenPath = "tokens.json";

        public static void Save(string accessToken, string refreshToken)
        {
            var json = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "access_token", accessToken },
                { "refresh_token", refreshToken }
            });
            File.WriteAllText(TokenPath, json);
        }

        public static (string? accessToken, string? refreshToken) Load()
        {
            if (!File.Exists(TokenPath))
                return (null, null);

            var json = File.ReadAllText(TokenPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return (data?["access_token"], data?["refresh_token"]);
        }
    }
}
