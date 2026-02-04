using Microsoft.AspNetCore.Mvc;
using PokerMultiplayerAPI.Domain.Interfaces;
using PokerMultiplayerAPI.Shared.DTOs;

namespace PokerMultiplayerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LobbyController : ControllerBase
{
    private readonly ITableRepository _tableRepository;

    public LobbyController(ITableRepository tableRepository)
    {
        _tableRepository = tableRepository;
    }

    [HttpGet]
    public IActionResult GetTables()
    {
        return Ok(_tableRepository.GetAllTables());
    }

    [HttpPost]
    public IActionResult CreateTable([FromBody] string tableName)
    {
        var table = _tableRepository.CreateTable(tableName);
        return Ok(table);
    }
}
