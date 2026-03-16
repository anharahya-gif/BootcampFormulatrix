using PokerAPIMPwDB.Domain.Models;
using PokerAPIMPwDB.Domain.Enums;
using System;
using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface ITable 
    {
        Guid TableId { get; }
        string Name { get; set; }
        int MaxPlayers { get; set; }
        int SmallBlind { get; set; }
        int BigBlind { get; set; }
        TableState State { get; set; }

        List<PlayerSeat> Seats { get; }
        IPokerGameEngine Game { get; set; }

        bool Join(IPlayer player, int seatIndex, int chips);
        void Leave(Guid playerId);
        bool CanStart();
        void StartGame();
    }
}
