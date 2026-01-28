using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PokerWebApp.Models;
using PokerWebApp.Services;

namespace PokerWebApp.Controllers
{
    public class PokerController : Controller
    {
        private readonly PokerApiClient _api;

        public PokerController(PokerApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            var state = await _api.GetGameState();
            return View(state);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayer(string name, int chips)
        {
            await _api.AddPlayer(name, chips);
            return RedirectToAction(nameof(Index));
        }
    }
}
