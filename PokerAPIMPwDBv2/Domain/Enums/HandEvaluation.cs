using PokerAPIMPwDB.Domain.Interfaces;
namespace PokerAPIMPwDB.Domain.Enums
{


    public class HandEvaluation
    {
        public HandRank Rank { get; set; }           // enum HighCard, Pair, dll
        public int Score { get; set; }               // untuk tie breaker
        public List<ICard> Cards { get; set; } = new(); // kartu terbaik
    }
}