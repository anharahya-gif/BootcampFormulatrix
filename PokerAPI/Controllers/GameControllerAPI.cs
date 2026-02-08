using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerAPI.Hubs;
using PokerAPI.Services.Interfaces;
using PokerAPI.DTOs;
using PokerAPI.Services; // <-- untuk ServiceResult
using System.Linq;

namespace PokerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameControllerAPI : ControllerBase
    {
        private readonly IGameController _game;
        private readonly IHubContext<PokerHub> _hub;

        public GameControllerAPI(IGameController game, IHubContext<PokerHub> hub)
        {
            _game = game;
            _hub = hub;

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
                _game.GetNextActivePlayer();
        }

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

        private object BuildAddChipsResponse(string playerName, int newChips, int addedAmount)
        {
            return new
            {
                PlayerName = playerName,
                NewChips = newChips,
                Message = $"{addedAmount} chip berhasil ditambahkan."
            };
        }

        private async Task BroadcastGameState()
        {
            await _hub.Clients.All.SendAsync("ReceiveGameState", BuildGameStateDto());
        }

        // ======================
        // Player Management
        // ======================
        [HttpPost("addPlayer")]
        public async Task<IActionResult> AddPlayer([FromQuery] string name, [FromQuery] int chips = 1000, [FromQuery] int seatIndex = -1)
        {
            try
            {
                _game.AddPlayer(name, chips, seatIndex);
                await BroadcastGameState();
                return Ok(ServiceResult.Success("Player added"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ServiceResult.Failure(ex.Message));
            }
            catch
            {
                return StatusCode(500, ServiceResult.Failure("Internal server error"));
            }
        }
        [HttpPost("registerPlayer")]
        public async Task<IActionResult> RegisterPlayer([FromQuery] string playerName, [FromQuery] int chipStack)
        {
            try
            {
                // pakai RegisterPlayer bukan AddPlayer
                _game.RegisterPlayer(playerName, chipStack);
                await BroadcastGameState();
                return Ok(new { success = true, message = "Player registered" });
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

        [HttpPost("joinSeat")]
        public async Task<IActionResult> JoinSeat([FromQuery] string playerName, [FromQuery] int seatIndex)
        {
            var player = _game.GetPlayerByName(playerName);
            if (player == null)
                return NotFound(ServiceResult.Failure("Player belum terdaftar"));

            try
            {
                _game.UpdatePlayerSeat(playerName, seatIndex); // method baru untuk update seat
                await BroadcastGameState();
                return Ok(ServiceResult.Success($"Player {playerName} menempati seat {seatIndex}"));
            }
            catch (Exception ex)
            {
                return BadRequest(ServiceResult.Failure(ex.Message));
            }
        }

        [HttpPost("removePlayer")]
        public async Task<IActionResult> RemovePlayer([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null)
                return NotFound(ServiceResult.Failure("Player not found"));

            _game.RemovePlayer(player);
            await BroadcastGameState();
            return Ok(ServiceResult.Success("Player removed"));
        }

        // ======================
        // Add Chips
        // ======================
        [HttpPost("addchips")]
        public async Task<IActionResult> AddChips([FromBody] AddChipsRequest request)
        {
            var player = _game.GetPlayerByName(request.PlayerName);
            if (player == null)
                return NotFound(ServiceResult.Failure("Player not found"));

            if (request.Amount <= 0)
                return BadRequest(ServiceResult.Failure("Amount must be greater than 0"));

            player.ChipStack += request.Amount;
            await BroadcastGameState();

            var response = BuildAddChipsResponse(player.Name, player.ChipStack, request.Amount);
            return Ok(ServiceResult.Success("Chips added successfully"));

        }

        // ======================
        // Round Management
        // ======================
        [HttpPost("startRound")]
        public async Task<IActionResult> StartRound()
        {
            _game.StartRound();
            await BroadcastGameState();
            return Ok(ServiceResult.Success("Round started"));
        }

        [HttpPost("nextPhase")]
        public async Task<IActionResult> NextPhase()
        {
            _game.NextPhase();
            await BroadcastGameState();
            return Ok(ServiceResult.Success("Phase advanced"));
        }

        // ======================
        // Betting Actions
        // ======================
        [HttpPost("bet")]
        public async Task<IActionResult> Bet([FromQuery] string name, [FromQuery] int amount)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            var result = _game.HandleBet(player, amount);
            if (result.IsSuccess) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(result);
        }

        [HttpPost("call")]
        public async Task<IActionResult> Call([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            var result = _game.HandleCall(player);
            if (result.IsSuccess) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(result);
        }

        [HttpPost("raise")]
        public async Task<IActionResult> Raise([FromQuery] string name, [FromQuery] int amount)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            var result = _game.HandleRaise(player, amount);
            if (result.IsSuccess) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(result);
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            _game.HandleCheck(player);
            AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(ServiceResult.Success("Check performed"));
        }

        [HttpPost("fold")]
        public async Task<IActionResult> Fold([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            _game.HandleFold(player);
            AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return Ok(ServiceResult.Success("Folded"));
        }

        [HttpPost("allin")]
        public async Task<IActionResult> AllIn([FromQuery] string name)
        {
            var player = _game.GetPlayerByName(name);
            if (player == null) return NotFound(ServiceResult.Failure("Player not found"));

            var result = _game.HandleAllIn(player.Name);
            if (result.IsSuccess) AdvanceTurnIfNeeded();
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
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
