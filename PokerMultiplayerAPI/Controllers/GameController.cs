using Microsoft.AspNetCore.Mvc;
using PokerMultiplayerAPI.Domain.Interfaces;
using PokerMultiplayerAPI.Shared.DTOs;

namespace PokerMultiplayerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameService _gameService;

    public GameController(IGameService gameService)
    {
        _gameService = gameService;
    }

    [HttpPost("{tableId}/join")]
    public async Task<IActionResult> JoinTable(Guid tableId, [FromBody] JoinTableRequest request)
    {
        try
        {
            var playerId = Guid.NewGuid(); // Or from User.Identity
            var table = await _gameService.JoinTableAsync(tableId, playerId, request.PlayerName, request.BuyIn);
            return Ok(new { TableId = table.Id, PlayerId = playerId });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPost("{tableId}/start")]
    public async Task<IActionResult> StartGame(Guid tableId)
    {
        try
        {
            await _gameService.StartGameAsync(tableId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{tableId}/action")]
    public async Task<IActionResult> PlayerAction(Guid tableId, [FromQuery] Guid playerId, [FromBody] PlayerActionRequest request)
    {
        try
        {
            var result = await _gameService.PlayerActionAsync(tableId, playerId, request.Action, request.Amount);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
