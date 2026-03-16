using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PokerAPIMultiplayerWithDB.Data;
using PokerAPIMultiplayerWithDB.Hubs;
using PokerAPIMultiplayerWithDB.Models;
using PokerAPIMultiplayerWithDB.Services;

namespace PokerAPIMultiplayerWithDB.Controllers
{
    [ApiController]
    [Route("api/table")]
    public class TableController : ControllerBase
    {
        private readonly PokerDbContext _db;
        private readonly IHubContext<LobbyHub> _hub;
        private readonly ITableGameService _gameService;
        private readonly IHubContext<GameHub> _gameHub;

        public TableController(
            PokerDbContext db, 
            IHubContext<LobbyHub> hub,
            ITableGameService gameService,
            IHubContext<GameHub> gameHub)
        {
            _db = db;
            _hub = hub;
            _gameService = gameService;
            _gameHub = gameHub;
            
            // Wire up game service events to broadcast via SignalR
            gameService.GameStateUpdated += async (tableId, state) =>
            {
                await gameHub.Clients.Group($"table-{tableId}").SendAsync("GameStateUpdated", new
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
                        IsAllIn = kv.Value.IsAllIn,
                        HasActed = kv.Value.HasActed
                    }).ToList()
                });
            };
            
            gameService.GameEventOccurred += async (tableId, eventMessage) =>
            {
                await gameHub.Clients.Group($"table-{tableId}").SendAsync("GameEventOccurred", new 
                { 
                    message = eventMessage, 
                    timestamp = DateTime.UtcNow 
                });
            };
        }

        [HttpGet("tables")]
        public async Task<IActionResult> GetTables()
        {
            var tables = await _db.Tables
                .Include(t => t.PlayerAtTables.Where(p => p.LeftAt == null))
                .ToListAsync();
            
            var dto = tables.Select(t => new
            {
                t.Id,
                t.TableName,
                t.Status,
                t.MinBuyIn,
                t.MaxBuyIn,
                t.MaxPlayers,
                CurrentPlayers = t.PlayerAtTables?.Count(p => p.LeftAt == null) ?? 0
            });
            return Ok(dto);
        }

        public class CreateTableRequest 
        { 
            public string TableName { get; set; } = "";
            public long MinBuyIn { get; set; } 
            public long MaxBuyIn { get; set; } 
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTableRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.TableName)) 
                return BadRequest("TableName required");
            if (req.MinBuyIn <= 0 || req.MaxBuyIn <= 0 || req.MinBuyIn > req.MaxBuyIn) 
                return BadRequest("Invalid buyin range");

            var table = new Table
            {
                TableName = req.TableName,
                MinBuyIn = req.MinBuyIn,
                MaxBuyIn = req.MaxBuyIn,
                MaxPlayers = 10,
                Status = TableStatus.Open
            };

            _db.Tables.Add(table);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("TableCreated", new 
            { 
                TableId = table.Id, 
                table.TableName, 
                table.MinBuyIn, 
                table.MaxBuyIn, 
                table.MaxPlayers 
            });

            return Ok(new { table.Id });
        }

        public class JoinTableRequest 
        { 
            public int TableId { get; set; } 
            public int SeatNumber { get; set; } 
            public long ChipDeposit { get; set; } 
        }

        [Authorize]
        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinTableRequest req)
        {
            var playerId = int.Parse(User.Claims.Single(c => c.Type == "playerId").Value);
            var player = await _db.Players.FindAsync(playerId);
            if (player == null) 
                return Unauthorized();

            var table = await _db.Tables
                .Include(t => t.PlayerAtTables)
                .SingleOrDefaultAsync(t => t.Id == req.TableId);
            if (table == null) 
                return NotFound("Table not found");
            if (req.SeatNumber < 1 || req.SeatNumber > table.MaxPlayers) 
                return BadRequest("Invalid seat number");
            if (table.PlayerAtTables.Any(p => p.SeatNumber == req.SeatNumber && p.LeftAt == null)) 
                return Conflict("Seat already taken");
            if (req.ChipDeposit < table.MinBuyIn || req.ChipDeposit > table.MaxBuyIn) 
                return BadRequest("Chip deposit not within table limits");
            if (player.ChipBalance < req.ChipDeposit) 
                return BadRequest("Insufficient chips");

            player.ChipBalance -= req.ChipDeposit;

            var pat = new PlayerAtTable
            {
                PlayerId = player.Id,
                TableId = table.Id,
                SeatNumber = req.SeatNumber,
                ChipDeposit = req.ChipDeposit,
                JoinedAt = DateTime.UtcNow
            };

            _db.PlayerAtTables.Add(pat);
            _db.GameLogs.Add(new GameLog 
            { 
                TableId = table.Id, 
                Action = "PlayerJoined", 
                DetailsJson = System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    player.Id, 
                    player.Username, 
                    Seat = req.SeatNumber, 
                    ChipDeposit = req.ChipDeposit 
                })
            });
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("PlayerJoined", new 
            { 
                TableId = table.Id, 
                PlayerId = player.Id, 
                Username = player.Username, 
                SeatNumber = pat.SeatNumber 
            });

            return Ok(new { message = "joined", pat.Id });
        }

        public class LeaveRequest 
        { 
            public int TableId { get; set; } 
        }

        [Authorize]
        [HttpPost("leave")]
        public async Task<IActionResult> Leave([FromBody] LeaveRequest req)
        {
            var playerId = int.Parse(User.Claims.Single(c => c.Type == "playerId").Value);
            var pat = await _db.PlayerAtTables
                .Include(p => p.Table)
                .Include(p => p.Player)
                .SingleOrDefaultAsync(p => p.TableId == req.TableId && p.PlayerId == playerId && p.LeftAt == null);
            if (pat == null) 
                return NotFound("Player not at table");

            // Leave rules: cannot leave if table in progress and player hasn't folded
            if (pat.Table.Status == TableStatus.InProgress && !pat.HasFolded) 
                return BadRequest("Cannot leave during in-progress hand unless folded");

            pat.LeftAt = DateTime.UtcNow;

            // restore chips (for simplicity assume full deposit returned unless in-progress losses tracked elsewhere)
            var player = pat.Player;
            player.ChipBalance += pat.ChipDeposit;

            _db.GameLogs.Add(new GameLog 
            { 
                TableId = pat.TableId, 
                Action = "PlayerLeft", 
                DetailsJson = System.Text.Json.JsonSerializer.Serialize(new 
                { 
                    player.Id, 
                    player.Username, 
                    Seat = pat.SeatNumber, 
                    Restored = pat.ChipDeposit 
                })
            });
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("PlayerLeft", new 
            { 
                TableId = pat.TableId, 
                PlayerId = player.Id, 
                Username = player.Username, 
                SeatNumber = pat.SeatNumber 
            });

            return Ok(new { message = "left" });
        }
    }
}
