using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Infrastructure.Services;
using PokerAPIMPwDB.DTO.Player;

namespace PokerAPIMPwDB.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;
        private readonly IPlayerServiceTable _playerServiceTable;
        private readonly AppDbContext _db;

        public TableController(
            ITableService tableService,
            IPlayerServiceTable playerServiceTable,
            AppDbContext db)
        {
            _tableService = tableService;
            _playerServiceTable = playerServiceTable;
            _db = db;
        }

        // GET: api/table
        [HttpGet]
        public async Task<IActionResult> GetTables()
        {
            var result = await _tableService.GetAllTablesAsync();
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            var tableDtos = result.Value.Select(t => new TableInfoDto
            {
                TableId = t.Id,
                Name = t.Name,
                MaxPlayers = t.MaxPlayers,
                PlayerCount = t.PlayerSeats.Count,
                State = t.Status
            }).ToList();

            return Ok(tableDtos);
        }

        // GET: api/table/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTable(Guid id)
        {
            var result = await _tableService.GetTableByIdAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            var table = result.Value;
            var dto = new TableInfoDto
            {
                TableId = table.Id,
                Name = table.Name,
                MaxPlayers = table.MaxPlayers,
                PlayerCount = table.PlayerSeats.Count,
                State = table.Status
            };
            return Ok(dto);
        }

        // POST: api/table
        [HttpPost]
        public async Task<IActionResult> CreateTable([FromBody] TableInfoDto request)
        {
            var table = new Table
            {
                Name = request.Name,
                MaxPlayers = request.MaxPlayers,
                Status = TableState.Waiting,
                CreatedAt = DateTime.UtcNow,
                PlayerSeats = new System.Collections.Generic.List<PlayerSeat>()
            };

            var result = await _tableService.CreateTableAsync(table,6);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            request.TableId = result.Value.Id;
            request.PlayerCount = 0;
            request.State = result.Value.Status;

            return CreatedAtAction(nameof(GetTable), new { id = result.Value.Id }, request);
        }

        // PUT: api/table/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTable(Guid id, [FromBody] TableInfoDto request)
        {
            var updatedTable = new Table
            {
                Name = request.Name,
                MaxPlayers = request.MaxPlayers
            };

            var result = await _tableService.UpdateTableAsync(id, updatedTable);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return Ok(request);
        }

        // DELETE: api/table/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTable(Guid id)
        {
            var result = await _tableService.DeleteTableAsync(id);
            if (!result.IsSuccess) return NotFound(result.ErrorMessage);

            return NoContent();
        }

        // POST: api/table/{tableId}/join
        [HttpPost("{tableId}/join")]
        public async Task<IActionResult> JoinTable(Guid tableId, int buyInAmount, int seatNumber)
        {
            // Ambil UserId dari JWT
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("UserId not found in token");

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid UserId in token");

            // Ambil user dari DB
            var user = await _db.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found");

            // Join table via service
            var result = await _playerServiceTable.JoinTableAsync(tableId, userId, buyInAmount, seatNumber);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            var player = result.Value;
            // Return sebagai DTO
            var dto = new PlayerPublicStateDto
            {
                PlayerId = player.UserId, // pastikan ini sesuai Domain.Models.Player
                DisplayName = player.DisplayName,
                ChipStack = player.ChipStack,
                State = player.State,
                IsDealer = false // bisa nanti diupdate sesuai posisi dealer
            };

            return Ok(dto);
        }

        // POST: api/table/{tableId}/leave/{playerId}
        [HttpPost("{tableId}/leave/{playerId}")]
        public async Task<IActionResult> LeaveTable(Guid tableId, Guid playerId)
        {
            var result = await _playerServiceTable.LeaveTableAsync(tableId, playerId);
            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            return Ok(new { message = "Left table successfully" });
        }
    }
}
