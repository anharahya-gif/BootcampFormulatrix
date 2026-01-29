using System.Net.Http;
using System.Net.Http.Json;
using PokerUI.Models;

namespace PokerUI.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private const string BaseUrl = "http://localhost:5175/api/GameControllerAPI/";

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<GameStateDTO?> GetStateAsync()
        {
            try
            {
                var resp = await _http.GetAsync(BaseUrl + "state");
                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"GetStateAsync failed: {resp.StatusCode}");
                    return null;
                }
                var result = await resp.Content.ReadFromJsonAsync<GameStateDTO>();
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetStateAsync exception: {ex.Message}");
                return null;
            }
        }


        public async Task<bool> AddPlayerAsync(string name, int chips = 1000)
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}addPlayer?name={Uri.EscapeDataString(name)}&chips={chips}", null);
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
                var resp = await _http.PostAsync($"{BaseUrl}removePlayer?name={Uri.EscapeDataString(name)}", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

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

        public async Task<ShowdownResultDTO?> ShowdownAsync()
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}showdown", null);

                if (!resp.IsSuccessStatusCode)
                    return null;

                var result = await resp.Content
                    .ReadFromJsonAsync<ShowdownResultDTO>();

                return result;
            }
            catch
            {
                return null;
            }
        }


        // --- Player actions ---
        public async Task<bool> BetAsync(string name, int amount)
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}bet?name={Uri.EscapeDataString(name)}&amount={amount}", null);
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
                var resp = await _http.PostAsync($"{BaseUrl}raise?name={Uri.EscapeDataString(name)}&amount={amount}", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> AllInAsync(string name)
        {
            var resp = await _http.PostAsync(
                $"{BaseUrl}allin?name={Uri.EscapeDataString(name)}",
                null
            );
            Console.WriteLine($"AllIn clicked: {name}");

            return resp.IsSuccessStatusCode;
        }



        public async Task<bool> CallAsync(string name)
        {
            try
            {
                var resp = await _http.PostAsync($"{BaseUrl}call?name={Uri.EscapeDataString(name)}", null);
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
                var resp = await _http.PostAsync($"{BaseUrl}check?name={Uri.EscapeDataString(name)}", null);
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
                var resp = await _http.PostAsync($"{BaseUrl}fold?name={Uri.EscapeDataString(name)}", null);
                return resp.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
