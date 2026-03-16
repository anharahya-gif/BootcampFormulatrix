using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerAPIMultiplayerWithDB.Data;
using Microsoft.EntityFrameworkCore;
using PokerAPIMultiplayerWithDB.Hubs;
using PokerAPIMultiplayerWithDB.Services;
using System.Security.Claims;

namespace PokerAPIMultiplayerWithDB.Controllers
{
    [ApiController]
    [Route("api/game")]
    [Authorize]
    public class GameController : ControllerBase
    {
        private readonly PokerDbContext _dbContext;
        private readonly ITableGameService _gameService;
        private readonly IHubContext<GameHub> _gameHub;

        public GameController(PokerDbContext dbContext, ITableGameService gameService, IHubContext<GameHub> gameHub)
        {
            _dbContext = dbContext;
            _gameService = gameService;
            _gameHub = gameHub;
        }

        /// <summary>
        /// Get game state for a table
        /// </summary>
        [HttpGet("status/{tableId}")]
        public IActionResult GetGameStatus(int tableId)
        {
            var state = _gameService.GetGameState(tableId);
            if (state.TableId == 0)
                return NotFound("No game at this table");

            // Hide hole cards of other players
            var publicState = new
            {
                state.TableId,
                state.Phase,
                state.CommunityCards,
                state.Pot,
                state.CurrentBet,
                state.DealerSeatNumber,
                state.SmallBlindSeatNumber,
                state.BigBlindSeatNumber,
                state.CurrentPlayerSeatNumber,
                state.IsGameActive,
                state.RoundNumber,
                Players = state.Players.Select(kv => new
                {
                    SeatNumber = kv.Key,
                    Username = kv.Value.Username,
                    ChipStack = kv.Value.ChipStack,
                    HasFolded = kv.Value.HasFolded,
                    IsAllIn = kv.Value.IsAllIn,
                    CurrentBet = kv.Value.CurrentBet,
                    HasActed = kv.Value.HasActed,
                    HoleCards = kv.Value.PlayerId == GetPlayerId() ? kv.Value.HoleCards : new() // Only show own cards
                }).ToList()
            };

            return Ok(publicState);
        }

        /// <summary>
        /// Start a game at a table (requires min 2 players seated)
        /// </summary>
        [HttpPost("start/{tableId}")]
        public async Task<IActionResult> StartGame(int tableId)
        {
            var table = _dbContext.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                return NotFound("Table not found");

            var playersAtTable = _dbContext.PlayerAtTables
                .Where(pat => pat.TableId == tableId && pat.LeftAt == null)
                .Include(pat => pat.Player)
                .ToList();

            if (playersAtTable.Count < 2)
                return BadRequest("Need at least 2 players to start game");

            // Build seat to player map
            var seatMap = new Dictionary<int, (int playerId, string username, long chipDeposit)>();
            foreach (var pat in playersAtTable)
            {
                seatMap[pat.SeatNumber] = (pat.Player!.Id, pat.Player.Username, pat.ChipDeposit);
            }

            // Start the game
            var gameStarted = _gameService.StartGame(tableId, seatMap);
            if (!gameStarted)
                return BadRequest("Failed to start game");

            // Log in database
            // Mark table as InProgress
            table.Status = PokerAPIMultiplayerWithDB.Models.TableStatus.InProgress;
            _dbContext.Tables.Update(table);

            _dbContext.GameLogs.Add(new()
            {
                TableId = tableId,
                Action = "GAME_STARTED",
                DetailsJson = $"{{\"playerCount\": {playersAtTable.Count}}}",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Broadcast via SignalR
            var state = _gameService.GetGameState(tableId);
            await _gameHub.Clients.Group($"table-{tableId}").SendAsync("GameStarted", new
            {
                state.Phase,
                state.CommunityCards,
                state.Pot,
                CurrentPlayerSeat = state.CurrentPlayerSeatNumber
            });

            return Ok(new { success = true, message = "Game started" });
        }

        /// <summary>
        /// Player action: fold, check, call, bet, raise, all-in
        /// </summary>
        [HttpPost("action/{tableId}")]
        public async Task<IActionResult> PlayerAction(int tableId, [FromBody] PlayerActionRequest request)
        {
            var playerId = GetPlayerId();
            var playerAtTable = _dbContext.PlayerAtTables
                .FirstOrDefault(pat => pat.TableId == tableId && pat.PlayerId == playerId && pat.LeftAt == null);

            if (playerAtTable == null)
                return BadRequest("Player not seated at this table");

            // Check turn: ensure caller is the current player for this table
            var state = _gameService.GetGameState(tableId);
            if (state.TableId == 0)
                return BadRequest("No active game at this table");

            if (state.CurrentPlayerSeatNumber != playerAtTable.SeatNumber)
            {
                return BadRequest(new { success = false, message = $"Not your turn. Current player seat: {state.CurrentPlayerSeatNumber}" });
            }

            // Execute action
            var actionSucceeded = _gameService.PlayerAction(tableId, playerId, request.Action, request.Amount);
            if (!actionSucceeded)
                return BadRequest("Invalid action or game state");

            // Log action
            _dbContext.GameLogs.Add(new()
            {
                TableId = tableId,
                Action = request.Action.ToUpper(),
                DetailsJson = $"{{\"playerId\": {playerId}, \"amount\": {request.Amount ?? 0}}}",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Broadcast state update
            state = _gameService.GetGameState(tableId);
            await _gameHub.Clients.Group($"table-{tableId}").SendAsync("GameStateUpdated", new
            {
                state.Phase,
                state.CommunityCards,
                state.Pot,
                state.CurrentBet,
                CurrentPlayerSeat = state.CurrentPlayerSeatNumber,
                Players = state.Players.Select(kv => new
                {
                    SeatNumber = kv.Key,
                    Username = kv.Value.Username,
                    ChipStack = kv.Value.ChipStack,
                    CurrentBet = kv.Value.CurrentBet,
                    HasFolded = kv.Value.HasFolded,
                    IsAllIn = kv.Value.IsAllIn
                }).ToList()
            });

            return Ok(new { success = true, message = $"Action '{request.Action}' executed" });
        }

        /// <summary>
        /// Advance to next game phase (flop, turn, river, showdown)
        /// Only callable when current phase betting is complete
        /// </summary>
        [HttpPost("next-phase/{tableId}")]
        public async Task<IActionResult> NextPhase(int tableId)
        {
            var table = _dbContext.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                return NotFound("Table not found");

            // Verify caller is authorized (could be dealer or admin)
            _gameService.MoveToNextPhase(tableId);

            var state = _gameService.GetGameState(tableId);
            _dbContext.GameLogs.Add(new()
            {
                TableId = tableId,
                Action = "PHASE_ADVANCED",
                DetailsJson = $"{{\"phase\": \"{state.Phase}\"}}",
                CreatedAt = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Broadcast
            await _gameHub.Clients.Group($"table-{tableId}").SendAsync("PhaseAdvanced", new
            {
                state.Phase,
                state.CommunityCards,
                state.Pot,
                CurrentPlayerSeat = state.CurrentPlayerSeatNumber
            });

            return Ok(new { success = true, phase = state.Phase.ToString() });
        }

        // ==================== HELPERS ====================

        private int GetPlayerId()
        {
            var claim = User.FindFirst("playerId");
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        public class PlayerActionRequest
        {
            public string Action { get; set; } = ""; // fold, check, call, bet, raise, all-in
            public long? Amount { get; set; }
        }
    }
}
