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
        public string Name { get; set; } = string.Empty;
        public int MaxPlayers { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public TableState State { get; set; } = TableState.Waiting;

        public List<PlayerSeat> Seats { get; } = new();
        public IPokerGameEngine? Game { get; set; }

        public Table(int maxPlayers)
        {
            MaxPlayers = maxPlayers;

            // ✅ seat FIXED dari awal
            for (int i = 0; i < maxPlayers; i++)
            {
                Seats.Add(new PlayerSeat(i));
            }
        }

        public bool Join(IPlayer player, int seatIndex, int chips)
        {
            if (seatIndex < 0 || seatIndex >= MaxPlayers)
                return false;
            if (chips <= 0)
                return false;

            var seat = Seats[seatIndex];
            if (seat.IsOccupied)
                return false;

            seat.SitDown(player, chips);
            return true;
        }

        public void Leave(Guid playerId)
        {
            var seat = Seats.FirstOrDefault(s =>
                s.IsOccupied && s.Player!.PlayerId == playerId);

            seat?.Leave();
        }

        public bool CanStart() =>
            Seats.Count(s => s.IsOccupied) >= 2;

        public void StartGame()
        {
            if (!CanStart())
                throw new InvalidOperationException("Not enough players.");

            State = TableState.Playing;
            Game?.StartRound();
        }
    }
}
