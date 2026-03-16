using PokerAPIMPwDB.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface IPlayer
    {
        Guid PlayerId { get; }
        string DisplayName { get; }
        int ChipStack { get; set; }
        int CurrentBet { get; set; }
        PlayerState State { get;set; }
        int SeatIndex { get; set; }
        List<ICard> Cards { get; set; }


    }
}
