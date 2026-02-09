using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PokerAPI.Hubs;
using PokerAPI.Services.Interfaces;
using PokerAPI.DTOs;
using PokerAPI.Services; 
using PokerAPI.Mapper;
using PokerAPI.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace PokerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameControllerAPI : ControllerBase
    {
        private readonly IGameService _game;
        private readonly IHubContext<PokerHub> _hub;

        public GameControllerAPI(IGameService game, IHubContext<PokerHub> hub)
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
                object details = _game.GetShowdownDetails();
                _ = _hub.Clients.All.SendAsync("ShowdownCompleted", details);
            };
        }

        #region Internal Helpers
        private void AdvanceTurnIfNeeded()
        {
            if (_game.IsBettingRoundOver())
            {
                _game.NextPhase();
            }
            else
            {
                _game.GetNextActivePlayer();
            }
        }

        private async Task BroadcastGameState()
        {
            GameStateDto gameState = GameMapper.MapToGameStateDto(_game);
            await _hub.Clients.All.SendAsync("ReceiveGameState", gameState);
        }
        #endregion

        #region Player Management
        [HttpPost("addPlayer")]
        public async Task<IActionResult> AddPlayer([FromQuery] string name, [FromQuery] int chips = 1000, [FromQuery] int seatIndex = -1)
        {
            ServiceResult result = _game.AddPlayer(name, chips, seatIndex);
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("registerPlayer")]
        public async Task<IActionResult> RegisterPlayer([FromQuery] string playerName, [FromQuery] int chipStack)
        {
            ServiceResult result = _game.RegisterPlayer(playerName, chipStack);
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("joinSeat")]
        public async Task<IActionResult> JoinSeat([FromQuery] string playerName, [FromQuery] int seatIndex)
        {
            ServiceResult result = _game.UpdatePlayerSeat(playerName, seatIndex);
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("removePlayer")]
        public async Task<IActionResult> RemovePlayer([FromQuery] string name)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.RemovePlayer(player);
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }
        #endregion

        #region Chips Management
        [HttpPost("addchips")]
        public async Task<IActionResult> AddChips([FromBody] AddChipsRequest request)
        {
            IPlayer? player = _game.GetPlayerByName(request.PlayerName);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            if (request.Amount <= 0)
            {
                ServiceResult resultFail = ServiceResult.Failure("Amount must be greater than 0");
                return BadRequest(resultFail);
            }

            player.ChipStack += request.Amount;
            await BroadcastGameState();

            ServiceResult result = ServiceResult.Success("Chips added successfully");
            return Ok(result);
        }
        #endregion

        #region Round Management
        [HttpPost("startRound")]
        public async Task<IActionResult> StartRound()
        {
            ServiceResult result = _game.StartRound();
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpPost("nextPhase")]
        public async Task<IActionResult> NextPhase()
        {
            ServiceResult result = _game.NextPhase();
            if (result.IsSuccess)
            {
                await BroadcastGameState();
                return Ok(result);
            }
            return BadRequest(result);
        }
        #endregion

        #region Betting Actions
        [HttpPost("bet")]
        public async Task<IActionResult> Bet([FromQuery] string name, [FromQuery] int amount)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.HandleBet(player, amount);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("call")]
        public async Task<IActionResult> Call([FromQuery] string name)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.HandleCall(player);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("raise")]
        public async Task<IActionResult> Raise([FromQuery] string name, [FromQuery] int amount)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.HandleRaise(player, amount);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("check")]
        public async Task<IActionResult> Check([FromQuery] string name)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.HandleCheck(player);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("fold")]
        public async Task<IActionResult> Fold([FromQuery] string name)
        {
            IPlayer? player = _game.GetPlayerByName(name);
            if (player == null)
            {
                ServiceResult resultFail = ServiceResult.Failure("Player not found");
                return NotFound(resultFail);
            }

            ServiceResult result = _game.HandleFold(player);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("allin")]
        public async Task<IActionResult> AllIn([FromQuery] string name)
        {
            ServiceResult result = _game.HandleAllIn(name);
            await BroadcastGameState();
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        #endregion

        #region Showdown & Reset
        [HttpPost("showdown")]
        public async Task<IActionResult> Showdown()
        {
            (List<IPlayer> winners, HandRank rank) showdownResult = _game.ResolveShowdownDetailed();
            await BroadcastGameState();
            ServiceResult result = ServiceResult.Success("Showdown resolved");
            return Ok(result);
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset()
        {
            ServiceResult result = _game.ResetGame();
            await BroadcastGameState();
            return Ok(result);
        }
        #endregion

        #region Game State
        [HttpGet("state")]
        public IActionResult State()
        {
            GameStateDto gameState = GameMapper.MapToGameStateDto(_game);
            return Ok(gameState);
        }
        #endregion
    }
}
