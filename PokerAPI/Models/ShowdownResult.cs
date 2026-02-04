using System.Collections.Generic;
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Models
{
    public class ShowdownResult
    {
        public List<IPlayer> Winners { get; }
        public HandRank HandRank { get; }

        public ShowdownResult(List<IPlayer> winners, HandRank handRank)
        {
            Winners = winners;
            HandRank = handRank;
        }

        public string Message
        {
            get
            {
                if (Winners.Count == 1)
                    return $"{Winners[0].Name} wins with {HandRank}";
                else
                {
                    var names = string.Join(", ", Winners.Select(w => w.Name));
                    return $"It's a tie between {names} with {HandRank}";
                }
            }
        }
    }
}
