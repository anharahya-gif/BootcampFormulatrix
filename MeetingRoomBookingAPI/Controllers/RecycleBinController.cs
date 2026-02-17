using System;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RecycleBinController : ControllerBase
    {
        private readonly IRecycleBinService _recycleBinService;

        public RecycleBinController(IRecycleBinService recycleBinService)
        {
            _recycleBinService = recycleBinService;
        }

        [HttpGet]
        public async Task<IActionResult> GetDeletedItems()
        {
            var result = await _recycleBinService.GetDeletedItemsAsync();
            return Ok(result);
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> RestoreItem(Guid id, [FromQuery] string type)
        {
            var result = await _recycleBinService.RestoreItemAsync(id, type);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> HardDelete(Guid id, [FromQuery] string type)
        {
            var result = await _recycleBinService.HardDeleteItemAsync(id, type);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }
    }
}
