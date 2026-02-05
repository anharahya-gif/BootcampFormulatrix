// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.SignalR;
// using PokerAPI.Models;
// using PokerAPI.Services.Interfaces;
// using System.Linq;
// using PokerAPI.Hubs;

// namespace PokerAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class GameControllerAPI : ControllerBase
//     {
//         private readonly IGameController _game;
//         private readonly IHubContext<PokerHub> _hub;
//         private int _roundStartedCount = 0;


//         public GameControllerAPI(IGameController game, IHubContext<PokerHub> hub)
//         {
//             _game = game;
//             _hub = hub;


//         }


//         private void OnRoundStarted()
//         {
//             Console.WriteLine("Round started event received");
//             _roundStartedCount++;
//             Console.WriteLine($"Round started event triggered {_roundStartedCount} time(s)");
//         }

//         // ======================
//         // buat BroadsCastGameStateDto
//         // ======================
//         private object BuildGameStateDto()
//         {
//             return new
//             {
//                 gameState = _game.GetGameState(),
//                 phase = _game.Phase.ToString(),
//                 currentPlayer = _game.GetCurrentPlayer()?.Name,
//                 currentBet = _game.CurrentBet,
//                 pot = _game.GetTotalPot(),

//                 communityCards = _game.CommunityCards
//                     .Select(c => $"{c.Rank} of {c.Suit}")
//                     .ToList(),

//                 players = _game.GetPlayersPublicState().ToList(),

//                 showdown = _game.LastShowdown == null
//                     ? null
//                     : new
//                     {
//                         winners = _game.LastShowdown.Winners.Select(p => p.Name),
//                         handRank = _game.LastShowdown.HandRank.ToString(),
//                         message = _game.LastShowdown.Message
//                     }
//             };
//         }

//         // ======================
//         // BroadsCastGameState Ke Client
//         // ======================
//         private async Task BroadcastGameState()
//         {
//             await _hub.Clients.All.SendAsync(
//                     "ReceiveGameState",
//                     BuildGameStateDto()
//                     );
//         }




//         // ======================
//         // Helper
//         // ======================
//         private void AdvanceTurnIfNeeded()
//         {
//             if (_game.IsBettingRoundOver())
//                 _game.NextPhase();
//             else
//                 _game.GetNextActivePlayer();
//         }

//         // ======================
//         // Player Management
//         // ======================
//         [HttpPost("addPlayer")]
//         public async Task<IActionResult> AddPlayer([FromQuery] string name, [FromQuery] int chips = 1000, [FromQuery] int seatIndex = -1)
//         {
//             try
//             {
//                 var player = _game.GetPlayerByName(name);
//                 if (player == null)
//                     return NotFound("Player not found");

//                 // ===========================
//                 // Buat player dulu
//                 // ===========================
//                 var player = new Player(name, chips);

//                 // ===========================
//                 // Assign seatIndex dari frontend
//                 // ===========================
//                 player.SeatIndex = seatIndex;

//                 // ===========================
//                 // Tambahkan player ke map
//                 // ===========================
//                 _game.AddPlayer(player);
//                 await BroadcastGameState();


//                 return Ok(new
//                 {
//                     success = true,
//                     totalPlayers = _game.PlayerMap.Count,
//                     maxPlayers = 10
//                 });
//             }
//             catch (InvalidOperationException ex)
//             {
//                 return BadRequest(new
//                 {
//                     success = false,
//                     message = ex.Message,
//                     totalPlayers = _game.PlayerMap.Count,
//                     maxPlayers = 10
//                 });
//             }
//         }


//         [HttpPost("addchips")]
//         public async Task<IActionResult> AddChips([FromBody] AddChipsRequest request)
//         {
//             // Validasi phase game
//             if (_game.GetGameState() == "InProgress")
//                 return BadRequest("Tidak bisa menambahkan chip saat game sedang berjalan.");

//             // Cari player di PlayerMap
//             var kv = _game.PlayerMap.FirstOrDefault(p => p.Key.Name == request.PlayerName);
//             if (kv.Key == null)
//                 return NotFound("Player tidak ditemukan.");

//             var player = kv.Key;

//             // Validasi jumlah chip
//             if (request.Amount <= 0)
//                 return BadRequest("Jumlah chip harus lebih dari 0.");

//             // Tambahkan chip
//             player.ChipStack += request.Amount;
//             await BroadcastGameState();

//             return Ok(new
//             {
//                 PlayerName = player.Name,
//                 NewChips = player.ChipStack,
//                 Message = $"{request.Amount} chip berhasil ditambahkan."
//             });
//         }

//         [HttpPost("removePlayer")]
//         public async Task<IActionResult> RemovePlayer([FromQuery] string name)
//         {
//             var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//             if (player == null)
//                 return NotFound("Player not found");

//             _game.RemovePlayer(player);
//             await BroadcastGameState();
//             return Ok(new { success = true });
//         }

//         // ======================
//         // Round Management
//         // ======================
//         [HttpPost("startRound")]
//         public async Task<IActionResult> StartRound()
//         {
//             try
//             {
//                 _game.RoundStarted -= OnRoundStarted;
//                 _game.RoundStarted += OnRoundStarted;

//                 _game.StartRound();

//                 _game.RoundStarted -= OnRoundStarted;

//                 var players = _game.PlayerMap.Select(kv =>
//                 {
//                     var player = kv.Key;
//                     var status = kv.Value;

//                     return new
//                     {
//                         name = player.Name,
//                         chipStack = player.ChipStack,
//                         handCount = status.Hand.Count,
//                         // isFolded = status.IsFolded,
//                         // isAllIn = status.IsAllIn
//                         // ⚠️ kartu TIDAK dikirim di sini
//                     };
//                 });
//                 await BroadcastGameState();

//                 return Ok(new
//                 {
//                     phase = _game.Phase.ToString(),
//                     // pot = _game.Pot.Amount,
//                     currentBet = _game.CurrentBet,
//                     // currentPlayer = _game.CurrentPlayer?.Name,
//                     deckRemaining = _game.Deck.RemainingCards(),
//                     totalPlayers = _game.PlayerMap.Count,
//                     expectedDeckRemaining = 52 - (_game.PlayerMap.Count * 2),
//                     players
//                 });
//             }
//             catch (InvalidOperationException ex)
//             {
//                 return BadRequest(ex.Message);
//             }
//             // if (_game.PlayerMap.Count < 2)
//             //     return BadRequest("Minimum 2 players required");
//             // _game.RoundStarted -= OnRoundStarted;
//             // _game.RoundStarted += OnRoundStarted;
//             // _game.StartRound();
//             // _game.RoundStarted -= OnRoundStarted;

//             // var players = _game.PlayerMap.Select(kv =>
//             // {
//             //     var player = kv.Key;
//             //     var status = kv.Value;

//             //     return new
//             //     {
//             //         name = player.Name,
//             //         chipStack = player.ChipStack,
//             //         handCount = status.Hand.Count
//             //     };
//             // });


//             // return Ok(new
//             // {
//             //     phase = _game.Phase.ToString(),
//             //     deckRemaining = _game.Deck.RemainingCards(),
//             //     totalPlayers = _game.PlayerMap.Count,
//             //     expectedDeckRemaining = 52 - (_game.PlayerMap.Count * 2),
//             //     players
//             // });

//         }


//         [HttpPost("nextPhase")]
//         public async Task<IActionResult> NextPhase()
//         {
//             _game.NextPhase();
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 phase = _game.Phase.ToString(),
//                 communityCards = _game.CommunityCards
//                     .Select(c => $"{c.Rank} of {c.Suit}")
//             });
//         }

//         // ======================
//         // Betting Actions
//         // ======================
//         [HttpPost("bet")]
//         public async Task<IActionResult> Bet([FromQuery] string name, [FromQuery] int amount)
//         {
//             if (amount <= 0)
//                 return BadRequest("Amount must be greater than zero");
//             try
//             {
//                 var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//                 if (player == null)
//                     return NotFound("Player not found");

//                 bool success = _game.HandleBet(player, amount);
//                 if (success) AdvanceTurnIfNeeded();
//                 await BroadcastGameState();
//                 return Ok(new
//                 {
//                     success,
//                     player = player.Name,
//                     chipStack = player.ChipStack,
//                     currentBet = _game.CurrentBet,
//                     pot = _game.Pot.TotalChips
//                 });
//             }
//             catch (InvalidOperationException ex)
//             {
//                 return BadRequest(ex.Message);
//             }
//         }

//         [HttpPost("call")]
//         public async Task<IActionResult> Call([FromQuery] string name)
//         {
//             var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//             if (player == null)
//                 return NotFound("Player not found");

//             bool success = _game.HandleCall(player);
//             if (success) AdvanceTurnIfNeeded();
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 success,
//                 player = player.Name,
//                 chipStack = player.ChipStack,
//                 pot = _game.Pot.TotalChips
//             });
//         }

//         [HttpPost("raise")]
//         public async Task<IActionResult> Raise([FromQuery] string name, [FromQuery] int amount)
//         {
//             if (amount <= 0)
//                 throw new InvalidOperationException("Raise amount must be greater than zero");
//             try
//             {


//                 var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//                 if (player == null)
//                     return NotFound("Player not found");

//                 bool success = _game.HandleRaise(player, amount);
//                 if (success) AdvanceTurnIfNeeded();
//                 await BroadcastGameState();
//                 return Ok(new
//                 {
//                     success,
//                     player = player.Name,
//                     chipStack = player.ChipStack,
//                     currentBet = _game.CurrentBet,
//                     pot = _game.Pot.TotalChips
//                 });
//             }

//             catch (InvalidOperationException ex)
//             {
//                 return BadRequest(ex.Message);
//             }
//         }

//         [HttpPost("check")]
//         public async Task<IActionResult> Check([FromQuery] string name)
//         {
//             var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//             if (player == null)
//                 return NotFound("Player not found");

//             _game.HandleCheck(player);
//             AdvanceTurnIfNeeded();
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 success = true,
//                 player = player.Name
//             });
//         }

//         [HttpPost("fold")]
//         public async Task<IActionResult> Fold([FromQuery] string name)
//         {
//             if (_game.GetGameState() != "InProgress")
//                 return BadRequest("Fold is not available in current state");
//             var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
//             if (player == null)
//                 return NotFound("Player not found");

//             _game.HandleFold(player);
//             AdvanceTurnIfNeeded();
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 success = true,
//                 player = player.Name
//             });
//         }

//         // ======================
//         // Showdown
//         // ======================
//         [HttpPost("showdown")]
//         public async Task<IActionResult> Showdown()
//         {
//             if (_game.Phase != GamePhase.Showdown)
//                 return BadRequest("Showdown is not available in current phase");
//             var (winners, rank) = _game.ResolveShowdownDetailed();

//             if (!winners.Any())
//                 return BadRequest("No winners");

//             bool isSplit = winners.Count > 1;
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 result = isSplit ? "SplitPot" : "Win",
//                 handRank = rank.ToString(),

//                 winners = winners.Select(p => new
//                 {
//                     name = p.Name,
//                     handRank = rank.ToString()
//                 }),

//                 message = isSplit
//                     ? $"Split pot: all winners have {rank}"
//                     : $"{winners.First().Name} wins with {rank}"
//             });
//         }

//         // =====================
//         // All-Inn
//         // =====================
//         [HttpPost("allin")]
//         public async Task<IActionResult> AllIn([FromQuery] string name)
//         {
//             bool success = _game.HandleAllIn(name);

//             if (!success)
//                 return BadRequest("Player tidak bisa all-in");
//             await BroadcastGameState();
//             return Ok(new
//             {
//                 success = true,
//                 state = new
//                 {
//                     phase = _game.Phase.ToString(),
//                     currentBet = _game.CurrentBet,
//                     pot = _game.Pot.TotalChips,
//                     players = _game.PlayerMap.Select(kv => new
//                     {
//                         name = kv.Key.Name,
//                         chipStack = kv.Key.ChipStack,
//                         state = kv.Value.State.ToString(),
//                         currentBet = kv.Value.CurrentBet
//                     })
//                 }
//             });
//         }


//         // =====================
//         // State
//         // =====================
//         [HttpGet("state")]
//         public async Task<IActionResult> State()
//         {
//             // var players = _game.PlayerMap.Select(kv =>
//             // {
//             //     var player = kv.Key;
//             //     var status = kv.Value;

//             //     return new
//             //     {
//             //         seatIndex = player.SeatIndex,
//             //         name = player.Name,
//             //         chipStack = player.ChipStack,
//             //         state = status.State.ToString(),
//             //         currentBet = status.CurrentBet,
//             //         hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}")
//             //     };
//             // });

//             // var currentPlayer = _game.GetCurrentPlayer();
//             // await BroadcastGameState();
//             // return Ok(new
//             // {
//             //     gameState = _game.GetGameState(),
//             //     phase = _game.Phase.ToString(),
//             //     currentPlayer = currentPlayer?.Name,
//             //     currentBet = _game.CurrentBet,
//             //     pot = _game.Pot.TotalChips,
//             //     communityCards = _game.CommunityCards
//             //         .Select(c => $"{c.Rank} of {c.Suit}"),

//             //     players,

//             //     showdown = _game.LastShowdown is null
//             //     ? null
//             //     : new
//             //     {
//             //         winners = _game.LastShowdown.Winners.Select(p => p.Name),
//             //         handRank = _game.LastShowdown.HandRank.ToString(),
//             //         message = _game.LastShowdown.Message

//             //     }
//             // });
//             return Ok(BuildGameStateDto());
//         }


//     }
// }
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
                    .Select(c => $"{c.Rank} of {c.Suit}"),
                players = _game.GetPlayersPublicState(),
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
        // Tambah Chips
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
        // Round
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
        // Betting
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

            bool success = _game.HandleAllIn(player.Name); // pastikan method di service return bool
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
        // State
        // ======================
        [HttpGet("state")]
        public IActionResult State()
        {
            return Ok(BuildGameStateDto());
        }
    }
}
