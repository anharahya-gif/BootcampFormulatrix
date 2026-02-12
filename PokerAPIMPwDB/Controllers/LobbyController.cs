using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PokerAPIMPwDB.Infrastructure.Services;
using PokerAPIMPwDB.DTO.Table;
using System.Linq;

namespace PokerAPIMPwDB.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LobbyController : ControllerBase
    {
        private readonly LobbyService _lobbyService;

        public LobbyController(LobbyService lobbyService)
        {
            _lobbyService = lobbyService;
        }

        // GET: api/lobby
        [HttpGet]
        public IActionResult ListTables()
        {
            var result = _lobbyService.GetAllTables();

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            var tablesDto = result.Value!.Select(t => new TableInfoDto
            {
                TableId = t.TableId,
                Name = t.Name,
                MaxPlayers = t.MaxPlayers,
                PlayerCount = t.PlayerCount,
                SmallBlind = t.SmallBlind,
                BigBlind = t.BigBlind,
                State = t.State
            }).ToList();

            return Ok(tablesDto);
        }
    }
}
