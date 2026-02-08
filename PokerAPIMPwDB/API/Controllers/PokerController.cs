using Microsoft.AspNetCore.Mvc;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.DTO.Actions;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Services;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using PokerAPIMPwDB.DTO.Table;

namespace PokerAPIMPwDB.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PokerController : ControllerBase
    {
        private readonly GameManager _gameManager;
        private readonly AppDbContext _db;

        public PokerController(GameManager gameManager, AppDbContext db)
        {
            _gameManager = gameManager;
            _db = db;
        }

        #region Players

        [HttpGet("table/{tableId}/players")]
        public IActionResult GetPlayers(Guid tableId)
        {
            var players = _gameManager.GetPlayersInTable(tableId)
                .Select(p => new PlayerPublicStateDto
                {
                    PlayerId = p.PlayerId,
                    DisplayName = p.DisplayName,
                    ChipStack = p.ChipStack,
                    CurrentBet = p.CurrentBet,
                    State = p.State
                });
            return Ok(players);
        }

        [HttpPost("table/{tableId}/join")]
        public IActionResult JoinTable(Guid tableId, [FromBody] JoinTableDto dto)
        {
            var result = _gameManager.AddPlayerToTable(tableId, dto.DisplayName, dto.ChipStack, dto.SeatIndex, dto.PlayerId);
            if (!result.IsSuccess) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [HttpDelete("table/{tableId}/players/{playerId}")]
        public IActionResult RemovePlayer(Guid tableId, Guid playerId)
        {
            var result = _gameManager.RemovePlayerFromTable(tableId, playerId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        #endregion

        #region Round Management

        [HttpPost("table/{tableId}/round/start")]
        public IActionResult StartRound(Guid tableId)
        {
            var result = _gameManager.StartRound(tableId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/round/next-phase")]
        public IActionResult NextPhase(Guid tableId)
        {
            var result = _gameManager.NextPhase(tableId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpGet("table/{tableId}/round/state")]
        public IActionResult GetGameState(Guid tableId)
        {
            var state = new
            {
                Phase = _gameManager.GetGameState(tableId),
                CurrentBet = _gameManager.GetCurrentBet(tableId)
            };
            return Ok(state);
        }

        #endregion

        #region Player Actions

        [HttpPost("table/{tableId}/action/bet")]
        public IActionResult Bet(Guid tableId, [FromBody] PlayerActionDto dto)
        {
            var result = _gameManager.HandleBet(tableId, dto.PlayerId, dto.Amount);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/action/fold/{playerId}")]
        public IActionResult Fold(Guid tableId, Guid playerId)
        {
            var result = _gameManager.HandleFold(tableId, playerId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/action/check/{playerId}")]
        public IActionResult Check(Guid tableId, Guid playerId)
        {
            var result = _gameManager.HandleCheck(tableId, playerId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/action/call/{playerId}")]
        public IActionResult Call(Guid tableId, Guid playerId)
        {
            var result = _gameManager.HandleCall(tableId, playerId);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/action/raise/{playerId}")]
        public IActionResult Raise(Guid tableId, Guid playerId, [FromBody] RaiseActionDto dto)
        {
            var result = _gameManager.HandleRaise(tableId, playerId, dto.Amount);
            return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Message);
        }

        [HttpPost("table/{tableId}/action/all-in/{playerId}")]
        public IActionResult AllIn(Guid tableId, Guid playerId)
        {
            var result = _gameManager.HandleAllIn(tableId, playerId);
            return result.IsSuccess ? Ok(result.Message) : BadRequest(result.Message);
        }

        #endregion

        #region Showdown

        [HttpPost("table/{tableId}/showdown")]
        public IActionResult Showdown(Guid tableId)
        {
            var winners = _gameManager.ResolveShowdown(tableId);
            return Ok(new
            {
                Winners = winners.Select(p => p.DisplayName),
                Pot = _gameManager.GetAllGames()[tableId].GetTotalPot()
            });
        }

        [HttpGet("table/{tableId}/showdown/details")]
        public IActionResult ShowdownDetails(Guid tableId)
        {
            var details = _gameManager.GetShowdownDetails(tableId);
            return Ok(details);
        }

        #endregion
    }

    public class RaiseActionDto
    {
        public int Amount { get; set; }
    }
}
