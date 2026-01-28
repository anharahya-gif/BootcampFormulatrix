using System;

namespace PokerWebApp.Models
{
    public class PlayerViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public int Chips { get; set; }
    }
}
