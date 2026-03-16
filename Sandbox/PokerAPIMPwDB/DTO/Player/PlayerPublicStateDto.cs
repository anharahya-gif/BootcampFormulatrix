using PokerAPIMPwDB.Domain.Enums;
using System;

namespace PokerAPIMPwDB.DTO.Player
{
    public class PlayerPublicStateDto
    {
        public Guid PlayerId { get; set; }
        public string DisplayName { get; set; }
        public int ChipStack { get; set; }
        public PlayerState State { get; set; }
        public int CurrentBet { get; set; }
        public int SeatIndex { get; set; }
        public bool IsDealer { get; set; } = false;
    }
}
