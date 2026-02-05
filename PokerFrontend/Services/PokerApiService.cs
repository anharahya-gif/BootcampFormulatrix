using PokerFrontend.Models;

namespace PokerFrontend.Services;

public interface IPokerApiService
{
    Task<List<Table>> GetTablesAsync();
    Task<int?> CreateTableAsync(string name, long minBuyIn, long maxBuyIn);
    Task<bool> JoinTableAsync(int tableId, int seatNumber, long chipDeposit);
    Task<bool> LeaveTableAsync(int tableId);
    Task<bool> StartGameAsync(int tableId);
    Task<bool> PostActionAsync(int tableId, string action, long? amount);
    Task<GameState?> GetGameStatusAsync(int tableId);
    Task<bool> NextPhaseAsync(int tableId);
}

public class PokerApiService : IPokerApiService
{
    private readonly HttpClient _http;
    private readonly IAuthService _auth;

    public PokerApiService(HttpClient http, IAuthService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void AddAuthHeader()
    {
        var token = _auth.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Table>> GetTablesAsync()
    {
        try
        {
            AddAuthHeader();
            var response = await _http.GetFromJsonAsync<List<Table>>("api/table/tables");
            return response ?? new();
        }
        catch { return new(); }
    }

    public async Task<int?> CreateTableAsync(string name, long minBuyIn, long maxBuyIn)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("api/table/create", new { TableName = name, MinBuyIn = minBuyIn, MaxBuyIn = maxBuyIn });
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsAsync<dynamic>();
                return result?.table?.Id ?? result?.tableId;
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> JoinTableAsync(int tableId, int seatNumber, long chipDeposit)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("api/table/join", new { TableId = tableId, SeatNumber = seatNumber, ChipDeposit = chipDeposit });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> LeaveTableAsync(int tableId)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync("api/table/leave", new { TableId = tableId });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> StartGameAsync(int tableId)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsync($"api/game/start/{tableId}", null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> PostActionAsync(int tableId, string action, long? amount)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsJsonAsync($"api/game/action/{tableId}", new PlayerActionRequest { Action = action, Amount = amount });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<GameState?> GetGameStatusAsync(int tableId)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.GetFromJsonAsync<GameState>($"api/game/status/{tableId}");
            return response;
        }
        catch { return null; }
    }

    public async Task<bool> NextPhaseAsync(int tableId)
    {
        try
        {
            AddAuthHeader();
            var response = await _http.PostAsync($"api/game/next-phase/{tableId}", null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
