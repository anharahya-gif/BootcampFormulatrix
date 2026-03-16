using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;
using System.Collections.Generic;

namespace PokerAPIMPwDB.Domain.Models
{
    public class ShowdownResult
    {
        public List<IPlayer> Winners { get; set; } = new();
        public List<ICard> CommunityCards { get; set; } = new();
        public Dictionary<IPlayer, List<ICard>> PlayerHands { get; set; } = new();
        public HandRank BestHandRank { get; set; } = HandRank.HighCard;
    }
}
