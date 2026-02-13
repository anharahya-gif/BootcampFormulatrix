using System;
using PokerAPIMPwDB.Domain.Enums;
namespace PokerAPIMPwDB.Infrastructure.Persistence.Entities
{
    public class Player
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Hubungan ke User
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string DisplayName { get; set; } = null!;
        public int ChipStack { get; set; } = 0;
        public int ChipsWonThisRound { get; set; } = 0;
        public bool isDeleted {get;set;}=false;

        // PlayerState
        public PlayerState State { get; set; } = PlayerState.Active;

        // Hubungan ke PlayerSeat
        public PlayerSeat? PlayerSeat { get; set; }
    }
}
