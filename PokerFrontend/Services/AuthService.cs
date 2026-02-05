using PokerFrontend.Models;

namespace PokerFrontend.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string password);
    Task<bool> LoginAsync(string username, string password);
    Task LogoutAsync();
    string? GetToken();
    bool IsAuthenticated { get; }
}

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private string? _token;

    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public AuthService(HttpClient http)
    {
        _http = http;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new { username, password });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<AuthResponse>();
                _token = result.Token;
                return true;
            }
            return false;
        }
        catch { return false; }
    }

    public Task LogoutAsync()
    {
        _token = null;
        return Task.CompletedTask;
    }

    public string? GetToken() => _token;
}
