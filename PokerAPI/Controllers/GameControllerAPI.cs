using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerAPI.Hubs;
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameControllerAPI : ControllerBase
    {
        private readonly IGameController _game;
        private readonly IHubContext<PokerHub> _hub;

        // ======================
        // Constructor
        // ======================
        public GameControllerAPI(
            IGameController game,
            IHubContext<PokerHub> hub)
        {
            _game = game;
            _hub = hub;

            // Subscribe to game events to broadcast updates via SignalR
            _game.CommunityCardsUpdated += () =>
            {
                _ = _hub.Clients.All.SendAsync("CommunityCardsUpdated", new
                {
                    communityCards = _game.CommunityCards.Select(c => $"{c.Rank} of {c.Suit}")
                });
            };

            _game.ShowdownCompleted += () =>
            {
                var details = _game.GetShowdownDetails();
                _ = _hub.Clients.All.SendAsync("ShowdownCompleted", details);
            };
        }

        // ======================
        // Internal Helpers
        // ======================
        private void AdvanceTurnIfNeeded()
        {
            if (_game.IsBettingRoundOver())
                _game.NextPhase();
            else
                _game.GetNextActivePlayer(); // pindah ke pemain aktif berikutnya
        }

        // ======================
        // DTO Builder
        // ======================
        private object BuildGameStateDto()
        {
            return new
            {
                gameState = _game.GetGameState(),
                phase = _game.Phase.ToString(),
                currentPlayer = _game.GetCurrentPlayer()?.Name,
                currentBet = _game.CurrentBet,
                pot = _game.GetTotalPot(),
                communityCards = _game.CommunityCards
                    .Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                players = _game.GetPlayersPublicState()
                    .Select(p => new
                    {
                        p.Name,
                        p.ChipStack,
                        p.CurrentBet,
                        p.IsFolded,
                        p.SeatIndex,
                        p.State,
                        hand = p.Hand != null ? p.Hand.ToList() : new List<string>()
                    }).ToList(),
                showdown = _game.LastShowdown == null ? null : new
                {
                    winners = _game.LastShowdown.Winners.Select(p => p.Name).ToList(),
                    handRank = _game.LastShowdown.HandRank.ToString(),
                    message = _game.LastShowdown.Message
                }
            };
        }

        private async Task BroadcastGameState()
        {
            await _hub.Clients.All.SendAsync(
                "ReceiveGameState",
                BuildGameStateDto());
        }

        // ======================
        // Player Management
        // ======================
        [HttpPost("addPlayer")]
        public async Task<IActionResult> AddPlayer(
            [FromQuery] string name,
            [FromQuery] int chips = 1000,
            [FromQuery] int seatIndex = -1)
        {
            try
            {
                _game.AddPlayer(name, chips, seatIndex);
                await BroadcastGameState();
                return Ok(new { success = true, message = "Player added" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        [HttpPost("removePlayer")]
        public async Task<IActionResult> RemovePlayer([FromQuery] string name)
        {
            try
            {
                var player = _game.GetPlayerByName(name);
                if (player == null)
                    return NotFound(new { success = false, message = "Player not found" });

                _game.RemovePlayer(player);
                await BroadcastGameState();
                return Ok(new { success = true, message = "Player removed" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ======================
        // Add Chips
        // ======================
        [HttpPost("addchips")]
        public async Task<IActionResult> AddChips([FromBody] AddChipsRequest request)
        {
            var player = _game.GetPlayerByName(request.PlayerName);
            if (player == null)
                return NotFound("Player not found");

            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than 0");

            player.ChipStack += request.Amount;
            await BroadcastGameState();

            return Ok(new
            {
                PlayerName = player.Name,
                NewChips = player.ChipStack,
                Message = $"{request.Amount} chip berhasil ditambahkan."
            });
        }

        // ======================
        // Round Management
        // ======================
        [HttpPost("startRound")]
        public async Task<IActionResult> StartRound()
        {
            _game.StartRound();
            await BroadcastGameState();
            return Ok();
        }

        [HttpPost("nextPhase")]
        public async Task<IActionResult> NextPhase()
        {
            _game.NextPhase();
            await BroadcastGameState();
            return Ok();
        }

        // ======================
        // Betting Actions
        // ======================
        [HttpPost("bet")]
        public async Task<IActionResult> Bet(
            [FromQuery] string name,
            [FromQuery] int amount)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            var success = _game.HandleBet(player, amount);
            if (success) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(new { success });
        }

        [HttpPost("call")]
        public async Task<IActionResult> Call([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            var success = _game.HandleCall(player);
            if (success) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(new { success });
        }

        [HttpPost("raise")]
        public async Task<IActionResult> Raise(
            [FromQuery] string name,
            [FromQuery] int amount)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            var success = _game.HandleRaise(player, amount);
            if (success) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(new { success });
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            _game.HandleCheck(player);
            AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok();
        }

        [HttpPost("fold")]
        public async Task<IActionResult> Fold([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            _game.HandleFold(player);
            AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok();
        }

        [HttpPost("allin")]
        public async Task<IActionResult> AllIn([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound();

            bool success = _game.HandleAllIn(player.Name);
            if (!success)
                return BadRequest("Player tidak bisa all-in");

            AdvanceTurnIfNeeded();
            await BroadcastGameState();

            return Ok(new { success = true });
        }

        // ======================
        // Showdown
        // ======================
        [HttpPost("showdown")]
        public async Task<IActionResult> Showdown()
        {
            var result = _game.ResolveShowdownDetailed();
            await BroadcastGameState();
            return Ok(result);
        }

        // ======================
        // Game State
        // ======================
        [HttpGet("state")]
        public IActionResult State()
        {
            return Ok(BuildGameStateDto());
        }
    }
}
