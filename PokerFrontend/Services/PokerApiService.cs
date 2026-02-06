using PokerFrontend.Models;
using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace PokerFrontend.Services;

public interface IPokerApiService
{
    Task<List<Table>?> GetTablesAsync();
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

    public PokerApiService(HttpClient http)
    {
        _http = http;
    }

    public void SetAuthToken(string? token)
    {
        if (string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = null;
        else
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<Table>?> GetTablesAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<Table>>("api/table/tables");
        }
        catch { return null; }
    }

    public async Task<int?> CreateTableAsync(string name, long minBuyIn, long maxBuyIn)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/table/create", new { TableName = name, MinBuyIn = minBuyIn, MaxBuyIn = maxBuyIn });
            if (response.IsSuccessStatusCode)
            {
                var tableId = await response.Content.ReadFromJsonAsync<int>();
                return tableId;
            }
            return null;
        }
        catch { return null; }
    }

    public async Task<bool> JoinTableAsync(int tableId, int seatNumber, long chipDeposit)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/table/join", new { TableId = tableId, SeatNumber = seatNumber, ChipDeposit = chipDeposit });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> LeaveTableAsync(int tableId)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/table/leave", new { TableId = tableId });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> StartGameAsync(int tableId)
    {
        try
        {
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"api/game/start/{tableId}", content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> PostActionAsync(int tableId, string action, long? amount)
    {
        try
        {
            var response = await _http.PostAsJsonAsync($"api/game/action/{tableId}", new PlayerActionRequest { Action = action, Amount = amount });
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<GameState?> GetGameStatusAsync(int tableId)
    {
        try
        {
            return await _http.GetFromJsonAsync<GameState>($"api/game/status/{tableId}");
        }
        catch { return null; }
    }

    public async Task<bool> NextPhaseAsync(int tableId)
    {
        try
        {
            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"api/game/next-phase/{tableId}", content);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }
}
