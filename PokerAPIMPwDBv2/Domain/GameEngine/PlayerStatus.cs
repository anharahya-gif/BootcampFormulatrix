using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;
using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Models
{
    public class PlayerStatus
    {
        public List<ICard> Hand { get; private set; } = new List<ICard>();
        public PlayerState State { get; set; }
        public int CurrentBet { get; set; }
        public bool HasActed { get; set; }

        public void Reset()
        {
            Hand.Clear();
            State = PlayerState.Active;
            CurrentBet = 0;
            HasActed = false;
        }
    }
}
