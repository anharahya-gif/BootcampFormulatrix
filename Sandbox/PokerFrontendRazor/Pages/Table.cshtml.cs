using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokerFrontendRazor.Services;

namespace PokerFrontendRazor.Pages
{
    public class TableModel : PageModel
    {
        private readonly PokerApiService _pokerService;
        public string CurrentPlayerName { get; private set; } = string.Empty;

        public TableModel(PokerApiService pokerService)
        {
            _pokerService = pokerService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentPlayerName = HttpContext.Session.GetString("PlayerName");
            var chips = HttpContext.Session.GetInt32("PlayerChips") ?? 1000;

            if (string.IsNullOrEmpty(CurrentPlayerName))
            {
                return RedirectToPage("/Index");
            }

            // Auto-join seat logic (Try seat -1 for auto assignment)
            // Ideally we only do this once or handle idempotency
            await _pokerService.JoinSeatAsync(CurrentPlayerName, -1);

            return Page();
        }
    }
}
