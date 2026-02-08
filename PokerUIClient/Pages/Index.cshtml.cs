using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;

namespace PokerUIClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [BindProperty]
        public string PlayerName { get; set; }

        [BindProperty]
        public int ChipStack { get; set; }

        public bool IsRegistered { get; set; } = false;
        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            // cek session
            var name = HttpContext.Session.GetString("PlayerName");
            var chips = HttpContext.Session.GetInt32("ChipStack");
            if (!string.IsNullOrEmpty(name) && chips > 0)
            {
                PlayerName = name;
                ChipStack = chips.Value;
                IsRegistered = true;
            }
        }

        public async Task<IActionResult> OnPostRegisterAsync()
        {
            if (string.IsNullOrEmpty(PlayerName) || ChipStack <= 0)
            {
                ErrorMessage = "Nama player dan chip harus diisi dengan benar!";
                return Page();
            }

            try
            {
                // panggil API register player
                var url = $"http://localhost:5175/api/GameControllerAPI/registerPlayer?playerName={PlayerName}&chipStack={ChipStack}";
                var response = await _httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    // simpan session
                    HttpContext.Session.SetString("PlayerName", PlayerName);
                    HttpContext.Session.SetInt32("ChipStack", ChipStack);

                    IsRegistered = true;
                }
                else
                {
                    ErrorMessage = $"Gagal register: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }

            return Page();
        }

        public IActionResult OnPostStartGame()
        {
            var name = HttpContext.Session.GetString("PlayerName");
            var chips = HttpContext.Session.GetInt32("ChipStack") ?? 0;

            if (string.IsNullOrEmpty(name) || chips == 0)
            {
                ErrorMessage = "Player belum terdaftar!";
                return Page();
            }

            return RedirectToPage("/Table");
        }
    }
}
