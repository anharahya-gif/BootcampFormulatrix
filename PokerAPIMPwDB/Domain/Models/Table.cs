using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerAPIMPwDB.Domain.Models
{
    public class Table : ITable
    {
        public Guid TableId { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public TableState State { get; set; } = TableState.Waiting;

        public List<PlayerSeat> Seats { get; set; } = new List<PlayerSeat>();
        public IPokerGameEngine Game { get; set; }

        public bool Join(IPlayer player)
        {
            if (Seats.Count >= MaxPlayers) return false;
            Seats.Add(new PlayerSeat { SeatIndex = Seats.Count, Player = player });
            return true;
        }

        public void Leave(Guid playerId)
        {
            var seat = Seats.FirstOrDefault(s => s.Player.PlayerId == playerId);
            if (seat != null)
                Seats.Remove(seat);
        }


        public bool CanStart() => Seats.Count >= 2;

        public void StartGame()
        {
            if (!CanStart()) throw new InvalidOperationException("Not enough players.");
            State = TableState.Playing;
            Game?.StartRound();
        }
    }

}