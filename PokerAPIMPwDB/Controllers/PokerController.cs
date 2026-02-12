using Microsoft.AspNetCore.Mvc;
using PokerAPIMPwDB.Services;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.API.Controllers
{
    [ApiController]
    [Route("api/poker")]
    public class PokerController : ControllerBase
    {
        private readonly GameManager _gameManager;

        public PokerController(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        // ===========================
        // Join Table (load table state)
        // ===========================
        [HttpPost("join")]
        public async Task<ActionResult<ServiceResult>> JoinTable([FromQuery] Guid tableId)
        {
            var result = await _gameManager.PlayerJoinTableAsync(tableId);
            if (!result.IsSuccess) return BadRequest(result);
            return Ok(result);
        }

        // ===========================
        // Sit Down
        // ===========================
        [HttpPost("sit")]
        public async Task<ActionResult<ServiceResult>> SitDown(
            [FromQuery] Guid tableId,
            [FromQuery] int seatIndex,
            [FromQuery] int chips)
        {
            // Ambil userId & displayName dari claim (atau header)
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Unauthorized"));
            var displayName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Player";

            var result = await _gameManager.SitPlayerAsync(tableId, userId, displayName, seatIndex, chips);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // ===========================
        // Stand Up (leave seat)
        // ===========================
        [HttpPost("stand")]
        public async Task<ActionResult<ServiceResult>> StandUp([FromQuery] Guid tableId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Unauthorized"));

            var result = await _gameManager.StandPlayerAsync(tableId, userId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // ===========================
        // Leave Table (full leave)
        // ===========================
        [HttpPost("leave")]
        public async Task<ActionResult<ServiceResult>> LeaveTable([FromQuery] Guid tableId)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? throw new Exception("Unauthorized"));

            var result = await _gameManager.PlayerLeaveTableAsync(tableId, userId);
            if (!result.IsSuccess) return BadRequest(result);

            return Ok(result);
        }

        // ===========================
        // Optional: get table state
        // ===========================
        [HttpGet("state")]
        public async Task<ActionResult<object>> GetTableState([FromQuery] Guid tableId)
        {
            var game = await _gameManager.GetOrCreateGameAsync(tableId);

            return Ok(new
            {
                TableId = tableId,
                Phase = game.Phase.ToString(),
                Seats = game.GetSeatsState(),
                CommunityCards = game.CommunityCards
            });
        }
    }
}
