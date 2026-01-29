using System.Collections.Generic;

namespace PokerAPI.Models
{
    public class ShowdownResult
    {
        public List<Player> Winners { get; }
        public HandRank HandRank { get; }

        public ShowdownResult(List<Player> winners, HandRank handRank)
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
