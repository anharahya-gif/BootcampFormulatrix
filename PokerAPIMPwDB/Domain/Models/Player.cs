using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;
using System;

namespace PokerAPIMPwDB.Domain.Models
{
    public class Player : IPlayer
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; }
        public int ChipStack { get; set; }
        public int CurrentBet { get; set; }
        public PlayerState State { get; set; } = PlayerState.Active;
        public int SeatIndex { get; set; }
        public List<ICard> Cards { get; set; } = new();

    }
}
