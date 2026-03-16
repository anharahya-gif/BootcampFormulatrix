using PokerAPIMPwDB.Domain.Interfaces;

namespace PokerAPIMPwDB.Domain.Models
{
    public class Pot : IPot
    {
        public int TotalChips { get; private set; }

        public void AddChips(int amount)
        {
            TotalChips += amount;
        }

        public void Reset()
        {
            TotalChips = 0;
        }
    }
}
