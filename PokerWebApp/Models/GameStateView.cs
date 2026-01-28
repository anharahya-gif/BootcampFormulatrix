using System.Collections.Generic;

namespace PokerWebApp.Models
{
    public class GameStateViewModel
    {
        public List<PlayerViewModel> Players { get; set; } = new();
    }
}
