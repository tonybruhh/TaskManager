using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Api.Contracts;

namespace TaskManager.Tests;


public static class TestsHelper
{
    public static (string email, string user, string pass) NewUserTriplet(
    string? email = null,
    string? user = null,
    string? pass = null)
    {

        var slug = Guid.NewGuid().ToString("N")[..8];
        return (
            email ?? $"user_{slug}@example.com",
            user ?? $"user_{slug}",
            pass ?? "Sup3rStrong!Pass"
        );
    }

    public static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string username, string password)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));
            
        var req = new RegisterRequest(email, username, password);
        return await client.PostAsJsonAsync("/api/auth/register", req);
    }

    public static async Task<(HttpResponseMessage resp, string? token)> LoginAsync(HttpClient client, string emailOrUserName, string password)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        var req = new LoginRequest(emailOrUserName, password);
        var resp = await client.PostAsJsonAsync("/api/auth/login", req);

        string? token = null;
        if (resp.IsSuccessStatusCode)
        {
            var json = await resp.Content.ReadAsStringAsync();
            // Try to extract "token" ignoring case.
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            foreach (var prop in root.EnumerateObject())
            {
                if (string.Equals(prop.Name, "token", StringComparison.OrdinalIgnoreCase))
                {
                    token = prop.Value.GetString();
                    break;
                }
            }

            // Fallback: crude check for any JWT-looking string
            token ??= root.ToString().Contains(".") ? root.GetProperty("token").GetString() : null;
        }

        return (resp, token);
    }

    public static async Task<HttpResponseMessage> WhoAmIAsync(HttpClient client, string? token = null)
    {
        if (client is null)
            throw new ArgumentNullException(nameof(client));

        var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        if (token is not null)
        {
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return await client.SendAsync(req);
    }
}