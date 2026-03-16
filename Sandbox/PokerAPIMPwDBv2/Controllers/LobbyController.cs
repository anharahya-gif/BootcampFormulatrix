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
        public async Task<IActionResult> ListTables()
        {
            var result = await _lobbyService.GetAllTablesAsync();

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Message });

            return Ok(result.Value);
        }
    }
}
