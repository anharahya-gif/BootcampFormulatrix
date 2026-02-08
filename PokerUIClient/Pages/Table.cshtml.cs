using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PokerUIClient.Models;
using PokerUIClient.Services;
using System.Linq;

namespace PokerUIClient.Pages
{
    public class TableModel : PageModel
    {
        private readonly ApiService _api;

        public TableModel(ApiService api)
        {
            _api = api;
        }

        // ==========================
        // Game state & showdown
        // ==========================
        public GameStateDTO? GameState { get; set; }
        public ShowdownDTO? ShowdownState { get; set; }

        public List<string> CommunityCards { get; set; } = new();
        public List<string> CommunityCardImages { get; set; } = new();

        // ==========================
        // Player login info (session)
        // ==========================
        public string? CurrentPlayerName { get; set; }
        public int CurrentPlayerChips { get; set; }

        // ==========================
        // Bind Properties
        // ==========================
        [BindProperty]
        public string? ActionPlayer { get; set; }

        [BindProperty]
        public int Amount { get; set; }

        public string? ErrorMessage { get; set; }

        // ==========================
        // OnGetAsync → load game state
        // ==========================
        public async Task OnGetAsync()
        {
            CurrentPlayerName = HttpContext.Session.GetString("PlayerName");
            CurrentPlayerChips = HttpContext.Session.GetInt32("ChipStack") ?? 0;

            if (string.IsNullOrEmpty(CurrentPlayerName))
            {
                ErrorMessage = "Player belum login!";
            }

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
            CommunityCardImages = CommunityCards.Select(CardImageMapper.ToImageFile).ToList();
        }

        // ==========================
        // Card mapper
        // ==========================
        public static class CardImageMapper
        {
            private static readonly Dictionary<string, string> RankMap = new()
            {
                ["Two"] = "2", ["Three"] = "3", ["Four"] = "4", ["Five"] = "5",
                ["Six"] = "6", ["Seven"] = "7", ["Eight"] = "8", ["Nine"] = "9",
                ["Ten"] = "10", ["Jack"] = "J", ["Queen"] = "Q", ["King"] = "K", ["Ace"] = "A"
            };

            private static readonly Dictionary<string, string> SuitMap = new()
            {
                ["Clubs"] = "clubs", ["Diamonds"] = "diamonds",
                ["Hearts"] = "hearts", ["Spades"] = "spades"
            };

            public static string ToImageFile(string apiCard)
            {
                if (string.IsNullOrWhiteSpace(apiCard)) return "back_dark.png";
                var parts = apiCard.Split(" of ", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) return "back_dark.png";
                if (!RankMap.TryGetValue(parts[0], out var rank)) return "back_dark.png";
                if (!SuitMap.TryGetValue(parts[1], out var suit)) return "back_dark.png";
                return $"{suit}_{rank}.png";
            }
        }

        // ==========================
        // Player helpers
        // ==========================
        public bool IsCurrentPlayer(PlayerDTO player) => CurrentPlayerName == player.Name;

        public bool CanBetOrRaise(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState != null &&
            new[] { "PreFlop", "Flop", "Turn", "River" }.Contains(GameState.Phase);

        public bool CanCall(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState != null &&
            GameState.CurrentBet > 0 && new[] { "PreFlop", "Flop", "Turn", "River" }.Contains(GameState.Phase);

        public bool CanCheck(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState != null &&
            GameState.CurrentBet == 0 && new[] { "PreFlop", "Flop", "Turn", "River" }.Contains(GameState.Phase);

        public bool CanFold(PlayerDTO player) => IsCurrentPlayer(player);

        public bool CanAllIn(PlayerDTO player) =>
            IsCurrentPlayer(player) && GameState != null &&
            player.ChipStack > 0 && player.State == "Active";

        // ==========================
        // Start round logic
        // ==========================
        public bool CanStartRound()
        {
            if (GameState == null || GameState.Players.Count < 2) return false;

            // Player pertama yang join seat
            var firstPlayer = GameState.Players.OrderBy(p => p.SeatIndex).FirstOrDefault();
            return firstPlayer != null && firstPlayer.Name == CurrentPlayerName;
        }

        // ==========================
        // Razor handlers
        // ==========================
        public async Task<IActionResult> OnPostJoinSeatAsync(int SeatIndex)
        {
            if (string.IsNullOrEmpty(CurrentPlayerName)) return Page();
            await _api.JoinSeatAsync(CurrentPlayerName,  SeatIndex);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostLeaveSeatAsync(string PlayerName)
        {
            if (string.IsNullOrEmpty(PlayerName)) return Page();
            await _api.RemovePlayerAsync(PlayerName);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddChipsAsync(string PlayerName, int Amount)
        {
            if (string.IsNullOrEmpty(PlayerName) || Amount <= 0) return Page();
            await _api.AddChipsAsync(PlayerName, Amount);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostStartRoundAsync()
        {
            if (!CanStartRound()) return Page();
            await _api.StartRoundAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBetAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer) && Amount > 0)
                await _api.BetAsync(ActionPlayer, Amount);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRaiseAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer) && Amount > 0)
                await _api.RaiseAsync(ActionPlayer, Amount);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCallAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer))
                await _api.CallAsync(ActionPlayer);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCheckAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer))
                await _api.CheckAsync(ActionPlayer);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostFoldAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer))
                await _api.FoldAsync(ActionPlayer);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAllInAsync()
        {
            if (!string.IsNullOrEmpty(ActionPlayer))
                await _api.AllInAsync(ActionPlayer);
            return RedirectToPage();
        }
    }
}
