using Microsoft.AspNetCore.Mvc;
using CobaWebAPI.Models;
using CobaWebAPI.Services;
using CobaWebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using CobaWebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace CobaWebAPI.Controllers
{
    [ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly AppDbContext _context;

    public TestController(AppDbContext context)
    {
        _context = context;
    }

   [HttpGet("db")]
public async Task<IActionResult> TestDb()
{
    try
    {
        await _context.Database.OpenConnectionAsync();
        await _context.Database.CloseConnectionAsync();

        return Ok(new { canConnect = true });
    }
    catch (Exception ex)
    {
        return BadRequest(new
        {
            message = ex.Message,
            inner = ex.InnerException?.Message,
            type = ex.GetType().FullName
        });
    }
}
}

}
