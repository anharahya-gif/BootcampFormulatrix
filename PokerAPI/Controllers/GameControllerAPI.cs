using Microsoft.AspNetCore.Mvc;
using PokerAPI.Models;
using PokerAPI.Services;
using System.Linq;

namespace PokerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameControllerAPI : ControllerBase
    {
        private readonly GameController _game;
        private int _roundStartedCount = 0;


        public GameControllerAPI(GameController game)
        {
            _game = game;


        }


        private void OnRoundStarted()
        {
            Console.WriteLine("Round started event received");
            _roundStartedCount++;
            Console.WriteLine($"Round started event triggered {_roundStartedCount} time(s)");
        }




        // ======================
        // Helper
        // ======================
        private void AdvanceTurnIfNeeded()
        {
            if (_game.IsBettingRoundOver())
                _game.NextPhase();
            else
                _game.GetNextActivePlayer();
        }

        // ======================
        // Player Management
        // ======================
        [HttpPost("addPlayer")]
        public IActionResult AddPlayer([FromQuery] string name, [FromQuery] int chips = 1000)
        {
            // if (_game.PlayerMap.Keys.Any(p => p.Name == name))
            //     return BadRequest("Player name already exists");

            // var player = new Player(name, chips);
            // _game.AddPlayer(player);

            // return Ok(new
            // {
            //     success = true,
            //     players = _game.PlayerMap.Keys.Select(p => p.Name),
            //     totalPlayers = _game.PlayerMap.Count,
            //     maxPlayers = 10
            // });
            try
            {
                if (_game.PlayerMap.Keys.Any(p => p.Name == name))
                    return BadRequest("Player name already exists");

                var player = new Player(name, chips);
                _game.AddPlayer(player);

                return Ok(new
                {
                    success = true,
                    totalPlayers = _game.PlayerMap.Count,
                    maxPlayers = 10
                });
            }
            catch (InvalidOperationException ex)
            {
                // contoh: "Table is full (max 10 players)"
                return BadRequest(new
                {
                    success = false,
                    message = ex.Message,
                    totalPlayers = _game.PlayerMap.Count,
                    maxPlayers = 10
                });
            }
        }

        [HttpPost("removePlayer")]
        public IActionResult RemovePlayer([FromQuery] string name)
        {
            var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
            if (player == null)
                return NotFound("Player not found");

            _game.RemovePlayer(player);
            return Ok(new { success = true });
        }

        // ======================
        // Round Management
        // ======================
        [HttpPost("startRound")]
        public IActionResult StartRound()
        {
            try
            {
                _game.RoundStarted -= OnRoundStarted;
                _game.RoundStarted += OnRoundStarted;

                _game.StartRound();

                _game.RoundStarted -= OnRoundStarted;

                var players = _game.PlayerMap.Select(kv =>
                {
                    var player = kv.Key;
                    var status = kv.Value;

                    return new
                    {
                        name = player.Name,
                        chipStack = player.ChipStack,
                        handCount = status.Hand.Count,
                        // isFolded = status.IsFolded,
                        // isAllIn = status.IsAllIn
                        // ⚠️ kartu TIDAK dikirim di sini
                    };
                });

                return Ok(new
                {
                    phase = _game.Phase.ToString(),
                    // pot = _game.Pot.Amount,
                    currentBet = _game.CurrentBet,
                    // currentPlayer = _game.CurrentPlayer?.Name,
                    deckRemaining = _game.Deck.RemainingCards(),
                    totalPlayers = _game.PlayerMap.Count,
                    expectedDeckRemaining = 52 - (_game.PlayerMap.Count * 2),
                    players
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            // if (_game.PlayerMap.Count < 2)
            //     return BadRequest("Minimum 2 players required");
            // _game.RoundStarted -= OnRoundStarted;
            // _game.RoundStarted += OnRoundStarted;
            // _game.StartRound();
            // _game.RoundStarted -= OnRoundStarted;

            // var players = _game.PlayerMap.Select(kv =>
            // {
            //     var player = kv.Key;
            //     var status = kv.Value;

            //     return new
            //     {
            //         name = player.Name,
            //         chipStack = player.ChipStack,
            //         handCount = status.Hand.Count
            //     };
            // });


            // return Ok(new
            // {
            //     phase = _game.Phase.ToString(),
            //     deckRemaining = _game.Deck.RemainingCards(),
            //     totalPlayers = _game.PlayerMap.Count,
            //     expectedDeckRemaining = 52 - (_game.PlayerMap.Count * 2),
            //     players
            // });

        }


        [HttpPost("nextPhase")]
        public IActionResult NextPhase()
        {
            _game.NextPhase();

            return Ok(new
            {
                phase = _game.Phase.ToString(),
                communityCards = _game.CommunityCards
                    .Select(c => $"{c.Rank} of {c.Suit}")
            });
        }

        // ======================
        // Betting Actions
        // ======================
        [HttpPost("bet")]
        public IActionResult Bet([FromQuery] string name, [FromQuery] int amount)
        {
            if (amount <= 0)
                return BadRequest("Amount must be greater than zero");
            try
            {
                var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
                if (player == null)
                    return NotFound("Player not found");

                bool success = _game.HandleBet(player, amount);
                if (success) AdvanceTurnIfNeeded();

                return Ok(new
                {
                    success,
                    player = player.Name,
                    chipStack = player.ChipStack,
                    currentBet = _game.CurrentBet,
                    pot = _game.Pot.TotalChips
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("call")]
        public IActionResult Call([FromQuery] string name)
        {
            var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
            if (player == null)
                return NotFound("Player not found");

            bool success = _game.HandleCall(player);
            if (success) AdvanceTurnIfNeeded();

            return Ok(new
            {
                success,
                player = player.Name,
                chipStack = player.ChipStack,
                pot = _game.Pot.TotalChips
            });
        }

        [HttpPost("raise")]
        public IActionResult Raise([FromQuery] string name, [FromQuery] int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Raise amount must be greater than zero");
            try
            {


                var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
                if (player == null)
                    return NotFound("Player not found");

                bool success = _game.HandleRaise(player, amount);
                if (success) AdvanceTurnIfNeeded();

                return Ok(new
                {
                    success,
                    player = player.Name,
                    chipStack = player.ChipStack,
                    currentBet = _game.CurrentBet,
                    pot = _game.Pot.TotalChips
                });
            }

            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("check")]
        public IActionResult Check([FromQuery] string name)
        {
            var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
            if (player == null)
                return NotFound("Player not found");

            _game.HandleCheck(player);
            AdvanceTurnIfNeeded();

            return Ok(new
            {
                success = true,
                player = player.Name
            });
        }

        [HttpPost("fold")]
        public IActionResult Fold([FromQuery] string name)
        {
            if (_game.GetGameState() != "InProgress")
                return BadRequest("Fold is not available in current state");
            var player = _game.PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
            if (player == null)
                return NotFound("Player not found");

            _game.HandleFold(player);
            AdvanceTurnIfNeeded();

            return Ok(new
            {
                success = true,
                player = player.Name
            });
        }

        // ======================
        // Showdown
        // ======================
        [HttpPost("showdown")]
        public IActionResult Showdown()
        {
            if (_game.Phase != GamePhase.Showdown)
                return BadRequest("Showdown is not available in current phase");
            var (winners, rank) = _game.ResolveShowdownDetailed();

            if (!winners.Any())
                return BadRequest("No winners");

            bool isSplit = winners.Count > 1;

            return Ok(new
            {
                result = isSplit ? "SplitPot" : "Win",
                handRank = rank.ToString(),

                winners = winners.Select(p => new
                {
                    name = p.Name,
                    handRank = rank.ToString()
                }),

                message = isSplit
                    ? $"Split pot: all winners have {rank}"
                    : $"{winners.First().Name} wins with {rank}"
            });
        }

        // =====================
        // All-Inn
        // =====================
        [HttpPost("allin")]
        public IActionResult AllIn([FromQuery] string name)
        {
            bool success = _game.HandleAllIn(name);

            if (!success)
                return BadRequest("Player tidak bisa all-in");

            return Ok(new
            {
                success = true,
                state = new
                {
                    phase = _game.Phase.ToString(),
                    currentBet = _game.CurrentBet,
                    pot = _game.Pot.TotalChips,
                    players = _game.PlayerMap.Select(kv => new
                    {
                        name = kv.Key.Name,
                        chipStack = kv.Key.ChipStack,
                        state = kv.Value.State.ToString(),
                        currentBet = kv.Value.CurrentBet
                    })
                }
            });
        }


        // =====================
        // State
        // =====================
            [HttpGet("state")]
            public IActionResult State()
            {
                var players = _game.PlayerMap.Select(kv =>
                {
                    var player = kv.Key;
                    var status = kv.Value;

                    return new
                    {   
                        seatIndex = player.SeatIndex,                        
                        name = player.Name,
                        chipStack = player.ChipStack,
                        state = status.State.ToString(),
                        currentBet = status.CurrentBet,
                        hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}")
                    };
                });

                var currentPlayer = _game.GetCurrentPlayer();

                return Ok(new
                {
                    gameState = _game.GetGameState(),
                    phase = _game.Phase.ToString(),
                    currentPlayer = currentPlayer?.Name,
                    currentBet = _game.CurrentBet,
                    pot = _game.Pot.TotalChips,
                    communityCards = _game.CommunityCards
                        .Select(c => $"{c.Rank} of {c.Suit}"),

                    players,

                    showdown = _game.LastShowdown is null
                    ? null
                    : new
                    {
                        winners = _game.LastShowdown.Winners.Select(p => p.Name),
                        handRank = _game.LastShowdown.HandRank.ToString(),
                        message = _game.LastShowdown.Message

                    }
                });
            }


    }
}
