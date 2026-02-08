using System.Net.Http;
using System.Net.Http.Json;
using PokerUIClient.Models;

namespace PokerUIClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:5175/api/GameControllerAPI/";

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        // =========================
        // Game State
        // =========================
        public async Task<GameStateDTO?> GetStateAsync()
        {
            try
            {
                var resp = await _http.GetAsync($"{BaseUrl}state");
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<GameStateDTO>();
            }
            catch
            {
                return null;
            }
        }

        // =========================
        // Player Management
        // =========================
        public async Task<bool> AddPlayerAsync(string name, int chips = 1000, int seatIndex = -1)
        {
            try
            {
                var url = $"{BaseUrl}addPlayer?name={Uri.EscapeDataString(name)}&chips={chips}&seatIndex={seatIndex}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RegisterPlayerAsync(string name, int chips)
        {
            try
            {
                var url = $"{BaseUrl}registerPlayer?playerName={Uri.EscapeDataString(name)}&chipStack={chips}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemovePlayerAsync(string name)
        {
            try
            {
                var url = $"{BaseUrl}removePlayer?name={Uri.EscapeDataString(name)}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> JoinSeatAsync(string name, int seatIndex)
        {
            try
            {
                var url = $"{BaseUrl}joinSeat?playerName={Uri.EscapeDataString(name)}&seatIndex={seatIndex}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddChipsAsync(string name, int amount)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync($"{BaseUrl}addchips", new { PlayerName = name, Amount = amount });
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // =========================
        // Round Management
        // =========================
        public async Task<bool> StartRoundAsync()
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}startRound", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> NextPhaseAsync()
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}nextPhase", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ShowdownDTO?> ShowdownAsync()
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}showdown", null);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<ShowdownDTO>();
            }
            catch
            {
                return null;
            }
        }

        // =========================
        // Player Actions
        // =========================
        public async Task<bool> BetAsync(string name, int amount)
        {
            try
            {
                var url = $"{BaseUrl}bet?name={Uri.EscapeDataString(name)}&amount={amount}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RaiseAsync(string name, int amount)
        {
            try
            {
                var url = $"{BaseUrl}raise?name={Uri.EscapeDataString(name)}&amount={amount}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CallAsync(string name)
        {
            try
            {
                var url = $"{BaseUrl}call?name={Uri.EscapeDataString(name)}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CheckAsync(string name)
        {
            try
            {
                var url = $"{BaseUrl}check?name={Uri.EscapeDataString(name)}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> FoldAsync(string name)
        {
            try
            {
                var url = $"{BaseUrl}fold?name={Uri.EscapeDataString(name)}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AllInAsync(string name)
        {
            try
            {
                var url = $"{BaseUrl}allin?name={Uri.EscapeDataString(name)}";
                var resp = await _http.PostAsync(url, null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
