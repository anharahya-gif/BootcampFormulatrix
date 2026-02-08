using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.Domain.Models;
using System.Collections.Generic;

namespace PokerAPIMPwDB.DTO.Table
{
    public class TableStateDto
    {
        public Guid TableId { get; set; }
        public GamePhase Phase { get; set; }
        public int CurrentBet { get; set; }
        public List<Card> CommunityCards { get; set; } = new List<Card>();
        public List<PlayerPublicStateDto> Players { get; set; } = new List<PlayerPublicStateDto>();
    }
}
