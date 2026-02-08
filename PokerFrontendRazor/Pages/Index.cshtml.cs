using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokerFrontendRazor.Services;

namespace PokerFrontendRazor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly PokerApiService _pokerService;

        [BindProperty]
        public string PlayerName { get; set; } = string.Empty;

        [BindProperty]
        public int InitialChips { get; set; } = 1000;

        public IndexModel(ILogger<IndexModel> logger, PokerApiService pokerService)
        {
            _logger = logger;
            _pokerService = pokerService;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(PlayerName))
            {
                return Page();
            }

            // 1. Save to Session
            HttpContext.Session.SetString("PlayerName", PlayerName);
            HttpContext.Session.SetInt32("PlayerChips", InitialChips);

            // 2. Register via API (Optional but good practice to reserve name)
            await _pokerService.RegisterPlayerAsync(PlayerName, InitialChips);

            return RedirectToPage("/Table");
        }
    }
}
