
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PokerWebApp.Models;

namespace PokerWebApp.Services
{
    public class PokerApiClient
    {
        private readonly HttpClient _http;

        public PokerApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<GameStateViewModel> GetGameState()
        {
            return await _http.GetFromJsonAsync<GameStateViewModel>("gamestate")
                   ?? new GameStateViewModel();
        }

        public async Task AddPlayer(string name, int chips)
        {
            await _http.PostAsJsonAsync("addPlayer", new
            {
                name,
                chips
            });
        }
    }
}
