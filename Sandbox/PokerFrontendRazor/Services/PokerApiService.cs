using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PokerFrontendRazor.Services
{
    public class PokerApiService
    {
        private readonly HttpClient _httpClient;

        public PokerApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> RegisterPlayerAsync(string name, int chips)
        {
            // Note: API expects query parameters for registerPlayer
            // POST /api/GameControllerAPI/registerPlayer?playerName=...&chipStack=...
            var response = await _httpClient.PostAsync($"/api/GameControllerAPI/registerPlayer?playerName={name}&chipStack={chips}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> JoinSeatAsync(string name, int seatIndex)
        {
             var response = await _httpClient.PostAsync($"/api/GameControllerAPI/joinSeat?playerName={name}&seatIndex={seatIndex}", null);
             return response.IsSuccessStatusCode;
        }
    }
}
