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
        private static readonly GameController _game = new GameController();

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
            if (_game.PlayerMap.Keys.Any(p => p.Name == name))
                return BadRequest("Player name already exists");

            var player = new Player(name, chips);
            _game.AddPlayer(player);

            return Ok(new
            {
                success = true,
                players = _game.PlayerMap.Keys.Select(p => p.Name)
            });
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
            if (_game.PlayerMap.Count < 2)
                return BadRequest("Minimum 2 players required");

            _game.StartRound();

            var players = _game.PlayerMap.Select(kv =>
            {
                var player = kv.Key;
                var status = kv.Value;

                return new
                {
                    name = player.Name,
                    chipStack = player.ChipStack,
                    handCount = status.Hand.Count
                };
            });

            return Ok(new
            {
                phase = _game.Phase.ToString(),
                deckRemaining = _game.Deck.RemainingCards(),
                totalPlayers = _game.PlayerMap.Count,
                expectedDeckRemaining = 52 - (_game.PlayerMap.Count * 2),
                players
            });
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
        [HttpGet("showdown")]
        public IActionResult Showdown()
        {
            var winners = _game.DetermineWinners();
            if (!winners.Any())
                return BadRequest("No winners");

            int pot = _game.Pot.TotalChips;
            int share = pot / winners.Count;

            foreach (var winner in winners)
                winner.ChipStack += share;

            _game.Pot.Reset();

            return Ok(new
            {
                winners = winners.Select(p => p.Name),
                potShare = share,
                communityCards = _game.CommunityCards
                    .Select(c => $"{c.Rank} of {c.Suit}")
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
                phase = _game.Phase.ToString(),
                currentPlayer = currentPlayer?.Name,
                currentBet = _game.CurrentBet,
                pot = _game.Pot.TotalChips,
                communityCards = _game.CommunityCards
                    .Select(c => $"{c.Rank} of {c.Suit}"),
                players
            });
        }

    }
}
