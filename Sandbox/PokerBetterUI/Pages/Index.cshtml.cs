using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokerBetterUI.Models;
using PokerBetterUI.Services;
using System.Linq;

namespace PokerBetterUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApiService _api;

        public IndexModel(ApiService api)
        {
            _api = api;
        }

        public List<string> CommunityCards { get; set; } = new();
        public List<string> CommunityCardImages { get; set; } = new();


        // --- Bind Properties ---
        [BindProperty]
        public string? NewPlayerName { get; set; }

        [BindProperty]
        public int NewChips { get; set; } = 1000;

        [BindProperty]
        public string? ActionPlayer { get; set; }

        [BindProperty]
        public int Amount { get; set; }

        // --- Game state ---
        public GameStateDTO? GameState { get; set; }
        // --- Showdown state ---
        public ShowdownResultDTO? ShowdownState { get; set; }


        // Error handling
        public string? ErrorMessage { get; set; }

        // =====================
        // OnGet
        // =====================
        public async Task OnGetAsync()
        {
            GameState = await _api.GetStateAsync();

            ShowdownState = GameState?.Showdown;

            if (GameState == null)
            {
                ErrorMessage = "Tidak bisa konek ke server. Silakan coba lagi nanti.";
                GameState = new GameStateDTO
                {
                    Players = new List<PlayerDTO>(),
                    CommunityCards = new List<string>(),
                    Phase = string.Empty,
                    CurrentPlayer = string.Empty,
                    CurrentBet = 0,
                    Pot = 0
                };
            }
            CommunityCards = GameState.CommunityCards ?? new List<string>();

            CommunityCardImages = CommunityCards
                .Select(CardImageMapper.ToImageFile)
                .ToList();
        }


        // Card list
        public static class CardImageMapper
        {
            private static readonly Dictionary<string, string> RankMap = new()
            {
                ["Two"] = "2",
                ["Three"] = "3",
                ["Four"] = "4",
                ["Five"] = "5",
                ["Six"] = "6",
                ["Seven"] = "7",
                ["Eight"] = "8",
                ["Nine"] = "9",
                ["Ten"] = "10",
                ["Jack"] = "J",
                ["Queen"] = "Q",
                ["King"] = "K",
                ["Ace"] = "A"
            };

            private static readonly Dictionary<string, string> SuitMap = new()
            {
                ["Clubs"] = "clubs",
                ["Diamonds"] = "diamonds",
                ["Hearts"] = "hearts",
                ["Spades"] = "spades"
            };

            public static string ToImageFile(string apiCard)
            {
                if (string.IsNullOrWhiteSpace(apiCard))
                    return "back_dark.png";

                var parts = apiCard.Split(" of ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                    return "back_dark.png";

                if (!RankMap.TryGetValue(parts[0], out var rank))
                    return "back_dark.png";

                if (!SuitMap.TryGetValue(parts[1], out var suit))
                    return "back_dark.png";

                return $"{suit}_{rank}.png";
            }
        }


        // =====================
        // Player management
        // =====================
        public async Task<IActionResult> OnPostAddPlayerAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewPlayerName))
                await _api.AddPlayerAsync(NewPlayerName, NewChips);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRemovePlayerAsync()
        {
            if (!string.IsNullOrWhiteSpace(NewPlayerName))
                await _api.RemovePlayerAsync(NewPlayerName);

            return RedirectToPage();
        }

        // =====================
        // Game control
        // =====================
        public async Task<IActionResult> OnPostStartRoundAsync()
        {
            await _api.StartRoundAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostNextPhaseAsync()
        {
            await _api.NextPhaseAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostShowdownAsync()
        {
            await _api.ShowdownAsync();
            GameState = await _api.GetStateAsync();
            ShowdownState = GameState?.Showdown;

            return Page();
        }



        // =====================
        // Player actions
        // =====================
        public async Task<IActionResult> OnPostBetAsync()
        {
            if (!string.IsNullOrWhiteSpace(ActionPlayer))
                await _api.BetAsync(ActionPlayer, Amount);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRaiseAsync()
        {
            if (!string.IsNullOrWhiteSpace(ActionPlayer))
                await _api.RaiseAsync(ActionPlayer, Amount);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCallAsync()
        {
            if (!string.IsNullOrWhiteSpace(ActionPlayer))
                await _api.CallAsync(ActionPlayer);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckAsync()
        {
            if (!string.IsNullOrWhiteSpace(ActionPlayer))
                await _api.CheckAsync(ActionPlayer);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFoldAsync()
        {
            if (!string.IsNullOrWhiteSpace(ActionPlayer))
                await _api.FoldAsync(ActionPlayer);

            return RedirectToPage();
        }
        public async Task<IActionResult> OnPostAllInAsync()
        {
            if (string.IsNullOrWhiteSpace(ActionPlayer))
            {
                ErrorMessage = "Player tidak diketahui!";
                return Page();
            }

            var success = await _api.AllInAsync(ActionPlayer);

            if (!success)
                ErrorMessage = "Gagal melakukan All-In!";

            GameState = await _api.GetStateAsync();
            ShowdownState = GameState?.Showdown;

            return RedirectToPage();
        }



        // =====================
        // Helper untuk tombol
        // =====================
        public bool IsCurrentPlayer(PlayerDTO player) =>
            GameState?.CurrentPlayer == player.Name && !player.IsFolded;

        // Fase betting post-flop
        private static readonly string[] PostFlopPhases = { "PreFlop", "Flop", "Turn", "River" };

        public bool CanBetOrRaise(PlayerDTO player) =>
            IsCurrentPlayer(player) && PostFlopPhases.Contains(GameState?.Phase);

        public bool CanCall(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState?.CurrentBet > 0 && PostFlopPhases.Contains(GameState?.Phase);

        public bool CanCheck(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState?.CurrentBet == 0 && PostFlopPhases.Contains(GameState?.Phase);

        public bool CanFold(PlayerDTO player) =>
            IsCurrentPlayer(player);

        public bool CanStartRound() =>
            GameState?.Players.Count(p => !p.IsFolded) >= 2 || GameState?.Phase == "Showdown";

        public bool CanNextPhase() =>
            GameState != null && PostFlopPhases.Contains(GameState.Phase) && IsBettingRoundOver();

        // =====================
        // Helper untuk tombol All-In
        // =====================
        public bool CanAllIn(PlayerDTO player)
        {

            return GameState != null
                   && GameState.CurrentPlayer == player.Name
                   && player.State == "Active"
                   && player.ChipStack > 0;
        }


        private bool IsBettingRoundOver()
        {
            return GameState != null &&
                   GameState.Players
                       .Where(p => !p.IsFolded)
                       .All(p => p.CurrentBet == GameState.CurrentBet);
        }

        public bool CanShowdown() =>
        GameState?.Phase == "Showdown" && IsBettingRoundOver();

    }
}
